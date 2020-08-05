using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace PetaPoco
{
#pragma warning disable 1066,1570,1573,1591

    /// <summary>
    ///     The main PetaPoco Database class.  You can either use this class directly, or derive from it.
    /// </summary>
    public class Database : IDatabase
    {
        /// <summary>
        /// Bulk inserts multiple rows to SQL
        /// </summary>
        /// <param name="tableName">The name of the table to insert into</param>
        /// <param name="primaryKeyName">The name of the primary key column of the table</param>
        /// <param name="autoIncrement">True if the primary key is automatically allocated by the DB</param>
        /// <param name="pocos">The POCO objects that specifies the column values to be inserted</param>
        /// <param name="batchSize">The number of POCOS to be grouped together for each database rounddtrip</param>        
        public void BulkInsert(string tableName, string primaryKeyName, bool autoIncrement, IEnumerable<object> pocos, int batchSize = 25)
        {
            try
            {
                OpenSharedConnection();
                try
                {
                    using (var cmd = CreateCommand(_sharedConnection, ""))
                    {
                        var pd = PocoData.ForObject(pocos.First().GetType(), primaryKeyName, _defaultMapper);
                        // Create list of columnnames only once
                        var names = new List<string>();
                        foreach (var i in pd.Columns)
                        {
                            // Don't insert result columns
                            if (i.Value.ResultColumn)
                                continue;

                            // Don't insert the primary key (except under oracle where we need bring in the next sequence value)
                            if (autoIncrement && primaryKeyName != null && string.Compare(i.Key, primaryKeyName, true) == 0)
                            {
                                // Setup auto increment expression
                                string autoIncExpression = _provider.GetAutoIncrementExpression(pd.TableInfo);
                                if (autoIncExpression != null)
                                {
                                    names.Add(i.Key);
                                }
                                continue;
                            }
                            names.Add(_provider.EscapeSqlIdentifier(i.Key));
                        }
                        var namesArray = names.ToArray();

                        var values = new List<string>();
                        int count = 0;
                        do
                        {
                            cmd.CommandText = "";
                            cmd.Parameters.Clear();
                            var index = 0;
                            foreach (var poco in pocos.Skip(count).Take(batchSize))
                            {
                                values.Clear();
                                foreach (var i in pd.Columns)
                                {
                                    // Don't insert result columns
                                    if (i.Value.ResultColumn) continue;

                                    // Don't insert the primary key (except under oracle where we need bring in the next sequence value)
                                    if (autoIncrement && primaryKeyName != null && string.Compare(i.Key, primaryKeyName, true) == 0)
                                    {
                                        // Setup auto increment expression
                                        string autoIncExpression = _provider.GetAutoIncrementExpression(pd.TableInfo);
                                        if (autoIncExpression != null)
                                        {
                                            values.Add(autoIncExpression);
                                        }
                                        continue;
                                    }

                                    values.Add(string.Format("{0}{1}", _paramPrefix, index++));
                                    AddParam(cmd, i.Value.GetValue(poco), i.Value.PropertyInfo);
                                }

                                string outputClause = String.Empty;
                                if (autoIncrement)
                                {
                                    outputClause = _provider.GetInsertOutputClause(primaryKeyName);
                                }

                                cmd.CommandText += string.Format("INSERT INTO {0} ({1}){2} VALUES ({3})", _provider.EscapeTableName(tableName),
                                                                 string.Join(",", namesArray), outputClause, string.Join(",", values.ToArray()));
                            }
                            // Are we done?
                            if (cmd.CommandText == "") break;
                            count += batchSize;
                            DoPreExecute(cmd);
                            cmd.ExecuteNonQuery();
                            OnExecutedCommand(cmd);
                        }
                        while (true);

                    }
                }
                finally
                {
                    CloseSharedConnection();
                }
            }
            catch (Exception x)
            {
                if (OnException(x))
                    throw;
            }
        }


        /// <summary>
        /// Performs a SQL Bulk Insert
        /// </summary>
        /// <param name="pocos">The POCO objects that specifies the column values to be inserted</param>        
        /// <param name="batchSize">The number of POCOS to be grouped together for each database rounddtrip</param>        
        public void Insert(IEnumerable<object> pocos, int batchSize = 25)
        {
            if (!pocos.Any()) return;
            var pd = PocoData.ForType(pocos.First().GetType(), _defaultMapper);
            BulkInsert(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, pd.TableInfo.AutoIncrement, pocos);
        }


        #region 获取datatable
        public DataTable GetDataTable(Sql sql)
        {
            return GetDataTable(sql.SQL, sql.Arguments);
        }
        public DataTable GetDataTable(string sql, params object[] args)
        {
            try
            {
                OpenSharedConnection();
                try
                {
                    using (var cmd = CreateCommand(_sharedConnection, sql, args))
                    {
                        var val = cmd.ExecuteReader();
                        OnExecutedCommand(cmd);
                        var dt = new DataTable();
                        dt.Load(val);
                        return dt; //(T)Convert.ChangeType(val, typeof(T));
                    }
                }
                finally
                {
                    CloseSharedConnection();
                }
            }
            catch (Exception x)
            {
                OnException(x);
                throw;
            }
        }


        #endregion

        #region IDisposable

        /// <summary>
        /// 自动关闭共享连接    Automatically close one open shared connection
        /// </summary>
        public void Dispose()
        {
            // Automatically close one open connection reference
            //  (Works with KeepConnectionAlive and manually opening a shared connection)
            CloseSharedConnection();
        }

        #endregion

        #region 构造函数 Constructors

        /// <summary>
        ///     Constructs an instance using the first connection string found in the app/web configuration file.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when no connection strings can registered.</exception>
        public Database()
        {
            if (ConfigurationManager.ConnectionStrings.Count == 0)
                throw new InvalidOperationException("One or more connection strings must be registered to use the no paramater constructor");

            var entry = ConfigurationManager.ConnectionStrings[0];
            _connectionString = entry.ConnectionString;
            var providerName = !string.IsNullOrEmpty(entry.ProviderName) ? entry.ProviderName : "System.Data.SqlClient";
            Initialise(DatabaseProvider.Resolve(providerName, false, _connectionString), null);
        }

        /// <summary>
        ///     Constructs an instance using a supplied IDbConnection.
        /// </summary>
        /// <param name="connection">The IDbConnection to use.</param>
        /// <remarks>
        ///     The supplied IDbConnection will not be closed/disposed by PetaPoco - that remains
        ///     the responsibility of the caller.
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown when <paramref name="connection" /> is null or empty.</exception>
        public Database(IDbConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");

            _sharedConnection = connection;
            _connectionString = connection.ConnectionString;
            // Prevent closing external connection
            _sharedConnectionDepth = 2;

            Initialise(DatabaseProvider.Resolve(_sharedConnection.GetType(), false, _connectionString), null);
        }

        /// <summary>
        ///     Constructs an instance using a supplied connections string and optionally a provider name. If no provider name is
        ///     given, the default database provider will be MS SQL Server.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="providerName">The database provider name, if given.</param>
        /// <remarks>
        ///     PetaPoco will automatically close and dispose any connections it creates.
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown when <paramref name="connectionString" /> is null or empty.</exception>
        public Database(string connectionString, string providerName = null)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty", "connectionString");

            _connectionString = connectionString;
            Initialise(DatabaseProvider.Resolve(providerName, true, _connectionString), null);
        }

        /// <summary>
        ///     Constructs an instance using the supplied connection string and DbProviderFactory.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="factory">The DbProviderFactory to use for instantiating IDbConnection's.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="connectionString" /> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory" /> is null.</exception>
        public Database(string connectionString, DbProviderFactory factory)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Connection string must not be null or empty", "connectionString");

            if (factory == null)
                throw new ArgumentNullException("factory");

            _connectionString = connectionString;
            Initialise(DatabaseProvider.Resolve(DatabaseProvider.Unwrap(factory).GetType(), false, _connectionString), null);
        }

        /// <summary>
        ///     Constructs an instance using a supplied connection string name. The actual connection string and provider will be
        ///     read from app/web.config.
        /// </summary>
        /// <param name="connectionStringName">The name of the connection.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="connectionStringName" /> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when a connection string cannot be found.</exception>
        public Database(string connectionStringName)
        {
            if (string.IsNullOrEmpty(connectionStringName))
                throw new ArgumentException("Connection string name must not be null or empty", "connectionStringName");

            //var entry = ConfigurationManager.ConnectionStrings[connectionStringName];

            //if (entry == null)
            //    throw new InvalidOperationException(string.Format("Can't find a connection string with the name '{0}'", connectionStringName));

            _connectionString = connectionStringName;
            var providerName = "System.Data.SqlClient";
            Initialise(DatabaseProvider.Resolve(providerName, false, _connectionString), null);
        }

        /// <summary>
        ///     Constructs an instance using the supplied provider and optional default mapper.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="provider">The provider to use.</param>
        /// <param name="defaultMapper">The default mapper to use when no specific mapper has been registered.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="connectionString" /> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="provider" /> is null.</exception>
        public Database(string connectionString, IProvider provider, IMapper defaultMapper = null)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Connection string must not be null or empty", "connectionString");

            if (provider == null)
                throw new ArgumentNullException("provider");

            _connectionString = connectionString;
            Initialise(provider, defaultMapper);
        }

        /// <summary>
        ///     Constructs an instance using the supplied <paramref name="configuration"/>.
        /// </summary>
        /// <param name="configuration">The configuration for constructing an instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration" /> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when no configuration string is configured and app/web config does any connection string registered.</exception>
        /// <exception cref="InvalidOperationException">Thrown when a connection string configured and no provider is configured.</exception>
        public Database(IDatabaseBuildConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            var settings = (IBuildConfigurationSettings)configuration;

            IMapper defaultMapper = null;
            settings.TryGetSetting<IMapper>(DatabaseConfigurationExtensions.DefaultMapper, v => defaultMapper = v);

            ConnectionStringSettings entry = null;
            settings.TryGetSetting<string>(DatabaseConfigurationExtensions.ConnectionString, cs => _connectionString = cs, () =>
            {
                settings.TryGetSetting<string>(DatabaseConfigurationExtensions.ConnectionStringName, cn =>
                {
                    entry = ConfigurationManager.ConnectionStrings[cn];

                    if (entry == null)
                        throw new InvalidOperationException(string.Format("Can't find a connection string with the name '{0}'", cn));
                }, () =>
                {
                    if (ConfigurationManager.ConnectionStrings.Count == 0)
                        throw new InvalidOperationException("One or more connection strings must be registered to not configure the connection string");

                    entry = ConfigurationManager.ConnectionStrings[0];
                });

                // ReSharper disable once PossibleNullReferenceException
                _connectionString = entry.ConnectionString;
            });

            settings.TryGetSetting<IProvider>(DatabaseConfigurationExtensions.Provider, v => Initialise(v, defaultMapper), () =>
            {
                if (entry == null)
                    throw new InvalidOperationException("Both a connection string and provider are required or neither.");

                var providerName = !string.IsNullOrEmpty(entry.ProviderName) ? entry.ProviderName : "System.Data.SqlClient";
                Initialise(DatabaseProvider.Resolve(providerName, false, _connectionString), defaultMapper);
            });

            settings.TryGetSetting<bool>(DatabaseConfigurationExtensions.EnableNamedParams, v => EnableNamedParams = v);
            settings.TryGetSetting<bool>(DatabaseConfigurationExtensions.EnableAutoSelect, v => EnableAutoSelect = v);
            settings.TryGetSetting<int>(DatabaseConfigurationExtensions.CommandTimeout, v => CommandTimeout = v);
            settings.TryGetSetting<IsolationLevel>(DatabaseConfigurationExtensions.IsolationLevel, v => IsolationLevel = v);
        }

        /// <summary>
        ///     Provides common initialization for the various constructors.
        /// </summary>
        private void Initialise(IProvider provider, IMapper mapper)
        {
            // Reset
            _transactionDepth = 0;
            EnableAutoSelect = true;
            EnableNamedParams = true;

            // What character is used for delimiting parameters in SQL
            _provider = provider;
            _paramPrefix = _provider.GetParameterPrefix(_connectionString);
            _factory = _provider.GetFactory();

            _defaultMapper = mapper ?? new ConventionMapper();
        }

        #endregion

        #region 连接管理 Connection Management

        /// <summary>
        ///     When set to true the first opened connection is kept alive until this object is disposed
        /// </summary>
        public bool KeepConnectionAlive { get; set; }

        /// <summary>
        ///     Open a connection that will be used for all subsequent queries.
        /// </summary>
        /// <remarks>
        ///     Calls to Open/CloseSharedConnection are reference counted and should be balanced
        /// </remarks>
        public void OpenSharedConnection()
        {
            if (_sharedConnectionDepth == 0)
            {
                _sharedConnection = _factory.CreateConnection();
                _sharedConnection.ConnectionString = _connectionString;

                if (_sharedConnection.State == ConnectionState.Broken)
                    _sharedConnection.Close();

                if (_sharedConnection.State == ConnectionState.Closed)
                    _sharedConnection.Open();

                _sharedConnection = OnConnectionOpened(_sharedConnection);

                if (KeepConnectionAlive)
                    _sharedConnectionDepth++; // Make sure you call Dispose
            }
            _sharedConnectionDepth++;
        }

        /// <summary>
        ///     Releases the shared connection
        /// </summary>
        public void CloseSharedConnection()
        {
            if (_sharedConnectionDepth > 0)
            {
                _sharedConnectionDepth--;
                if (_sharedConnectionDepth == 0)
                {
                    OnConnectionClosing(_sharedConnection);
                    _sharedConnection.Dispose();
                    _sharedConnection = null;
                }
            }
        }

        /// <summary>
        ///     Provides access to the currently open shared connection (or null if none)
        /// </summary>
        public IDbConnection Connection
        {
            get { return _sharedConnection; }
        }

        #endregion

        #region 事务管理 Transaction Management

        /// <summary>
        /// 当前事务实例    Gets the current transaction instance.
        /// </summary>
        /// <returns>
        ///     The current transaction instance; else, <c>null</c> if not transaction is in progress.
        /// </returns>
        IDbTransaction ITransactionAccessor.Transaction
        {
            get { return _transaction; }
        }

        // Helper to create a transaction scope

        /// <summary>
        ///     Starts or continues a transaction.
        /// </summary>
        /// <returns>An ITransaction reference that must be Completed or disposed</returns>
        /// <remarks>
        ///     This method makes management of calls to Begin/End/CompleteTransaction easier.
        ///     The usage pattern for this should be:
        ///     using (var tx = db.GetTransaction())
        ///     {
        ///     // Do stuff
        ///     db.Update(...);
        ///     // Mark the transaction as complete
        ///     tx.Complete();
        ///     }
        ///     Transactions can be nested but they must all be completed otherwise the entire
        ///     transaction is aborted.
        /// </remarks>
        public ITransaction GetTransaction()
        {
            return new Transaction(this);
        }

        /// <summary>
        ///     Called when a transaction starts.  Overridden by the T4 template generated database
        ///     classes to ensure the same DB instance is used throughout the transaction.
        /// </summary>
        public virtual void OnBeginTransaction()
        {
        }

        /// <summary>
        ///     Called when a transaction ends.
        /// </summary>
        public virtual void OnEndTransaction()
        {
        }

        /// <summary>
        ///     Starts a transaction scope, see GetTransaction() for recommended usage
        /// </summary>
        public void BeginTransaction()
        {
            _transactionDepth++;

            if (_transactionDepth == 1)
            {
                OpenSharedConnection();
                _transaction = !_isolationLevel.HasValue ? _sharedConnection.BeginTransaction() : _sharedConnection.BeginTransaction(_isolationLevel.Value);
                _transactionCancelled = false;
                OnBeginTransaction();
            }
        }

        /// <summary>
        ///     Internal helper to cleanup transaction
        /// </summary>
        private void CleanupTransaction()
        {
            OnEndTransaction();

            if (_transactionCancelled)
                _transaction.Rollback();
            else
                _transaction.Commit();

            _transaction.Dispose();
            _transaction = null;

            CloseSharedConnection();
        }

        /// <summary>
        ///     Aborts the entire outer most transaction scope
        /// </summary>
        /// <remarks>
        ///     Called automatically by Transaction.Dispose()
        ///     if the transaction wasn't completed.
        /// </remarks>
        public void AbortTransaction()
        {
            _transactionCancelled = true;
            if ((--_transactionDepth) == 0)
                CleanupTransaction();
        }

        /// <summary>
        ///     Marks the current transaction scope as complete.
        /// </summary>
        public void CompleteTransaction()
        {
            if ((--_transactionDepth) == 0)
                CleanupTransaction();
        }

        #endregion

        #region Command Management

        /// <summary>
        /// 添加参数到Db     Add a parameter to a DB command
        /// </summary>
        /// <param name="cmd">A reference to the IDbCommand to which the parameter is to be added</param>
        /// <param name="value">The value to assign to the parameter</param>
        /// <param name="pi">Optional, a reference to the property info of the POCO property from which the value is coming.</param>
        private void AddParam(IDbCommand cmd, object value, PropertyInfo pi)
        {
            // Convert value to from poco type to db type
            if (pi != null)
            {
                var mapper = Mappers.GetMapper(pi.DeclaringType, _defaultMapper);
                var fn = mapper.GetToDbConverter(pi);
                if (fn != null)
                    value = fn(value);
            }

            // Support passed in parameters
            var idbParam = value as IDbDataParameter;
            if (idbParam != null)
            {
                idbParam.ParameterName = string.Format("{0}{1}", _paramPrefix, cmd.Parameters.Count);
                cmd.Parameters.Add(idbParam);
                return;
            }

            // Create the parameter
            var p = cmd.CreateParameter();
            p.ParameterName = string.Format("{0}{1}", _paramPrefix, cmd.Parameters.Count);

            // Assign the parmeter value
            if (value == null)
            {
                p.Value = DBNull.Value;

                if (pi != null && pi.PropertyType.Name == "Byte[]")
                {
                    p.DbType = DbType.Binary;
                }
            }
            else
            {
                // Give the database type first crack at converting to DB required type
                value = _provider.MapParameterValue(value);

                var t = value.GetType();
                if (t.IsEnum) // PostgreSQL .NET driver wont cast enum to int
                {
                    p.Value = Convert.ChangeType(value, ((Enum)value).GetTypeCode());
                }
                else if (t == typeof(Guid) && !_provider.HasNativeGuidSupport)
                {
                    p.Value = value.ToString();
                    p.DbType = DbType.String;
                    p.Size = 40;
                }
                else if (t == typeof(string))
                {
                    // out of memory exception occurs if trying to save more than 4000 characters to SQL Server CE NText column. Set before attempting to set Size, or Size will always max out at 4000
                    if ((value as string).Length + 1 > 4000 && p.GetType().Name == "SqlCeParameter")
                        p.GetType().GetProperty("SqlDbType").SetValue(p, SqlDbType.NText, null);

                    p.Size = Math.Max((value as string).Length + 1, 4000); // Help query plan caching by using common size
                    p.Value = value;
                }
                else if (t == typeof(AnsiString))
                {
                    // Thanks @DataChomp for pointing out the SQL Server indexing performance hit of using wrong string type on varchar
                    p.Size = Math.Max((value as AnsiString).Value.Length + 1, 4000);
                    p.Value = (value as AnsiString).Value;
                    p.DbType = DbType.AnsiString;
                }
                else if (value.GetType().Name == "SqlGeography") //SqlGeography is a CLR Type
                {
                    p.GetType().GetProperty("UdtTypeName").SetValue(p, "geography", null); //geography is the equivalent SQL Server Type
                    p.Value = value;
                }
                else if (value.GetType().Name == "SqlGeometry") //SqlGeometry is a CLR Type
                {
                    p.GetType().GetProperty("UdtTypeName").SetValue(p, "geometry", null); //geography is the equivalent SQL Server Type
                    p.Value = value;
                }
                else
                {
                    p.Value = value;
                }
            }

            // Add to the collection
            cmd.Parameters.Add(p);
        }

        // Create a command
        private static Regex rxParamsPrefix = new Regex(@"(?<!@)@\w+", RegexOptions.Compiled);

        public IDbCommand CreateCommand(IDbConnection connection, string sql, params object[] args)
        {
            // Perform named argument replacements
            if (EnableNamedParams)
            {
                var new_args = new List<object>();
                sql = ParametersHelper.ProcessParams(sql, args, new_args);
                args = new_args.ToArray();
            }

            // Perform parameter prefix replacements
            if (_paramPrefix != "@")
                sql = rxParamsPrefix.Replace(sql, m => _paramPrefix + m.Value.Substring(1));
            sql = sql.Replace("@@", "@"); // <- double @@ escapes a single @

            // Create the command and add parameters
            IDbCommand cmd = connection.CreateCommand();
            cmd.Connection = connection;
            cmd.CommandText = sql;
            cmd.Transaction = _transaction;
            foreach (var item in args)
            {
                AddParam(cmd, item, null);
            }

            // Notify the DB type
            _provider.PreExecute(cmd);

            // Call logging
            if (!String.IsNullOrEmpty(sql))
                DoPreExecute(cmd);

            return cmd;
        }

        #endregion

        #region Exception Reporting and Logging

        /// <summary>
        ///     Called if an exception occurs during processing of a DB operation.  Override to provide custom logging/handling.
        /// </summary>
        /// <param name="x">The exception instance</param>
        /// <returns>True to re-throw the exception, false to suppress it</returns>
        public virtual bool OnException(Exception x)
        {
            System.Diagnostics.Debug.WriteLine(x.ToString());
            System.Diagnostics.Debug.WriteLine(LastCommand);
            return true;
        }

        /// <summary>
        ///     Called when DB connection opened
        /// </summary>
        /// <param name="conn">The newly opened IDbConnection</param>
        /// <returns>The same or a replacement IDbConnection</returns>
        /// <remarks>
        ///     Override this method to provide custom logging of opening connection, or
        ///     to provide a proxy IDbConnection.
        /// </remarks>
        public virtual IDbConnection OnConnectionOpened(IDbConnection conn)
        {
            return conn;
        }

        /// <summary>
        ///     Called when DB connection closed
        /// </summary>
        /// <param name="conn">The soon to be closed IDBConnection</param>
        public virtual void OnConnectionClosing(IDbConnection conn)
        {
        }

        /// <summary>
        ///     Called just before an DB command is executed
        /// </summary>
        /// <param name="cmd">The command to be executed</param>
        /// <remarks>
        ///     Override this method to provide custom logging of commands and/or
        ///     modification of the IDbCommand before it's executed
        /// </remarks>
        public virtual void OnExecutingCommand(IDbCommand cmd)
        {
        }

        /// <summary>
        ///     Called on completion of command execution
        /// </summary>
        /// <param name="cmd">The IDbCommand that finished executing</param>
        public virtual void OnExecutedCommand(IDbCommand cmd)
        {
        }

        #endregion

        #region 执行SQL语句 operation: Execute

        /// <summary>
        ///     Executes a non-query command
        /// </summary>
        /// <param name="sql">The SQL statement to execute</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>The number of rows affected</returns>
        public int Execute(string sql, params object[] args)
        {
            try
            {
                OpenSharedConnection();
                try
                {
                    using (var cmd = CreateCommand(_sharedConnection, sql, args))
                    {
                        var retv = cmd.ExecuteNonQuery();
                        OnExecutedCommand(cmd);
                        return retv;
                    }
                }
                finally
                {
                    CloseSharedConnection();
                }
            }
            catch (Exception x)
            {
                if (OnException(x))
                    throw;
                return -1;
            }
        }

        /// <summary>
        ///     Executes a non-query command
        /// </summary>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>The number of rows affected</returns>
        public int Execute(Sql sql)
        {
            return Execute(sql.SQL, sql.Arguments);
        }

        #endregion

        #region operation: ExecuteScalar

        /// <summary>
        ///     Executes a query and return the first column of the first row in the result set.
        /// </summary>
        /// <typeparam name="T">The type that the result value should be cast to</typeparam>
        /// <param name="sql">The SQL query to execute</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>The scalar value cast to T</returns>
        public T ExecuteScalar<T>(string sql, params object[] args)
        {
            try
            {
                OpenSharedConnection();
                try
                {
                    using (var cmd = CreateCommand(_sharedConnection, sql, args))
                    {
                        object val = cmd.ExecuteScalar();
                        OnExecutedCommand(cmd);

                        // Handle nullable types
                        Type u = Nullable.GetUnderlyingType(typeof(T));
                        if (u != null && (val == null || val == DBNull.Value))
                            return default(T);

                        return (T)Convert.ChangeType(val, u == null ? typeof(T) : u);
                    }
                }
                finally
                {
                    CloseSharedConnection();
                }
            }
            catch (Exception x)
            {
                if (OnException(x))
                    throw;
                return default(T);
            }
        }

        /// <summary>
        ///     Executes a query and return the first column of the first row in the result set.
        /// </summary>
        /// <typeparam name="T">The type that the result value should be cast to</typeparam>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>The scalar value cast to T</returns>
        public T ExecuteScalar<T>(Sql sql)
        {
            return ExecuteScalar<T>(sql.SQL, sql.Arguments);
        }

        #endregion

        #region 获取列表 operation: Fetch

        /// <summary>
        ///     Runs a query and returns the result set as a typed list
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="sql">The SQL query to execute</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A List holding the results of the query</returns>
        public List<T> Fetch<T>(string sql, params object[] args)
        {
            return Query<T>(sql, args).ToList();
        }

        /// <summary>
        ///     Runs a query and returns the result set as a typed list
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A List holding the results of the query</returns>
        public List<T> Fetch<T>(Sql sql)
        {
            return Fetch<T>(sql.SQL, sql.Arguments);
        }

        #endregion

        #region 分页操作 operation: Page

        /// <summary>
        ///     Starting with a regular SELECT statement, derives the SQL statements required to query a
        ///     DB for a page of records and the total number of records
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="skip">The number of rows to skip before the start of the page</param>
        /// <param name="take">The number of rows in the page</param>
        /// <param name="sql">The original SQL select statement</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <param name="sqlCount">Outputs the SQL statement to query for the total number of matching rows</param>
        /// <param name="sqlPage">Outputs the SQL statement to retrieve a single page of matching rows</param>
        private void BuildPageQueries<T>(long skip, long take, string sql, ref object[] args, out string sqlCount, out string sqlPage)
        {
            // Add auto select clause
            if (EnableAutoSelect)
                sql = AutoSelectHelper.AddSelectClause<T>(_provider, sql, _defaultMapper);

            // Split the SQL
            SQLParts parts;
            if (!Provider.PagingUtility.SplitSQL(sql, out parts))
                throw new Exception("Unable to parse SQL statement for paged query");

            sqlPage = _provider.BuildPageQuery(skip, take, parts, ref args);
            sqlCount = parts.SqlCount;
        }

        /// <summary>
        ///     Retrieves a page of records	and the total number of available records
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="page">The 1 based page number to retrieve</param>
        /// <param name="itemsPerPage">The number of records per page</param>
        /// <param name="sqlCount">The SQL to retrieve the total number of records</param>
        /// <param name="countArgs">Arguments to any embedded parameters in the sqlCount statement</param>
        /// <param name="sqlPage">The SQL To retrieve a single page of results</param>
        /// <param name="pageArgs">Arguments to any embedded parameters in the sqlPage statement</param>
        /// <returns>A Page of results</returns>
        /// <remarks>
        ///     This method allows separate SQL statements to be explicitly provided for the two parts of the page query.
        ///     The page and itemsPerPage parameters are not used directly and are used simply to populate the returned Page
        ///     object.
        /// </remarks>
        public Page<T> Page<T>(long page, long itemsPerPage, string sqlCount, object[] countArgs, string sqlPage, object[] pageArgs)
        {
            // Save the one-time command time out and use it for both queries
            var saveTimeout = OneTimeCommandTimeout;

            // Setup the paged result
            var result = new Page<T>
            {
                CurrentPage = page,
                ItemsPerPage = itemsPerPage,
                TotalItems = ExecuteScalar<long>(sqlCount, countArgs)
            };
            result.TotalPages = result.TotalItems / itemsPerPage;

            if ((result.TotalItems % itemsPerPage) != 0)
                result.TotalPages++;

            OneTimeCommandTimeout = saveTimeout;

            // Get the records
            result.Items = Fetch<T>(sqlPage, pageArgs);

            // Done
            return result;
        }





        /// <summary>
        ///     Retrieves a page of records	and the total number of available records
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="page">The 1 based page number to retrieve</param>
        /// <param name="itemsPerPage">The number of records per page</param>
        /// <param name="sql">The base SQL query</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL statement</param>
        /// <returns>A Page of results</returns>
        /// <remarks>
        ///     PetaPoco will automatically modify the supplied SELECT statement to only retrieve the
        ///     records for the specified page.  It will also execute a second query to retrieve the
        ///     total number of records in the result set.
        /// </remarks>
        public Page<T> Page<T>(long page, long itemsPerPage, string sql, params object[] args)
        {
            string sqlCount, sqlPage;
            BuildPageQueries<T>((page - 1) * itemsPerPage, itemsPerPage, sql, ref args, out sqlCount, out sqlPage);
            return Page<T>(page, itemsPerPage, sqlCount, args, sqlPage, args);
        }

        /// <summary>
        ///     Retrieves a page of records	and the total number of available records
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="page">The 1 based page number to retrieve</param>
        /// <param name="itemsPerPage">The number of records per page</param>
        /// <param name="sql">An SQL builder object representing the base SQL query and it's arguments</param>
        /// <returns>A Page of results</returns>
        /// <remarks>
        ///     PetaPoco will automatically modify the supplied SELECT statement to only retrieve the
        ///     records for the specified page.  It will also execute a second query to retrieve the
        ///     total number of records in the result set.
        /// </remarks>
        public Page<T> Page<T>(long page, long itemsPerPage, Sql sql)
        {
            return Page<T>(page, itemsPerPage, sql.SQL, sql.Arguments);
        }

        /// <summary>
        ///     Retrieves a page of records	and the total number of available records
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="page">The 1 based page number to retrieve</param>
        /// <param name="itemsPerPage">The number of records per page</param>
        /// <param name="sqlCount">An SQL builder object representing the SQL to retrieve the total number of records</param>
        /// <param name="sqlPage">An SQL builder object representing the SQL to retrieve a single page of results</param>
        /// <returns>A Page of results</returns>
        /// <remarks>
        ///     This method allows separate SQL statements to be explicitly provided for the two parts of the page query.
        ///     The page and itemsPerPage parameters are not used directly and are used simply to populate the returned Page
        ///     object.
        /// </remarks>
        public Page<T> Page<T>(long page, long itemsPerPage, Sql sqlCount, Sql sqlPage)
        {
            return Page<T>(page, itemsPerPage, sqlCount.SQL, sqlCount.Arguments, sqlPage.SQL, sqlPage.Arguments);
        }

        #endregion

        #region 分页列表 operation: Fetch (page)

        /// <summary>
        ///     Retrieves a page of records (without the total count)
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="page">The 1 based page number to retrieve</param>
        /// <param name="itemsPerPage">The number of records per page</param>
        /// <param name="sql">The base SQL query</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL statement</param>
        /// <returns>A List of results</returns>
        /// <remarks>
        ///     PetaPoco will automatically modify the supplied SELECT statement to only retrieve the
        ///     records for the specified page.
        /// </remarks>
        public List<T> Fetch<T>(long page, long itemsPerPage, string sql, params object[] args)
        {
            return SkipTake<T>((page - 1) * itemsPerPage, itemsPerPage, sql, args);
        }

        /// <summary>
        ///     Retrieves a page of records (without the total count)
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="page">The 1 based page number to retrieve</param>
        /// <param name="itemsPerPage">The number of records per page</param>
        /// <param name="sql">An SQL builder object representing the base SQL query and it's arguments</param>
        /// <returns>A List of results</returns>
        /// <remarks>
        ///     PetaPoco will automatically modify the supplied SELECT statement to only retrieve the
        ///     records for the specified page.
        /// </remarks>
        public List<T> Fetch<T>(long page, long itemsPerPage, Sql sql)
        {
            return SkipTake<T>((page - 1) * itemsPerPage, itemsPerPage, sql.SQL, sql.Arguments);
        }

        #endregion

        #region operation: SkipTake

        /// <summary>
        ///     Retrieves a range of records from result set
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="skip">The number of rows at the start of the result set to skip over</param>
        /// <param name="take">The number of rows to retrieve</param>
        /// <param name="sql">The base SQL query</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL statement</param>
        /// <returns>A List of results</returns>
        /// <remarks>
        ///     PetaPoco will automatically modify the supplied SELECT statement to only retrieve the
        ///     records for the specified range.
        /// </remarks>
        public List<T> SkipTake<T>(long skip, long take, string sql, params object[] args)
        {
            string sqlCount, sqlPage;
            BuildPageQueries<T>(skip, take, sql, ref args, out sqlCount, out sqlPage);
            return Fetch<T>(sqlPage, args);
        }

        /// <summary>
        ///     Retrieves a range of records from result set
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="skip">The number of rows at the start of the result set to skip over</param>
        /// <param name="take">The number of rows to retrieve</param>
        /// <param name="sql">An SQL builder object representing the base SQL query and it's arguments</param>
        /// <returns>A List of results</returns>
        /// <remarks>
        ///     PetaPoco will automatically modify the supplied SELECT statement to only retrieve the
        ///     records for the specified range.
        /// </remarks>
        public List<T> SkipTake<T>(long skip, long take, Sql sql)
        {
            return SkipTake<T>(skip, take, sql.SQL, sql.Arguments);
        }

        #endregion

        #region operation: Query

        /// <summary>
        ///     Runs an SQL query, returning the results as an IEnumerable collection
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="sql">The SQL query</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL statement</param>
        /// <returns>An enumerable collection of result records</returns>
        /// <remarks>
        ///     For some DB providers, care should be taken to not start a new Query before finishing with
        ///     and disposing the previous one. In cases where this is an issue, consider using Fetch which
        ///     returns the results as a List rather than an IEnumerable.
        /// </remarks>
        public IEnumerable<T> Query<T>(string sql, params object[] args)
        {
            if (EnableAutoSelect)
                sql = AutoSelectHelper.AddSelectClause<T>(_provider, sql, _defaultMapper);

            OpenSharedConnection();
            try
            {
                using (var cmd = CreateCommand(_sharedConnection, sql, args))
                {
                    IDataReader r;
                    var pd = PocoData.ForType(typeof(T), _defaultMapper);
                    try
                    {
                        r = cmd.ExecuteReader();
                        OnExecutedCommand(cmd);
                    }
                    catch (Exception x)
                    {
                        if (OnException(x))
                            throw;
                        yield break;
                    }
                    var factory = pd.GetFactory(cmd.CommandText, _sharedConnection.ConnectionString, 0, r.FieldCount, r, _defaultMapper) as Func<IDataReader, T>;
                    using (r)
                    {
                        while (true)
                        {
                            T poco;
                            try
                            {
                                if (!r.Read())
                                    yield break;
                                poco = factory(r);
                            }
                            catch (Exception x)
                            {
                                if (OnException(x))
                                    throw;
                                yield break;
                            }

                            yield return poco;
                        }
                    }
                }
            }
            finally
            {
                CloseSharedConnection();
            }
        }

        /// <summary>
        ///     Runs an SQL query, returning the results as an IEnumerable collection
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="sql">An SQL builder object representing the base SQL query and it's arguments</param>
        /// <returns>An enumerable collection of result records</returns>
        /// <remarks>
        ///     For some DB providers, care should be taken to not start a new Query before finishing with
        ///     and disposing the previous one. In cases where this is an issue, consider using Fetch which
        ///     returns the results as a List rather than an IEnumerable.
        /// </remarks>
        public IEnumerable<T> Query<T>(Sql sql)
        {
            return Query<T>(sql.SQL, sql.Arguments);
        }

        #endregion

        #region operation: Exists

        /// <summary>
        ///     Checks for the existence of a row matching the specified condition
        /// </summary>
        /// <typeparam name="T">The Type representing the table being queried</typeparam>
        /// <param name="sqlCondition">The SQL expression to be tested for (ie: the WHERE expression)</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL statement</param>
        /// <returns>True if a record matching the condition is found.</returns>
        public bool Exists<T>(string sqlCondition, params object[] args)
        {
            var poco = PocoData.ForType(typeof(T), _defaultMapper).TableInfo;

            if (sqlCondition.TrimStart().StartsWith("where", StringComparison.OrdinalIgnoreCase))
                sqlCondition = sqlCondition.TrimStart().Substring(5);

            return ExecuteScalar<int>(string.Format(_provider.GetExistsSql(), Provider.EscapeTableName(poco.TableName), sqlCondition), args) != 0;
        }

        /// <summary>
        ///     Checks for the existence of a row with the specified primary key value.
        /// </summary>
        /// <typeparam name="T">The Type representing the table being queried</typeparam>
        /// <param name="primaryKey">The primary key value to look for</param>
        /// <returns>True if a record with the specified primary key value exists.</returns>
        public bool Exists<T>(object primaryKey)
        {
            return Exists<T>(string.Format("{0}=@0", _provider.EscapeSqlIdentifier(PocoData.ForType(typeof(T), _defaultMapper).TableInfo.PrimaryKey)), primaryKey);
        }

        #endregion

        #region operation: linq style (Exists, Single, SingleOrDefault etc...)

        /// <summary>
        ///     Returns the record with the specified primary key value
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="primaryKey">The primary key value of the record to fetch</param>
        /// <returns>The single record matching the specified primary key value</returns>
        /// <remarks>
        ///     Throws an exception if there are zero or more than one record with the specified primary key value.
        /// </remarks>
        public T Single<T>(object primaryKey)
        {
            return Single<T>(string.Format("WHERE {0}=@0", _provider.EscapeSqlIdentifier(PocoData.ForType(typeof(T), _defaultMapper).TableInfo.PrimaryKey)), primaryKey);
        }

        /// <summary>
        ///     Returns the record with the specified primary key value, or the default value if not found
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="primaryKey">The primary key value of the record to fetch</param>
        /// <returns>The single record matching the specified primary key value</returns>
        /// <remarks>
        ///     If there are no records with the specified primary key value, default(T) (typically null) is returned.
        /// </remarks>
        public T SingleOrDefault<T>(object primaryKey)
        {
            return SingleOrDefault<T>(string.Format("WHERE {0}=@0", _provider.EscapeSqlIdentifier(PocoData.ForType(typeof(T), _defaultMapper).TableInfo.PrimaryKey)), primaryKey);
        }

        /// <summary>
        ///     Runs a query that should always return a single row.
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="sql">The SQL query</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL statement</param>
        /// <returns>The single record matching the specified primary key value</returns>
        /// <remarks>
        ///     Throws an exception if there are zero or more than one matching record
        /// </remarks>
        public T Single<T>(string sql, params object[] args)
        {
            return Query<T>(sql, args).Single();
        }

        /// <summary>
        ///     Runs a query that should always return either a single row, or no rows
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="sql">The SQL query</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL statement</param>
        /// <returns>The single record matching the specified primary key value, or default(T) if no matching rows</returns>
        public T SingleOrDefault<T>(string sql, params object[] args)
        {
            return Query<T>(sql, args).SingleOrDefault();
        }

        /// <summary>
        ///     Runs a query that should always return at least one return
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="sql">The SQL query</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL statement</param>
        /// <returns>The first record in the result set</returns>
        public T First<T>(string sql, params object[] args)
        {
            return Query<T>(sql, args).First();
        }

        /// <summary>
        ///     Runs a query and returns the first record, or the default value if no matching records
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="sql">The SQL query</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL statement</param>
        /// <returns>The first record in the result set, or default(T) if no matching rows</returns>
        public T FirstOrDefault<T>(string sql, params object[] args)
        {
            return Query<T>(sql, args).FirstOrDefault();
        }

        /// <summary>
        ///     Runs a query that should always return a single row.
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>The single record matching the specified primary key value</returns>
        /// <remarks>
        ///     Throws an exception if there are zero or more than one matching record
        /// </remarks>
        public T Single<T>(Sql sql)
        {
            return Query<T>(sql).Single();
        }

        /// <summary>
        ///     Runs a query that should always return either a single row, or no rows
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>The single record matching the specified primary key value, or default(T) if no matching rows</returns>
        public T SingleOrDefault<T>(Sql sql)
        {
            return Query<T>(sql).SingleOrDefault();
        }

        /// <summary>
        ///     Runs a query that should always return at least one return
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>The first record in the result set</returns>
        public T First<T>(Sql sql)
        {
            return Query<T>(sql).First();
        }

        /// <summary>
        ///     Runs a query and returns the first record, or the default value if no matching records
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>The first record in the result set, or default(T) if no matching rows</returns>
        public T FirstOrDefault<T>(Sql sql)
        {
            return Query<T>(sql).FirstOrDefault();
        }

        #endregion

        #region 插入数据 operation: Insert

        /// <summary>
        ///     Performs an SQL Insert
        /// </summary>
        /// <param name="tableName">The name of the table to insert into</param>
        /// <param name="poco">The POCO object that specifies the column values to be inserted</param>
        /// <returns>The auto allocated primary key of the new record, or null for non-auto-increment tables</returns>
        public object Insert(string tableName, object poco)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");

            if (poco == null)
                throw new ArgumentNullException("poco");

            var pd = PocoData.ForType(poco.GetType(), _defaultMapper);

            return ExecuteInsert(tableName, pd == null ? null : pd.TableInfo.PrimaryKey, pd != null && pd.TableInfo.AutoIncrement, poco);
        }

        /// <summary>
        ///     Performs an SQL Insert
        /// </summary>
        /// <param name="tableName">The name of the table to insert into</param>
        /// <param name="primaryKeyName">The name of the primary key column of the table</param>
        /// <param name="poco">The POCO object that specifies the column values to be inserted</param>
        /// <returns>The auto allocated primary key of the new record, or null for non-auto-increment tables</returns>
        public object Insert(string tableName, string primaryKeyName, object poco)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");

            if (string.IsNullOrEmpty(primaryKeyName))
                throw new ArgumentNullException("primaryKeyName");

            if (poco == null)
                throw new ArgumentNullException("poco");

            var t = poco.GetType();
            var pd = PocoData.ForType(poco.GetType(), _defaultMapper);
            var autoIncrement = pd == null || pd.TableInfo.AutoIncrement ||
                                t.Name.Contains("AnonymousType") &&
                                !t.GetProperties().Any(p => p.Name.Equals(primaryKeyName, StringComparison.OrdinalIgnoreCase));

            return ExecuteInsert(tableName, primaryKeyName, autoIncrement, poco);
        }

        /// <summary>
        ///     Performs an SQL Insert
        /// </summary>
        /// <param name="tableName">The name of the table to insert into</param>
        /// <param name="primaryKeyName">The name of the primary key column of the table</param>
        /// <param name="autoIncrement">True if the primary key is automatically allocated by the DB</param>
        /// <param name="poco">The POCO object that specifies the column values to be inserted</param>
        /// <returns>The auto allocated primary key of the new record, or null for non-auto-increment tables</returns>
        /// <remarks>
        ///     Inserts a poco into a table.  If the poco has a property with the same name
        ///     as the primary key the id of the new record is assigned to it.  Either way,
        ///     the new id is returned.
        /// </remarks>
        public object Insert(string tableName, string primaryKeyName, bool autoIncrement, object poco)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");

            if (string.IsNullOrEmpty(primaryKeyName))
                throw new ArgumentNullException("primaryKeyName");

            if (poco == null)
                throw new ArgumentNullException("poco");

            return ExecuteInsert(tableName, primaryKeyName, autoIncrement, poco);
        }

        /// <summary>
        ///     Performs an SQL Insert
        /// </summary>
        /// <param name="poco">The POCO object that specifies the column values to be inserted</param>
        /// <returns>The auto allocated primary key of the new record, or null for non-auto-increment tables</returns>
        /// <remarks>
        ///     The name of the table, it's primary key and whether it's an auto-allocated primary key are retrieved
        ///     from the POCO's attributes
        /// </remarks>
        public object Insert(object poco)
        {
            if (poco == null)
                throw new ArgumentNullException("poco");

            var pd = PocoData.ForType(poco.GetType(), _defaultMapper);
            return ExecuteInsert(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, pd.TableInfo.AutoIncrement, poco);
        }

        private object ExecuteInsert(string tableName, string primaryKeyName, bool autoIncrement, object poco)
        {
            try
            {
                OpenSharedConnection();
                try
                {
                    using (var cmd = CreateCommand(_sharedConnection, ""))
                    {
                        var pd = PocoData.ForObject(poco, primaryKeyName, _defaultMapper);
                        var names = new List<string>();
                        var values = new List<string>();
                        var index = 0;
                        foreach (var i in pd.Columns)
                        {
                            // Don't insert result columns
                            if (i.Value.ResultColumn)
                                continue;

                            // Don't insert the primary key (except under oracle where we need bring in the next sequence value)
                            if (autoIncrement && primaryKeyName != null && string.Compare(i.Key, primaryKeyName, true) == 0)
                            {
                                // Setup auto increment expression
                                string autoIncExpression = _provider.GetAutoIncrementExpression(pd.TableInfo);
                                if (autoIncExpression != null)
                                {
                                    names.Add(i.Key);
                                    values.Add(autoIncExpression);
                                }
                                continue;
                            }

                            names.Add(_provider.EscapeSqlIdentifier(i.Key));
                            values.Add(string.Format(i.Value.InsertTemplate ?? "{0}{1}", _paramPrefix, index++));
                            AddParam(cmd, i.Value.GetValue(poco), i.Value.PropertyInfo);
                        }

                        string outputClause = String.Empty;
                        if (autoIncrement)
                        {
                            outputClause = _provider.GetInsertOutputClause(primaryKeyName);
                        }

                        cmd.CommandText = string.Format("INSERT INTO {0} ({1}){2} VALUES ({3})",
                            _provider.EscapeTableName(tableName),
                            string.Join(",", names.ToArray()),
                            outputClause,
                            string.Join(",", values.ToArray())
                            );

                        if (!autoIncrement)
                        {
                            DoPreExecute(cmd);
                            cmd.ExecuteNonQuery();
                            OnExecutedCommand(cmd);

                            PocoColumn pkColumn;
                            if (primaryKeyName != null && pd.Columns.TryGetValue(primaryKeyName, out pkColumn))
                                return pkColumn.GetValue(poco);
                            else
                                return null;
                        }

                        object id = _provider.ExecuteInsert(this, cmd, primaryKeyName);

                        // Assign the ID back to the primary key property
                        if (primaryKeyName != null && !poco.GetType().Name.Contains("AnonymousType"))
                        {
                            PocoColumn pc;
                            if (pd.Columns.TryGetValue(primaryKeyName, out pc))
                            {
                                pc.SetValue(poco, pc.ChangeType(id));
                            }
                        }

                        return id;
                    }
                }
                finally
                {
                    CloseSharedConnection();
                }
            }
            catch (Exception x)
            {
                if (OnException(x))
                    throw;
                return null;
            }
        }

        #endregion

        #region 更新数据 operation: Update

        /// <summary>
        ///     Performs an SQL update
        /// </summary>
        /// <param name="tableName">The name of the table to update</param>
        /// <param name="primaryKeyName">The name of the primary key column of the table</param>
        /// <param name="poco">The POCO object that specifies the column values to be updated</param>
        /// <param name="primaryKeyValue">The primary key of the record to be updated</param>
        /// <returns>The number of affected records</returns>
        public int Update(string tableName, string primaryKeyName, object poco, object primaryKeyValue)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");

            if (string.IsNullOrEmpty(primaryKeyName))
                throw new ArgumentNullException("primaryKeyName");

            if (poco == null)
                throw new ArgumentNullException("poco");

            return ExecuteUpdate(tableName, primaryKeyName, poco, primaryKeyValue, null);
        }

        /// <summary>
        ///     Performs an SQL update
        /// </summary>
        /// <param name="tableName">The name of the table to update</param>
        /// <param name="primaryKeyName">The name of the primary key column of the table</param>
        /// <param name="poco">The POCO object that specifies the column values to be updated</param>
        /// <param name="primaryKeyValue">The primary key of the record to be updated</param>
        /// <param name="columns">The column names of the columns to be updated, or null for all</param>
        /// <returns>The number of affected rows</returns>
        public int Update(string tableName, string primaryKeyName, object poco, object primaryKeyValue, IEnumerable<string> columns)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");

            if (string.IsNullOrEmpty(primaryKeyName))
                throw new ArgumentNullException("primaryKeyName");

            if (poco == null)
                throw new ArgumentNullException("poco");

            return ExecuteUpdate(tableName, primaryKeyName, poco, primaryKeyValue, columns);
        }

        /// <summary>
        ///     Performs an SQL update
        /// </summary>
        /// <param name="tableName">The name of the table to update</param>
        /// <param name="primaryKeyName">The name of the primary key column of the table</param>
        /// <param name="poco">The POCO object that specifies the column values to be updated</param>
        /// <returns>The number of affected rows</returns>
        public int Update(string tableName, string primaryKeyName, object poco)
        {
            return Update(tableName, primaryKeyName, poco, null);
        }

        /// <summary>
        ///     Performs an SQL update
        /// </summary>
        /// <param name="tableName">The name of the table to update</param>
        /// <param name="primaryKeyName">The name of the primary key column of the table</param>
        /// <param name="poco">The POCO object that specifies the column values to be updated</param>
        /// <param name="columns">The column names of the columns to be updated, or null for all</param>
        /// <returns>The number of affected rows</returns>
        public int Update(string tableName, string primaryKeyName, object poco, IEnumerable<string> columns)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");

            if (string.IsNullOrEmpty(primaryKeyName))
                throw new ArgumentNullException("primaryKeyName");

            if (poco == null)
                throw new ArgumentNullException("poco");

            return ExecuteUpdate(tableName, primaryKeyName, poco, null, columns);
        }

        /// <summary>
        ///     Performs an SQL update
        /// </summary>
        /// <param name="poco">The POCO object that specifies the column values to be updated</param>
        /// <param name="columns">The column names of the columns to be updated, or null for all</param>
        /// <returns>The number of affected rows</returns>
        public int Update(object poco, IEnumerable<string> columns)
        {
            return Update(poco, null, columns);
        }

        /// <summary>
        ///     Performs an SQL update
        /// </summary>
        /// <param name="poco">The POCO object that specifies the column values to be updated</param>
        /// <returns>The number of affected rows</returns>
        public int Update(object poco)
        {
            return Update(poco, null, null);
        }

        /// <summary>
        ///     Performs an SQL update
        /// </summary>
        /// <param name="poco">The POCO object that specifies the column values to be updated</param>
        /// <param name="primaryKeyValue">The primary key of the record to be updated</param>
        /// <returns>The number of affected rows</returns>
        public int Update(object poco, object primaryKeyValue)
        {
            return Update(poco, primaryKeyValue, null);
        }

        /// <summary>
        ///     Performs an SQL update
        /// </summary>
        /// <param name="poco">The POCO object that specifies the column values to be updated</param>
        /// <param name="primaryKeyValue">The primary key of the record to be updated</param>
        /// <param name="columns">The column names of the columns to be updated, or null for all</param>
        /// <returns>The number of affected rows</returns>
        public int Update(object poco, object primaryKeyValue, IEnumerable<string> columns)
        {
            if (poco == null)
                throw new ArgumentNullException("poco");

            var pd = PocoData.ForType(poco.GetType(), _defaultMapper);
            return ExecuteUpdate(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, poco, primaryKeyValue, columns);
        }

        /// <summary>
        ///     Performs an SQL update
        /// </summary>
        /// <typeparam name="T">The POCO class who's attributes specify the name of the table to update</typeparam>
        /// <param name="sql">The SQL update and condition clause (ie: everything after "UPDATE tablename"</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>The number of affected rows</returns>
        public int Update<T>(string sql, params object[] args)
        {
            if (string.IsNullOrEmpty(sql))
                throw new ArgumentNullException("sql");

            var pd = PocoData.ForType(typeof(T), _defaultMapper);
            return Execute(string.Format("UPDATE {0} {1}", _provider.EscapeTableName(pd.TableInfo.TableName), sql), args);
        }

        /// <summary>
        ///     Performs an SQL update
        /// </summary>
        /// <typeparam name="T">The POCO class who's attributes specify the name of the table to update</typeparam>
        /// <param name="sql">
        ///     An SQL builder object representing the SQL update and condition clause (ie: everything after "UPDATE
        ///     tablename"
        /// </param>
        /// <returns>The number of affected rows</returns>
        public int Update<T>(Sql sql)
        {
            if (sql == null)
                throw new ArgumentNullException("sql");

            var pd = PocoData.ForType(typeof(T), _defaultMapper);
            return Execute(new Sql(string.Format("UPDATE {0}", _provider.EscapeTableName(pd.TableInfo.TableName))).Append(sql));
        }

        private int ExecuteUpdate(string tableName, string primaryKeyName, object poco, object primaryKeyValue, IEnumerable<string> columns)
        {
            try
            {
                OpenSharedConnection();
                try
                {
                    using (var cmd = CreateCommand(_sharedConnection, ""))
                    {
                        var sb = new StringBuilder();
                        var index = 0;
                        var pd = PocoData.ForObject(poco, primaryKeyName, _defaultMapper);
                        if (columns == null)
                        {
                            foreach (var i in pd.Columns)
                            {
                                // Don't update the primary key, but grab the value if we don't have it
                                if (string.Compare(i.Key, primaryKeyName, true) == 0)
                                {
                                    if (primaryKeyValue == null)
                                        primaryKeyValue = i.Value.GetValue(poco);
                                    continue;
                                }

                                // Dont update result only columns
                                if (i.Value.ResultColumn)
                                    continue;

                                // Build the sql
                                if (index > 0)
                                    sb.Append(", ");
                                sb.AppendFormat(i.Value.UpdateTemplate ?? "{0} = {1}{2}", _provider.EscapeSqlIdentifier(i.Key), _paramPrefix, index++);

                                // Store the parameter in the command
                                AddParam(cmd, i.Value.GetValue(poco), i.Value.PropertyInfo);
                            }
                        }
                        else
                        {
                            foreach (var colname in columns)
                            {
                                var pc = pd.Columns[colname];

                                // Build the sql
                                if (index > 0)
                                    sb.Append(", ");
                                sb.AppendFormat(pc.UpdateTemplate ?? "{0} = {1}{2}", _provider.EscapeSqlIdentifier(colname), _paramPrefix, index++);

                                // Store the parameter in the command
                                AddParam(cmd, pc.GetValue(poco), pc.PropertyInfo);
                            }

                            // Grab primary key value
                            if (primaryKeyValue == null)
                            {
                                var pc = pd.Columns[primaryKeyName];
                                primaryKeyValue = pc.GetValue(poco);
                            }
                        }

                        // Find the property info for the primary key
                        PropertyInfo pkpi = null;
                        if (primaryKeyName != null)
                        {
                            PocoColumn col;
                            pkpi = pd.Columns.TryGetValue(primaryKeyName, out col)
                                ? col.PropertyInfo
                                : new { Id = primaryKeyValue }.GetType().GetProperties()[0];
                        }

                        cmd.CommandText = string.Format("UPDATE {0} SET {1} WHERE {2} = {3}{4}",
                            _provider.EscapeTableName(tableName), sb.ToString(), _provider.EscapeSqlIdentifier(primaryKeyName), _paramPrefix, index++);
                        AddParam(cmd, primaryKeyValue, pkpi);

                        DoPreExecute(cmd);

                        // Do it
                        var retv = cmd.ExecuteNonQuery();
                        OnExecutedCommand(cmd);
                        return retv;
                    }
                }
                finally
                {
                    CloseSharedConnection();
                }
            }
            catch (Exception x)
            {
                if (OnException(x))
                    throw;
                return -1;
            }
        }

        #endregion

        #region 删除数据 operation: Delete

        /// <summary>
        ///     Performs and SQL Delete
        /// </summary>
        /// <param name="tableName">The name of the table to delete from</param>
        /// <param name="primaryKeyName">The name of the primary key column</param>
        /// <param name="poco">The POCO object whose primary key value will be used to delete the row</param>
        /// <returns>The number of rows affected</returns>
        public int Delete(string tableName, string primaryKeyName, object poco)
        {
            return Delete(tableName, primaryKeyName, poco, null);
        }

        /// <summary>
        ///     Performs and SQL Delete
        /// </summary>
        /// <param name="tableName">The name of the table to delete from</param>
        /// <param name="primaryKeyName">The name of the primary key column</param>
        /// <param name="poco">
        ///     The POCO object whose primary key value will be used to delete the row (or null to use the supplied
        ///     primary key value)
        /// </param>
        /// <param name="primaryKeyValue">
        ///     The value of the primary key identifing the record to be deleted (or null, or get this
        ///     value from the POCO instance)
        /// </param>
        /// <returns>The number of rows affected</returns>
        public int Delete(string tableName, string primaryKeyName, object poco, object primaryKeyValue)
        {
            // If primary key value not specified, pick it up from the object
            if (primaryKeyValue == null)
            {
                var pd = PocoData.ForObject(poco, primaryKeyName, _defaultMapper);
                PocoColumn pc;
                if (pd.Columns.TryGetValue(primaryKeyName, out pc))
                {
                    primaryKeyValue = pc.GetValue(poco);
                }
            }

            // Do it
            var sql = string.Format("DELETE FROM {0} WHERE {1}=@0", _provider.EscapeTableName(tableName), _provider.EscapeSqlIdentifier(primaryKeyName));
            return Execute(sql, primaryKeyValue);
        }

        /// <summary>
        ///     Performs an SQL Delete
        /// </summary>
        /// <param name="poco">The POCO object specifying the table name and primary key value of the row to be deleted</param>
        /// <returns>The number of rows affected</returns>
        public int Delete(object poco)
        {
            var pd = PocoData.ForType(poco.GetType(), _defaultMapper);
            return Delete(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, poco);
        }

        /// <summary>
        ///     Performs an SQL Delete
        /// </summary>
        /// <typeparam name="T">The POCO class whose attributes identify the table and primary key to be used in the delete</typeparam>
        /// <param name="pocoOrPrimaryKey">The value of the primary key of the row to delete</param>
        /// <returns></returns>
        public int Delete<T>(object pocoOrPrimaryKey)
        {
            if (pocoOrPrimaryKey.GetType() == typeof(T))
                return Delete(pocoOrPrimaryKey);

            var pd = PocoData.ForType(typeof(T), _defaultMapper);

            if (pocoOrPrimaryKey.GetType().Name.Contains("AnonymousType"))
            {
                var pi = pocoOrPrimaryKey.GetType().GetProperty(pd.TableInfo.PrimaryKey);

                if (pi == null)
                    throw new InvalidOperationException(string.Format("Anonymous type does not contain an id for PK column `{0}`.", pd.TableInfo.PrimaryKey));

                pocoOrPrimaryKey = pi.GetValue(pocoOrPrimaryKey, new object[0]);
            }

            return Delete(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, null, pocoOrPrimaryKey);
        }

        /// <summary>
        ///     Performs an SQL Delete
        /// </summary>
        /// <typeparam name="T">The POCO class who's attributes specify the name of the table to delete from</typeparam>
        /// <param name="sql">The SQL condition clause identifying the row to delete (ie: everything after "DELETE FROM tablename"</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>The number of affected rows</returns>
        public int Delete<T>(string sql, params object[] args)
        {
            var pd = PocoData.ForType(typeof(T), _defaultMapper);
            return Execute(string.Format("DELETE FROM {0} {1}", _provider.EscapeTableName(pd.TableInfo.TableName), sql), args);
        }

        /// <summary>
        ///     Performs an SQL Delete
        /// </summary>
        /// <typeparam name="T">The POCO class who's attributes specify the name of the table to delete from</typeparam>
        /// <param name="sql">
        ///     An SQL builder object representing the SQL condition clause identifying the row to delete (ie:
        ///     everything after "UPDATE tablename"
        /// </param>
        /// <returns>The number of affected rows</returns>
        public int Delete<T>(Sql sql)
        {
            var pd = PocoData.ForType(typeof(T), _defaultMapper);
            return Execute(new Sql(string.Format("DELETE FROM {0}", _provider.EscapeTableName(pd.TableInfo.TableName))).Append(sql));
        }

        #endregion

        #region operation: IsNew

        /// <summary>
        ///     Check if a poco represents a new row
        /// </summary>
        /// <param name="primaryKeyName">The name of the primary key column</param>
        /// <param name="poco">The object instance whose "newness" is to be tested</param>
        /// <returns>True if the POCO represents a record already in the database</returns>
        /// <remarks>This method simply tests if the POCO's primary key column property has been set to something non-zero.</remarks>
        public bool IsNew(string primaryKeyName, object poco)
        {
            if (poco == null)
                throw new ArgumentNullException("poco");

            if (string.IsNullOrEmpty(primaryKeyName))
                throw new ArgumentException("primaryKeyName");

            return IsNew(primaryKeyName, PocoData.ForObject(poco, primaryKeyName, _defaultMapper), poco);
        }

        protected virtual bool IsNew(string primaryKeyName, PocoData pd, object poco)
        {
            if (string.IsNullOrEmpty(primaryKeyName) || poco is ExpandoObject)
                throw new InvalidOperationException("IsNew() and Save() are only supported on tables with identity (inc auto-increment) primary key columns");

            object pk;
            PocoColumn pc;
            PropertyInfo pi;
            if (pd.Columns.TryGetValue(primaryKeyName, out pc))
            {
                pk = pc.GetValue(poco);
                pi = pc.PropertyInfo;
            }
            else
            {
                pi = poco.GetType().GetProperty(primaryKeyName);
                if (pi == null)
                    throw new ArgumentException(string.Format("The object doesn't have a property matching the primary key column name '{0}'", primaryKeyName));
                pk = pi.GetValue(poco, null);
            }

            var type = pk != null ? pk.GetType() : pi.PropertyType;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) || !type.IsValueType)
                return pk == null;

            if (type == typeof(string))
                return string.IsNullOrEmpty((string)pk);
            if (!pi.PropertyType.IsValueType)
                return pk == null;
            if (type == typeof(long))
                return (long)pk == default(long);
            if (type == typeof(int))
                return (int)pk == default(int);
            if (type == typeof(Guid))
                return (Guid)pk == default(Guid);
            if (type == typeof(ulong))
                return (ulong)pk == default(ulong);
            if (type == typeof(uint))
                return (uint)pk == default(uint);
            if (type == typeof(short))
                return (short)pk == default(short);
            if (type == typeof(ushort))
                return (ushort)pk == default(ushort);

            // Create a default instance and compare
            return pk == Activator.CreateInstance(pk.GetType());
        }

        /// <summary>
        ///     Check if a poco represents a new row
        /// </summary>
        /// <param name="poco">The object instance whose "newness" is to be tested</param>
        /// <returns>True if the POCO represents a record already in the database</returns>
        /// <remarks>This method simply tests if the POCO's primary key column property has been set to something non-zero.</remarks>
        public bool IsNew(object poco)
        {
            if (poco == null)
                throw new ArgumentNullException("poco");

            var pd = PocoData.ForType(poco.GetType(), _defaultMapper);
            return IsNew(pd.TableInfo.PrimaryKey, pd, poco);
        }

        #endregion

        #region operation: Save

        /// <summary>
        ///     Saves a POCO by either performing either an SQL Insert or SQL Update
        /// </summary>
        /// <param name="tableName">The name of the table to be updated</param>
        /// <param name="primaryKeyName">The name of the primary key column</param>
        /// <param name="poco">The POCO object to be saved</param>
        public void Save(string tableName, string primaryKeyName, object poco)
        {
            if (IsNew(primaryKeyName, poco))
            {
                Insert(tableName, primaryKeyName, true, poco);
            }
            else
            {
                Update(tableName, primaryKeyName, poco);
            }
        }

        /// <summary>
        ///     Saves a POCO by either performing either an SQL Insert or SQL Update
        /// </summary>
        /// <param name="poco">The POCO object to be saved</param>
        public void Save(object poco)
        {
            var pd = PocoData.ForType(poco.GetType(), _defaultMapper);
            Save(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, poco);
        }

        #endregion

        #region operation: Multi-Poco Query/Fetch

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="TRet">The returned list POCO type</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as a List</returns>
        public List<TRet> Fetch<T1, T2, TRet>(Func<T1, T2, TRet> cb, string sql, params object[] args)
        {
            return Query<T1, T2, TRet>(cb, sql, args).ToList();
        }

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="TRet">The returned list POCO type</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as a List</returns>
        public List<TRet> Fetch<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, string sql, params object[] args)
        {
            return Query<T1, T2, T3, TRet>(cb, sql, args).ToList();
        }

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="TRet">The returned list POCO type</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as a List</returns>
        public List<TRet> Fetch<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, string sql, params object[] args)
        {
            return Query<T1, T2, T3, T4, TRet>(cb, sql, args).ToList();
        }

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="T5">The fifth POCO type</typeparam>
        /// <typeparam name="TRet">The returned list POCO type</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as a List</returns>
        public List<TRet> Fetch<T1, T2, T3, T4, T5, TRet>(Func<T1, T2, T3, T4, T5, TRet> cb, string sql, params object[] args)
        {
            return Query<T1, T2, T3, T4, T5, TRet>(cb, sql, args).ToList();
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        public IEnumerable<TRet> Query<T1, T2, TRet>(Func<T1, T2, TRet> cb, string sql, params object[] args)
        {
            return Query<TRet>(new Type[] { typeof(T1), typeof(T2) }, cb, sql, args);
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        public IEnumerable<TRet> Query<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, string sql, params object[] args)
        {
            return Query<TRet>(new Type[] { typeof(T1), typeof(T2), typeof(T3) }, cb, sql, args);
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        public IEnumerable<TRet> Query<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, string sql, params object[] args)
        {
            return Query<TRet>(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, cb, sql, args);
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="T5">The fifth POCO type</typeparam>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        public IEnumerable<TRet> Query<T1, T2, T3, T4, T5, TRet>(Func<T1, T2, T3, T4, T5, TRet> cb, string sql, params object[] args)
        {
            return Query<TRet>(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) }, cb, sql, args);
        }

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="TRet">The returned list POCO type</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as a List</returns>
        public List<TRet> Fetch<T1, T2, TRet>(Func<T1, T2, TRet> cb, Sql sql)
        {
            return Query<T1, T2, TRet>(cb, sql.SQL, sql.Arguments).ToList();
        }

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="TRet">The returned list POCO type</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as a List</returns>
        public List<TRet> Fetch<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, Sql sql)
        {
            return Query<T1, T2, T3, TRet>(cb, sql.SQL, sql.Arguments).ToList();
        }

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="TRet">The returned list POCO type</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as a List</returns>
        public List<TRet> Fetch<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, Sql sql)
        {
            return Query<T1, T2, T3, T4, TRet>(cb, sql.SQL, sql.Arguments).ToList();
        }

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="T5">The fifth POCO type</typeparam>
        /// <typeparam name="TRet">The returned list POCO type</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as a List</returns>
        public List<TRet> Fetch<T1, T2, T3, T4, T5, TRet>(Func<T1, T2, T3, T4, T5, TRet> cb, Sql sql)
        {
            return Query<T1, T2, T3, T4, T5, TRet>(cb, sql.SQL, sql.Arguments).ToList();
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        public IEnumerable<TRet> Query<T1, T2, TRet>(Func<T1, T2, TRet> cb, Sql sql)
        {
            return Query<TRet>(new Type[] { typeof(T1), typeof(T2) }, cb, sql.SQL, sql.Arguments);
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        public IEnumerable<TRet> Query<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, Sql sql)
        {
            return Query<TRet>(new Type[] { typeof(T1), typeof(T2), typeof(T3) }, cb, sql.SQL, sql.Arguments);
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        public IEnumerable<TRet> Query<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, Sql sql)
        {
            return Query<TRet>(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, cb, sql.SQL, sql.Arguments);
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="T5">The fifth POCO type</typeparam>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        public IEnumerable<TRet> Query<T1, T2, T3, T4, T5, TRet>(Func<T1, T2, T3, T4, T5, TRet> cb, Sql sql)
        {
            return Query<TRet>(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) }, cb, sql.SQL, sql.Arguments);
        }

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as a List</returns>
        public List<T1> Fetch<T1, T2>(string sql, params object[] args)
        {
            return Query<T1, T2>(sql, args).ToList();
        }

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as a List</returns>
        public List<T1> Fetch<T1, T2, T3>(string sql, params object[] args)
        {
            return Query<T1, T2, T3>(sql, args).ToList();
        }

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as a List</returns>
        public List<T1> Fetch<T1, T2, T3, T4>(string sql, params object[] args)
        {
            return Query<T1, T2, T3, T4>(sql, args).ToList();
        }

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="T5">The fifth POCO type</typeparam>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as a List</returns>
        public List<T1> Fetch<T1, T2, T3, T4, T5>(string sql, params object[] args)
        {
            return Query<T1, T2, T3, T4, T5>(sql, args).ToList();
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        public IEnumerable<T1> Query<T1, T2>(string sql, params object[] args)
        {
            return Query<T1>(new Type[] { typeof(T1), typeof(T2) }, null, sql, args);
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        public IEnumerable<T1> Query<T1, T2, T3>(string sql, params object[] args)
        {
            return Query<T1>(new Type[] { typeof(T1), typeof(T2), typeof(T3) }, null, sql, args);
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        public IEnumerable<T1> Query<T1, T2, T3, T4>(string sql, params object[] args)
        {
            return Query<T1>(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, null, sql, args);
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="T5">The fifth POCO type</typeparam>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        public IEnumerable<T1> Query<T1, T2, T3, T4, T5>(string sql, params object[] args)
        {
            return Query<T1>(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) }, null, sql, args);
        }

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as a List</returns>
        public List<T1> Fetch<T1, T2>(Sql sql)
        {
            return Query<T1, T2>(sql.SQL, sql.Arguments).ToList();
        }

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as a List</returns>
        public List<T1> Fetch<T1, T2, T3>(Sql sql)
        {
            return Query<T1, T2, T3>(sql.SQL, sql.Arguments).ToList();
        }

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as a List</returns>
        public List<T1> Fetch<T1, T2, T3, T4>(Sql sql)
        {
            return Query<T1, T2, T3, T4>(sql.SQL, sql.Arguments).ToList();
        }

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="T5">The fifth POCO type</typeparam>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as a List</returns>
        public List<T1> Fetch<T1, T2, T3, T4, T5>(Sql sql)
        {
            return Query<T1, T2, T3, T4, T5>(sql.SQL, sql.Arguments).ToList();
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        public IEnumerable<T1> Query<T1, T2>(Sql sql)
        {
            return Query<T1>(new Type[] { typeof(T1), typeof(T2) }, null, sql.SQL, sql.Arguments);
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        public IEnumerable<T1> Query<T1, T2, T3>(Sql sql)
        {
            return Query<T1>(new Type[] { typeof(T1), typeof(T2), typeof(T3) }, null, sql.SQL, sql.Arguments);
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        public IEnumerable<T1> Query<T1, T2, T3, T4>(Sql sql)
        {
            return Query<T1>(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, null, sql.SQL, sql.Arguments);
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="T5">The fifth POCO type</typeparam>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        public IEnumerable<T1> Query<T1, T2, T3, T4, T5>(Sql sql)
        {
            return Query<T1>(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) }, null, sql.SQL, sql.Arguments);
        }

        /// <summary>
        ///     Performs a multi-poco query
        /// </summary>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="types">An array of Types representing the POCO types of the returned result set.</param>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        public IEnumerable<TRet> Query<TRet>(Type[] types, object cb, string sql, params object[] args)
        {
            OpenSharedConnection();
            try
            {
                using (var cmd = CreateCommand(_sharedConnection, sql, args))
                {
                    IDataReader r;
                    try
                    {
                        r = cmd.ExecuteReader();
                        OnExecutedCommand(cmd);
                    }
                    catch (Exception x)
                    {
                        if (OnException(x))
                            throw;
                        yield break;
                    }
                    var factory = MultiPocoFactory.GetFactory<TRet>(types, _sharedConnection.ConnectionString, sql, r, _defaultMapper);
                    if (cb == null)
                        cb = MultiPocoFactory.GetAutoMapper(types.ToArray());
                    bool bNeedTerminator = false;
                    using (r)
                    {
                        while (true)
                        {
                            TRet poco;
                            try
                            {
                                if (!r.Read())
                                    break;
                                poco = factory(r, cb);
                            }
                            catch (Exception x)
                            {
                                if (OnException(x))
                                    throw;
                                yield break;
                            }

                            if (poco != null)
                                yield return poco;
                            else
                                bNeedTerminator = true;
                        }
                        if (bNeedTerminator)
                        {
                            var poco = (TRet)(cb as Delegate).DynamicInvoke(new object[types.Length]);
                            if (poco != null)
                                yield return poco;
                            else
                                yield break;
                        }
                    }
                }
            }
            finally
            {
                CloseSharedConnection();
            }
        }

        #endregion

        #region operation: Multi-Result Set
        /// <summary>
        /// Perform a multi-results set query
        /// </summary>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A GridReader to be queried</returns>
        public IGridReader QueryMultiple(Sql sql)
        {
            return QueryMultiple(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// Perform a multi-results set query
        /// </summary>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A GridReader to be queried</returns>
        public IGridReader QueryMultiple(string sql, params object[] args)
        {
            OpenSharedConnection();

            GridReader result = null;

            var cmd = CreateCommand(_sharedConnection, sql, args);

            try
            {
                var reader = cmd.ExecuteReader();
                result = new GridReader(this, cmd, reader, _defaultMapper);
            }
            catch (Exception x)
            {
                if (OnException(x))
                    throw;
            }
            return result;
        }
        #endregion

        #region Last Command

        /// <summary>
        ///     Retrieves the SQL of the last executed statement
        /// </summary>
        public string LastSQL
        {
            get { return _lastSql; }
        }

        /// <summary>
        ///     Retrieves the arguments to the last execute statement
        /// </summary>
        public object[] LastArgs
        {
            get { return _lastArgs; }
        }

        /// <summary>
        ///     Returns a formatted string describing the last executed SQL statement and it's argument values
        /// </summary>
        public string LastCommand
        {
            get { return FormatCommand(_lastSql, _lastArgs); }
        }

        #endregion

        #region FormatCommand

        /// <summary>
        ///     Formats the contents of a DB command for display
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public string FormatCommand(IDbCommand cmd)
        {
            return FormatCommand(cmd.CommandText, (from IDataParameter parameter in cmd.Parameters select parameter.Value).ToArray());
        }

        /// <summary>
        ///     Formats an SQL query and it's arguments for display
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public string FormatCommand(string sql, object[] args)
        {
            var sb = new StringBuilder();
            if (sql == null)
                return "";
            sb.Append(sql);
            if (args != null && args.Length > 0)
            {
                sb.Append("\n");
                for (int i = 0; i < args.Length; i++)
                {
                    sb.AppendFormat("\t -> {0}{1} [{2}] = \"{3}\"\n", _paramPrefix, i, args[i].GetType().Name, args[i]);
                }
                sb.Remove(sb.Length - 1, 1);
            }
            return sb.ToString();
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the default mapper.
        /// </summary>
        public IMapper DefaultMapper
        {
            get { return _defaultMapper; }
        }

        /// <summary>
        ///     When set to true, PetaPoco will automatically create the "SELECT columns" part of any query that looks like it
        ///     needs it
        /// </summary>
        public bool EnableAutoSelect { get; set; }

        /// <summary>
        ///     When set to true, parameters can be named ?myparam and populated from properties of the passed in argument values.
        /// </summary>
        public bool EnableNamedParams { get; set; }

        /// <summary>
        ///     Sets the timeout value for all SQL statements.
        /// </summary>
        public int CommandTimeout { get; set; }

        /// <summary>
        ///     Sets the timeout value for the next (and only next) SQL statement
        /// </summary>
        public int OneTimeCommandTimeout { get; set; }

        /// <summary>
        ///     Gets the loaded database provider. <seealso cref="Provider" />.
        /// </summary>
        /// <returns>
        ///     The loaded database type.
        /// </returns>
        public IProvider Provider
        {
            get { return _provider; }
        }

        /// <summary>
        ///     Gets the connection string.
        /// </summary>
        /// <returns>
        ///     The connection string.
        /// </returns>
        public string ConnectionString
        {
            get { return _connectionString; }
        }

        /// <summary>
        ///     Gets or sets the transaction isolation level.
        /// </summary>
        /// <remarks>
        ///     When value is null, the underlying providers default isolation level is used.
        /// </remarks>
        public IsolationLevel? IsolationLevel
        {
            get { return _isolationLevel; }
            set
            {
                if (_transaction != null)
                    throw new InvalidOperationException("Isolation level can't be changed during a transaction.");

                _isolationLevel = value;
            }
        }

        #endregion

        #region Member Fields

        // Member variables
        private IMapper _defaultMapper;
        private string _connectionString;
        private IProvider _provider;
        private IDbConnection _sharedConnection;
        private IDbTransaction _transaction;
        private int _sharedConnectionDepth;
        private int _transactionDepth;
        private bool _transactionCancelled;
        private string _lastSql;
        private object[] _lastArgs;
        private string _paramPrefix;
        private DbProviderFactory _factory;
        private IsolationLevel? _isolationLevel;

        #endregion

        #region Internal operations

        internal void DoPreExecute(IDbCommand cmd)
        {
            // Setup command timeout
            if (OneTimeCommandTimeout != 0)
            {
                cmd.CommandTimeout = OneTimeCommandTimeout;
                OneTimeCommandTimeout = 0;
            }
            else if (CommandTimeout != 0)
            {
                cmd.CommandTimeout = CommandTimeout;
            }

            // Call hook
            OnExecutingCommand(cmd);

            // Save it
            _lastSql = cmd.CommandText;
            _lastArgs = (from IDataParameter parameter in cmd.Parameters select parameter.Value).ToArray();
        }

        #endregion
    }


    /// <summary>
    ///     A helper class which enables fluent configuration.
    /// </summary>
    public class DatabaseConfiguration : IDatabaseBuildConfiguration, IBuildConfigurationSettings, IHideObjectMethods
    {
        private readonly IDictionary<string, object> _settings = new Dictionary<string, object>();

        /// <summary>
        ///     Private constructor to force usage of static build method.
        /// </summary>
        private DatabaseConfiguration()
        {
        }

        void IBuildConfigurationSettings.SetSetting(string key, object value)
        {
            // Note: no argument checking because, pref, enduser unlikely and handled by RT/FW
            if (value != null)
                _settings[key] = value;
            else
                _settings.Remove(key);
        }

        void IBuildConfigurationSettings.TryGetSetting<T>(string key, Action<T> setSetting, Action onFail = null)
        {
            // Note: no argument checking because, pref, enduser unlikely and handled by RT/FW
            object setting;
            if (_settings.TryGetValue(key, out setting))
                setSetting((T)setting);
            else if (onFail != null)
                onFail();
        }

        /// <summary>
        ///     Starts a new PetaPoco build configuration.
        /// </summary>
        /// <returns>An instance of <see cref="IDatabaseBuildConfiguration" /> to form a fluent interface.</returns>
        public static IDatabaseBuildConfiguration Build()
        {
            return new DatabaseConfiguration();
        }
    }


    /// <summary>
    ///     A static helper class where extensions for <see cref="IDatabaseBuildConfiguration" /> are placed.
    /// </summary>
    public static class DatabaseConfigurationExtensions
    {
        internal const string CommandTimeout = "CommandTimeout";

        internal const string EnableAutoSelect = "EnableAutoSelect";

        internal const string EnableNamedParams = "EnableNamedParams";

        internal const string Provider = "Provider";

        internal const string ConnectionString = "ConnectionString";

        internal const string ConnectionStringName = "ConnectionStringName";

        internal const string DefaultMapper = "DefaultMapper";

        internal const string IsolationLevel = "IsolationLevel";

        private static void SetSetting(this IDatabaseBuildConfiguration source, string key, object value)
        {
            ((IBuildConfigurationSettings)source).SetSetting(key, value);
        }

        /// <summary>
        ///     Adds a command timeout - see <see cref="IDatabase.CommandTimeout" />.
        /// </summary>
        /// <param name="source">The configuration source.</param>
        /// <param name="seconds">The timeout in seconds.</param>
        /// <exception cref="ArgumentException">Thrown when seconds is less than 1.</exception>
        /// <returns>The configuration source to form a fluent interface.</returns>
        public static IDatabaseBuildConfiguration UsingCommandTimeout(this IDatabaseBuildConfiguration source, int seconds)
        {
            if (seconds < 1)
                throw new ArgumentException("Timeout value must be greater than zero.");
            source.SetSetting(CommandTimeout, seconds);
            return source;
        }

        /// <summary>
        ///     Enables named params - see <see cref="IDatabase.EnableNamedParams" />.
        /// </summary>
        /// <param name="source">The configuration source.</param>
        /// <returns>The configuration source to form a fluent interface.</returns>
        public static IDatabaseBuildConfiguration WithNamedParams(this IDatabaseBuildConfiguration source)
        {
            source.SetSetting(EnableNamedParams, true);
            return source;
        }

        /// <summary>
        ///     Disables named params - see <see cref="IDatabase.EnableNamedParams" />.
        /// </summary>
        /// <param name="source">The configuration source.</param>
        /// <returns>The configuration source to form a fluent interface.</returns>
        public static IDatabaseBuildConfiguration WithoutNamedParams(this IDatabaseBuildConfiguration source)
        {
            source.SetSetting(EnableNamedParams, false);
            return source;
        }

        /// <summary>
        ///     Specifies the provider to be used. - see <see cref="IDatabase.Provider" />.
        /// </summary>
        /// <param name="source">The configuration source.</param>
        /// <param name="provider">The provider to use.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="provider" /> is null.</exception>
        /// <returns>The configuration source to form a fluent interface.</returns>
        public static IDatabaseBuildConfiguration UsingProvider<T>(this IDatabaseBuildConfiguration source, T provider)
            where T : class, IProvider
        {
            if (provider == null)
                throw new ArgumentNullException("provider");
            source.SetSetting(Provider, provider);
            return source;
        }

        /// <summary>
        ///     Specifies the provider to be used. - see <see cref="IDatabase.Provider" />.
        /// </summary>
        /// <param name="source">The configuration source.</param>
        /// <param name="configure">The configure provider callback.</param>
        /// <param name="provider">The provider to use.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="provider" /> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure" /> is null.</exception>
        /// <returns>The configuration source to form a fluent interface.</returns>
        public static IDatabaseBuildConfiguration UsingProvider<T>(this IDatabaseBuildConfiguration source, T provider, Action<T> configure)
            where T : class, IProvider
        {
            if (provider == null)
                throw new ArgumentNullException("provider");
            if (configure == null)
                throw new ArgumentNullException("configure");
            source.SetSetting(Provider, provider);
            return source;
        }

        /// <summary>
        ///     Specifies the provider to be used. - see <see cref="IDatabase.Provider" />.
        /// </summary>
        /// <param name="source">The configuration source.</param>
        /// <typeparam name="T">The provider type.</typeparam>
        /// <returns>The configuration source to form a fluent interface.</returns>
        public static IDatabaseBuildConfiguration UsingProvider<T>(this IDatabaseBuildConfiguration source)
            where T : class, IProvider, new()
        {
            source.SetSetting(Provider, new T());
            return source;
        }

        /// <summary>
        ///     Specifies the provider to be used. - see <see cref="IDatabase.Provider" />.
        /// </summary>
        /// <param name="source">The configuration source.</param>
        /// <param name="configure">The configure provider callback.</param>
        /// <typeparam name="T">The provider type.</typeparam>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure" /> is null.</exception>
        /// <returns>The configuration source to form a fluent interface.</returns>
        public static IDatabaseBuildConfiguration UsingProvider<T>(this IDatabaseBuildConfiguration source, Action<T> configure)
            where T : class, IProvider, new()
        {
            if (configure == null)
                throw new ArgumentNullException("configure");
            var provider = new T();
            configure(provider);
            source.SetSetting(Provider, provider);
            return source;
        }

        /// <summary>
        ///     Enables auto select - see <see cref="IDatabase.EnableAutoSelect" />.
        /// </summary>
        /// <param name="source">The configuration source.</param>
        /// <returns>The configuration source to form a fluent interface.</returns>
        public static IDatabaseBuildConfiguration WithAutoSelect(this IDatabaseBuildConfiguration source)
        {
            source.SetSetting("EnableAutoSelect", true);
            return source;
        }

        /// <summary>
        ///     Disables auto select - see <see cref="IDatabase.EnableAutoSelect" />.
        /// </summary>
        /// <param name="source">The configuration source.</param>
        /// <returns>The configuration source to form a fluent interface.</returns>
        public static IDatabaseBuildConfiguration WithoutAutoSelect(this IDatabaseBuildConfiguration source)
        {
            source.SetSetting("EnableAutoSelect", false);
            return source;
        }

        /// <summary>
        ///     Adds a connection string - see <see cref="IDatabase.ConnectionString" />.
        /// </summary>
        /// <param name="source">The configuration source.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="connectionString" /> is null or empty.</exception>
        /// <returns>The configuration source to form a fluent interface.</returns>
        public static IDatabaseBuildConfiguration UsingConnectionString(this IDatabaseBuildConfiguration source, string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Argument is null or empty", "connectionString");
            source.SetSetting(ConnectionString, connectionString);
            return source;
        }

        /// <summary>
        ///     Adds a connection string name.
        /// </summary>
        /// <param name="source">The configuration source.</param>
        /// <param name="connectionStringName">The connection string name.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="connectionStringName" /> is null or empty.</exception>
        /// <returns>The configuration source to form a fluent interface.</returns>
        public static IDatabaseBuildConfiguration UsingConnectionStringName(this IDatabaseBuildConfiguration source, string connectionStringName)
        {
            if (string.IsNullOrEmpty(connectionStringName))
                throw new ArgumentException("Argument is null or empty", "connectionStringName");
            source.SetSetting(ConnectionStringName, connectionStringName);
            return source;
        }

        /// <summary>
        ///     Specifies the default mapper to use when no specific mapper has been registered.
        /// </summary>
        /// <param name="source">The configuration source.</param>
        /// <param name="mapper">The mapper to use as the default.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="mapper" /> is null.</exception>
        /// <returns>The configuration source to form a fluent interface.</returns>
        public static IDatabaseBuildConfiguration UsingDefaultMapper<T>(this IDatabaseBuildConfiguration source, T mapper)
            where T : class, IMapper
        {
            if (mapper == null)
                throw new ArgumentNullException("mapper");
            source.SetSetting(DefaultMapper, mapper);
            return source;
        }

        /// <summary>
        ///     Specifies the default mapper to use when no specific mapper has been registered.
        /// </summary>
        /// <param name="source">The configuration source.</param>
        /// <param name="mapper">The mapper to use as the default.</param>
        /// <param name="configure">The configure mapper callback.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="mapper" /> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure" /> is null.</exception>
        /// <returns>The configuration source to form a fluent interface.</returns>
        public static IDatabaseBuildConfiguration UsingDefaultMapper<T>(this IDatabaseBuildConfiguration source, T mapper, Action<T> configure)
            where T : class, IMapper
        {
            if (mapper == null)
                throw new ArgumentNullException("mapper");
            if (configure == null)
                throw new ArgumentNullException("configure");
            configure(mapper);
            source.SetSetting(DefaultMapper, mapper);
            return source;
        }

        /// <summary>
        ///     Specifies the default mapper to use when no specific mapper has been registered.
        /// </summary>
        /// <param name="source">The configuration source.</param>
        /// <typeparam name="T">The mapper type.</typeparam>
        /// <returns>The configuration source to form a fluent interface.</returns>
        public static IDatabaseBuildConfiguration UsingDefaultMapper<T>(this IDatabaseBuildConfiguration source)
            where T : class, IMapper, new()
        {
            source.SetSetting(DefaultMapper, new T());
            return source;
        }

        /// <summary>
        ///     Specifies the default mapper to use when no specific mapper has been registered.
        /// </summary>
        /// <param name="source">The configuration source.</param>
        /// <param name="configure">The configure mapper callback.</param>
        /// <typeparam name="T">The mapper type.</typeparam>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure" /> is null.</exception>
        /// <returns>The configuration source to form a fluent interface.</returns>
        public static IDatabaseBuildConfiguration UsingDefaultMapper<T>(this IDatabaseBuildConfiguration source, Action<T> configure)
            where T : class, IMapper, new()
        {
            if (configure == null)
                throw new ArgumentNullException("configure");

            var mapper = new T();
            configure(mapper);
            source.SetSetting(DefaultMapper, mapper);
            return source;
        }

        /// <summary>
        ///     Specifies the transaction isolation level to use.
        /// </summary>
        /// <param name="source">The configuration source.</param>
        /// <param name="isolationLevel"></param>
        /// <returns>The configuration source to form a fluent interface.</returns>
        public static IDatabaseBuildConfiguration UsingIsolationLevel(this IDatabaseBuildConfiguration source, IsolationLevel isolationLevel)
        {
            source.SetSetting(IsolationLevel, isolationLevel);
            return source;
        }

        /// <summary>
        ///     Creates an instance of PetaPooc using the specified <paramref name="source" />.
        /// </summary>
        /// <param name="source">The configuration source used to create and configure an instance of PetaPoco.</param>
        /// <returns>An instance of PetaPoco.</returns>
        public static IDatabase Create(this IDatabaseBuildConfiguration source)
        {
            return new Database(source);
        }
    }


    public interface IAlterPoco
    {
        /// <summary>
        ///     Performs an SQL Insert.
        /// </summary>
        /// <param name="tableName">The name of the table to insert into.</param>
        /// <param name="poco">The POCO object that specifies the column values to be inserted.</param>
        /// <returns>The auto allocated primary key of the new record, or null for non-auto-increment tables.</returns>
        object Insert(string tableName, object poco);

        /// <summary>
        ///     Performs an SQL Insert.
        /// </summary>
        /// <param name="tableName">The name of the table to insert into.</param>
        /// <param name="primaryKeyName">The name of the primary key column of the table.</param>
        /// <param name="poco">The POCO object that specifies the column values to be inserted.</param>
        /// <returns>The auto allocated primary key of the new record, or null for non-auto-increment tables.</returns>
        object Insert(string tableName, string primaryKeyName, object poco);

        /// <summary>
        ///     Performs an SQL Insert.
        /// </summary>
        /// <param name="tableName">The name of the table to insert into.</param>
        /// <param name="primaryKeyName">The name of the primary key column of the table.</param>
        /// <param name="autoIncrement">True if the primary key is automatically allocated by the DB.</param>
        /// <param name="poco">The POCO object that specifies the column values to be inserted.</param>
        /// <returns>The auto allocated primary key of the new record, or null for non-auto-increment tables.</returns>
        /// <remarks>
        ///     Inserts a poco into a table. If the poco has a property with the same name
        ///     as the primary key, the id of the new record is assigned to it. Either way,
        ///     the new id is returned.
        /// </remarks>
        object Insert(string tableName, string primaryKeyName, bool autoIncrement, object poco);

        /// <summary>
        ///     Performs an SQL Insert.
        /// </summary>
        /// <param name="poco">The POCO object that specifies the column values to be inserted.</param>
        /// <returns>The auto allocated primary key of the new record, or null for non-auto-increment tables.</returns>
        /// <remarks>
        ///     The name of the table, it's primary key and whether it's an auto-allocated primary key are retrieved
        ///     from the POCO's attributes
        /// </remarks>
        object Insert(object poco);

        /// <summary>
        ///     Performs an SQL update
        /// </summary>
        /// <param name="tableName">The name of the table to update</param>
        /// <param name="primaryKeyName">The name of the primary key column of the table</param>
        /// <param name="poco">The POCO object that specifies the column values to be updated</param>
        /// <param name="primaryKeyValue">The primary key of the record to be updated</param>
        /// <returns>The number of affected records</returns>
        int Update(string tableName, string primaryKeyName, object poco, object primaryKeyValue);

        /// <summary>
        ///     Performs an SQL update
        /// </summary>
        /// <param name="tableName">The name of the table to update</param>
        /// <param name="primaryKeyName">The name of the primary key column of the table</param>
        /// <param name="poco">The POCO object that specifies the column values to be updated</param>
        /// <param name="primaryKeyValue">The primary key of the record to be updated</param>
        /// <param name="columns">The column names of the columns to be updated, or null for all</param>
        /// <returns>The number of affected rows</returns>
        int Update(string tableName, string primaryKeyName, object poco, object primaryKeyValue, IEnumerable<string> columns);

        /// <summary>
        ///     Performs an SQL update
        /// </summary>
        /// <param name="tableName">The name of the table to update</param>
        /// <param name="primaryKeyName">The name of the primary key column of the table</param>
        /// <param name="poco">The POCO object that specifies the column values to be updated</param>
        /// <returns>The number of affected rows</returns>
        int Update(string tableName, string primaryKeyName, object poco);

        /// <summary>
        ///     Performs an SQL update
        /// </summary>
        /// <param name="tableName">The name of the table to update</param>
        /// <param name="primaryKeyName">The name of the primary key column of the table</param>
        /// <param name="poco">The POCO object that specifies the column values to be updated</param>
        /// <param name="columns">The column names of the columns to be updated, or null for all</param>
        /// <returns>The number of affected rows</returns>
        int Update(string tableName, string primaryKeyName, object poco, IEnumerable<string> columns);

        /// <summary>
        ///     Performs an SQL update
        /// </summary>
        /// <param name="poco">The POCO object that specifies the column values to be updated</param>
        /// <param name="columns">The column names of the columns to be updated, or null for all</param>
        /// <returns>The number of affected rows</returns>
        int Update(object poco, IEnumerable<string> columns);

        /// <summary>
        ///     Performs an SQL update
        /// </summary>
        /// <param name="poco">The POCO object that specifies the column values to be updated</param>
        /// <returns>The number of affected rows</returns>
        int Update(object poco);

        /// <summary>
        ///     Performs an SQL update
        /// </summary>
        /// <param name="poco">The POCO object that specifies the column values to be updated</param>
        /// <param name="primaryKeyValue">The primary key of the record to be updated</param>
        /// <returns>The number of affected rows</returns>
        int Update(object poco, object primaryKeyValue);

        /// <summary>
        ///     Performs an SQL update
        /// </summary>
        /// <param name="poco">The POCO object that specifies the column values to be updated</param>
        /// <param name="primaryKeyValue">The primary key of the record to be updated</param>
        /// <param name="columns">The column names of the columns to be updated, or null for all</param>
        /// <returns>The number of affected rows</returns>
        int Update(object poco, object primaryKeyValue, IEnumerable<string> columns);

        /// <summary>
        ///     Performs an SQL update
        /// </summary>
        /// <typeparam name="T">The POCO class who's attributes specify the name of the table to update</typeparam>
        /// <param name="sql">The SQL update and condition clause (ie: everything after "UPDATE tablename"</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>The number of affected rows</returns>
        int Update<T>(string sql, params object[] args);

        /// <summary>
        ///     Performs an SQL update
        /// </summary>
        /// <typeparam name="T">The POCO class who's attributes specify the name of the table to update</typeparam>
        /// <param name="sql">
        ///     An SQL builder object representing the SQL update and condition clause (ie: everything after "UPDATE
        ///     tablename"
        /// </param>
        /// <returns>The number of affected rows</returns>
        int Update<T>(Sql sql);

        /// <summary>
        ///     Performs and SQL Delete
        /// </summary>
        /// <param name="tableName">The name of the table to delete from</param>
        /// <param name="primaryKeyName">The name of the primary key column</param>
        /// <param name="poco">The POCO object whose primary key value will be used to delete the row</param>
        /// <returns>The number of rows affected</returns>
        int Delete(string tableName, string primaryKeyName, object poco);

        /// <summary>
        ///     Performs and SQL Delete
        /// </summary>
        /// <param name="tableName">The name of the table to delete from</param>
        /// <param name="primaryKeyName">The name of the primary key column</param>
        /// <param name="poco">
        ///     The POCO object whose primary key value will be used to delete the row (or null to use the supplied
        ///     primary key value)
        /// </param>
        /// <param name="primaryKeyValue">
        ///     The value of the primary key identifing the record to be deleted (or null, or get this
        ///     value from the POCO instance)
        /// </param>
        /// <returns>The number of rows affected</returns>
        int Delete(string tableName, string primaryKeyName, object poco, object primaryKeyValue);

        /// <summary>
        ///     Performs an SQL Delete
        /// </summary>
        /// <param name="poco">The POCO object specifying the table name and primary key value of the row to be deleted</param>
        /// <returns>The number of rows affected</returns>
        int Delete(object poco);

        /// <summary>
        ///     Performs an SQL Delete
        /// </summary>
        /// <typeparam name="T">The POCO class whose attributes identify the table and primary key to be used in the delete</typeparam>
        /// <param name="pocoOrPrimaryKey">The value of the primary key of the row to delete</param>
        /// <returns></returns>
        int Delete<T>(object pocoOrPrimaryKey);

        /// <summary>
        ///     Performs an SQL Delete
        /// </summary>
        /// <typeparam name="T">The POCO class who's attributes specify the name of the table to delete from</typeparam>
        /// <param name="sql">The SQL condition clause identifying the row to delete (ie: everything after "DELETE FROM tablename"</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>The number of affected rows</returns>
        int Delete<T>(string sql, params object[] args);

        /// <summary>
        ///     Performs an SQL Delete
        /// </summary>
        /// <typeparam name="T">The POCO class who's attributes specify the name of the table to delete from</typeparam>
        /// <param name="sql">
        ///     An SQL builder object representing the SQL condition clause identifying the row to delete (ie:
        ///     everything after "UPDATE tablename"
        /// </param>
        /// <returns>The number of affected rows</returns>
        int Delete<T>(Sql sql);

        /// <summary>
        ///     Check if a poco represents a new row
        /// </summary>
        /// <param name="primaryKeyName">The name of the primary key column</param>
        /// <param name="poco">The object instance whose "newness" is to be tested</param>
        /// <returns>True if the POCO represents a record already in the database</returns>
        /// <remarks>This method simply tests if the POCO's primary key column property has been set to something non-zero.</remarks>
        bool IsNew(string primaryKeyName, object poco);

        /// <summary>
        ///     Check if a poco represents a new row
        /// </summary>
        /// <param name="poco">The object instance whose "newness" is to be tested</param>
        /// <returns>True if the POCO represents a record already in the database</returns>
        /// <remarks>This method simply tests if the POCO's primary key column property has been set to something non-zero.</remarks>
        bool IsNew(object poco);

        /// <summary>
        ///     Saves a POCO by either performing either an SQL Insert or SQL Update
        /// </summary>
        /// <param name="tableName">The name of the table to be updated</param>
        /// <param name="primaryKeyName">The name of the primary key column</param>
        /// <param name="poco">The POCO object to be saved</param>
        void Save(string tableName, string primaryKeyName, object poco);

        /// <summary>
        ///     Saves a POCO by either performing either an SQL Insert or SQL Update
        /// </summary>
        /// <param name="poco">The POCO object to be saved</param>
        void Save(object poco);
    }


    /// <summary>
    ///     Represents the build configuration settings contract.
    /// </summary>
    public interface IBuildConfigurationSettings
    {
        /// <summary>
        ///     Sets the setting against the specified key.
        /// </summary>
        /// <param name="key">The setting's key.</param>
        /// <param name="value">The setting's value.</param>
        void SetSetting(string key, object value);

        /// <summary>
        ///     Tries to get the setting and calls the <paramref name="setSetting" /> to set the value if found.
        /// </summary>
        /// <typeparam name="T">The setting type.</typeparam>
        /// <param name="key">The setting's key.</param>
        /// <param name="setSetting">The set setting callback.</param>
        /// <param name="onFail">The on fail callback, called when no setting can be set.</param>
        void TryGetSetting<T>(string key, Action<T> setSetting, Action onFail = null);
    }


    /// <summary>
    ///     Specifies the database contract.
    /// </summary>
    public interface IDatabase : IDisposable, IQuery, IAlterPoco, IExecute, ITransactionAccessor
    {
        /// <summary>
        ///     Gets the default mapper. (Default is <see cref="ConventionMapper" />)
        /// </summary>
        /// <returns>
        ///     The default mapper.
        /// </returns>
        IMapper DefaultMapper { get; }

        /// <summary>
        ///     Gets the SQL of the last executed statement
        /// </summary>
        /// <returns>
        ///     The last executed SQL.
        /// </returns>
        string LastSQL { get; }

        /// <summary>
        ///     Gets the arguments to the last execute statement
        /// </summary>
        /// <returns>
        ///     The last executed SQL arguments.
        /// </returns>
        object[] LastArgs { get; }

        /// <summary>
        ///     Gets a formatted string describing the last executed SQL statement and it's argument values
        /// </summary>
        /// <returns>
        ///     The formatted string.
        /// </returns>
        string LastCommand { get; }

        /// <summary>
        ///     Gets or sets the enable auto select. (Default is True)
        /// </summary>
        /// <remarks>
        ///     When set to true, PetaPoco will automatically create the "SELECT columns" section of the query for any query which
        ///     is found to require them.
        /// </remarks>
        /// <returns>
        ///     True, if auto select is enabled; Else, false.
        /// </returns>
        bool EnableAutoSelect { get; set; }

        /// <summary>
        ///     Gets the flag for whether named params are enabled. (Default is True)
        /// </summary>
        /// <remarks>
        ///     When set to true, parameters can be named ?myparam and populated from properties of the passed in argument values.
        /// </remarks>
        /// <returns>
        ///     True, if named parameters are enabled; Else, false.
        /// </returns>
        bool EnableNamedParams { get; set; }

        /// <summary>
        ///     Sets the timeout value, in seconds, which PetaPoco applies to all <see cref="IDbCommand.CommandTimeout" />.
        ///     (Default is 0)
        /// </summary>
        /// <remarks>
        ///     If the current value is zero PetaPoco will not set the command timeout, and therefor, the .net default (30 seconds)
        ///     will be in affect.
        /// </remarks>
        /// <returns>
        ///     The current command timeout.
        /// </returns>
        int CommandTimeout { get; set; }

        /// <summary>
        ///     Sets the timeout value for the next (and only next) SQL statement.
        /// </summary>
        /// <remarks>
        ///     This is a one-time settings, which after use, will return the <see cref="CommandTimeout" /> setting.
        /// </remarks>
        /// <returns>
        ///     The one time command timeout.
        /// </returns>
        int OneTimeCommandTimeout { get; set; }

        /// <summary>
        ///     Gets the current <seealso cref="Provider" />.
        /// </summary>
        /// <returns>
        ///     The current database provider.
        /// </returns>
        IProvider Provider { get; }

        /// <summary>
        ///     Gets the connection string.
        /// </summary>
        /// <returns>
        ///     The connection string.
        /// </returns>
        string ConnectionString { get; }

        /// <summary>
        ///     Gets or sets the transaction isolation level.
        /// </summary>
        /// <remarks>
        ///     When value is null, the underlying providers default isolation level is used.
        /// </remarks>
        IsolationLevel? IsolationLevel { get; set; }

        /// <summary>
        ///     Starts or continues a transaction.
        /// </summary>
        /// <returns>An ITransaction reference that must be Completed or disposed</returns>
        /// <remarks>
        ///     This method makes management of calls to Begin/End/CompleteTransaction easier.
        ///     The usage pattern for this should be:
        ///     using (var tx = db.GetTransaction())
        ///     {
        ///     // Do stuff
        ///     db.Update(...);
        ///     // Mark the transaction as complete
        ///     tx.Complete();
        ///     }
        ///     Transactions can be nested but they must all be completed otherwise the entire
        ///     transaction is aborted.
        /// </remarks>
        ITransaction GetTransaction();

        /// <summary>
        ///     Starts a transaction scope, see GetTransaction() for recommended usage
        /// </summary>
        void BeginTransaction();

        /// <summary>
        ///     Aborts the entire outer most transaction scope
        /// </summary>
        /// <remarks>
        ///     Called automatically by Transaction.Dispose()
        ///     if the transaction wasn't completed.
        /// </remarks>
        void AbortTransaction();

        /// <summary>
        ///     Marks the current transaction scope as complete.
        /// </summary>
        void CompleteTransaction();
    }


    /// <summary>
    ///     A helper interface which enables fluent configuration extension methods.
    /// </summary>
    public interface IDatabaseBuildConfiguration
    {

    }


    public interface IExecute
    {
        /// <summary>
        ///     Executes a non-query command
        /// </summary>
        /// <param name="sql">The SQL statement to execute</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>The number of rows affected</returns>
        int Execute(string sql, params object[] args);

        /// <summary>
        ///     Executes a non-query command
        /// </summary>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>The number of rows affected</returns>
        int Execute(Sql sql);

        /// <summary>
        ///     Executes a query and return the first column of the first row in the result set.
        /// </summary>
        /// <typeparam name="T">The type that the result value should be cast to</typeparam>
        /// <param name="sql">The SQL query to execute</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>The scalar value cast to T</returns>
        T ExecuteScalar<T>(string sql, params object[] args);

        /// <summary>
        ///     Executes a query and return the first column of the first row in the result set.
        /// </summary>
        /// <typeparam name="T">The type that the result value should be cast to</typeparam>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>The scalar value cast to T</returns>
        T ExecuteScalar<T>(Sql sql);
    }


    /// <summary>
    ///     An interface used to hide the 4 System.Object instance methods from the API in Visual Studio intellisense.
    /// </summary>
    /// <remarks>
    ///     Reference Project: MircoLite ORM (https://github.com/TrevorPilley/MicroLite)
    ///     Author: Trevor Pilley
    ///     Source: https://github.com/TrevorPilley/MicroLite/blob/develop/MicroLite/IHideObjectMethods.cs
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IHideObjectMethods
    {
        /// <summary>
        ///     Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool Equals(object other);

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        int GetHashCode();

        /// <summary>
        ///     Gets the type.
        /// </summary>
        /// <returns>The type of the object.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "The method is defined on System.Object, this interface is just to hide it from intelisense in Visual Studio")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "GetType",
            Justification = "The method is defined on System.Object, this interface is just to hide it from intelisense in Visual Studio")]
        Type GetType();

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        string ToString();
    }


    public interface IQuery
    {
        /// <summary>
        ///     Runs an SQL query, returning the results as an IEnumerable collection
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="sql">The SQL query</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL statement</param>
        /// <returns>An enumerable collection of result records</returns>
        /// <remarks>
        ///     For some DB providers, care should be taken to not start a new Query before finishing with
        ///     and disposing the previous one. In cases where this is an issue, consider using Fetch which
        ///     returns the results as a List rather than an IEnumerable.
        /// </remarks>
        IEnumerable<T> Query<T>(string sql, params object[] args);

        /// <summary>
        ///     Runs an SQL query, returning the results as an IEnumerable collection
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="sql">An SQL builder object representing the base SQL query and it's arguments</param>
        /// <returns>An enumerable collection of result records</returns>
        /// <remarks>
        ///     For some DB providers, care should be taken to not start a new Query before finishing with
        ///     and disposing the previous one. In cases where this is an issue, consider using Fetch which
        ///     returns the results as a List rather than an IEnumerable.
        /// </remarks>
        IEnumerable<T> Query<T>(Sql sql);

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        IEnumerable<TRet> Query<T1, T2, TRet>(Func<T1, T2, TRet> cb, string sql, params object[] args);

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        IEnumerable<TRet> Query<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, string sql, params object[] args);

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        IEnumerable<TRet> Query<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, string sql, params object[] args);

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="T5">The fifth POCO type</typeparam>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        IEnumerable<TRet> Query<T1, T2, T3, T4, T5, TRet>(Func<T1, T2, T3, T4, T5, TRet> cb, string sql, params object[] args);

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        IEnumerable<TRet> Query<T1, T2, TRet>(Func<T1, T2, TRet> cb, Sql sql);

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        IEnumerable<TRet> Query<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, Sql sql);

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        IEnumerable<TRet> Query<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, Sql sql);

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="T5">The fifth POCO type</typeparam>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        IEnumerable<TRet> Query<T1, T2, T3, T4, T5, TRet>(Func<T1, T2, T3, T4, T5, TRet> cb, Sql sql);

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        IEnumerable<T1> Query<T1, T2>(string sql, params object[] args);

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        IEnumerable<T1> Query<T1, T2, T3>(string sql, params object[] args);

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        IEnumerable<T1> Query<T1, T2, T3, T4>(string sql, params object[] args);

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="T5">The fifth POCO type</typeparam>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        IEnumerable<T1> Query<T1, T2, T3, T4, T5>(string sql, params object[] args);

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        IEnumerable<T1> Query<T1, T2>(Sql sql);

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        IEnumerable<T1> Query<T1, T2, T3>(Sql sql);

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        IEnumerable<T1> Query<T1, T2, T3, T4>(Sql sql);

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="T5">The fifth POCO type</typeparam>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        IEnumerable<T1> Query<T1, T2, T3, T4, T5>(Sql sql);

        /// <summary>
        ///     Performs a multi-poco query
        /// </summary>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="types">An array of Types representing the POCO types of the returned result set.</param>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        IEnumerable<TRet> Query<TRet>(Type[] types, object cb, string sql, params object[] args);

        /// <summary>
        ///     Runs a query and returns the result set as a typed list
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="sql">The SQL query to execute</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A List holding the results of the query</returns>
        List<T> Fetch<T>(string sql, params object[] args);

        /// <summary>
        ///     Runs a query and returns the result set as a typed list
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A List holding the results of the query</returns>
        List<T> Fetch<T>(Sql sql);

        /// <summary>
        ///     Retrieves a page of records	and the total number of available records
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="page">The 1 based page number to retrieve</param>
        /// <param name="itemsPerPage">The number of records per page</param>
        /// <param name="sqlCount">The SQL to retrieve the total number of records</param>
        /// <param name="countArgs">Arguments to any embedded parameters in the sqlCount statement</param>
        /// <param name="sqlPage">The SQL To retrieve a single page of results</param>
        /// <param name="pageArgs">Arguments to any embedded parameters in the sqlPage statement</param>
        /// <returns>A Page of results</returns>
        /// <remarks>
        ///     This method allows separate SQL statements to be explicitly provided for the two parts of the page query.
        ///     The page and itemsPerPage parameters are not used directly and are used simply to populate the returned Page
        ///     object.
        /// </remarks>
        Page<T> Page<T>(long page, long itemsPerPage, string sqlCount, object[] countArgs, string sqlPage, object[] pageArgs);

        /// <summary>
        ///     Retrieves a page of records	and the total number of available records
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="page">The 1 based page number to retrieve</param>
        /// <param name="itemsPerPage">The number of records per page</param>
        /// <param name="sql">The base SQL query</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL statement</param>
        /// <returns>A Page of results</returns>
        /// <remarks>
        ///     PetaPoco will automatically modify the supplied SELECT statement to only retrieve the
        ///     records for the specified page.  It will also execute a second query to retrieve the
        ///     total number of records in the result set.
        /// </remarks>
        Page<T> Page<T>(long page, long itemsPerPage, string sql, params object[] args);

        /// <summary>
        ///     Retrieves a page of records	and the total number of available records
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="page">The 1 based page number to retrieve</param>
        /// <param name="itemsPerPage">The number of records per page</param>
        /// <param name="sql">An SQL builder object representing the base SQL query and it's arguments</param>
        /// <returns>A Page of results</returns>
        /// <remarks>
        ///     PetaPoco will automatically modify the supplied SELECT statement to only retrieve the
        ///     records for the specified page.  It will also execute a second query to retrieve the
        ///     total number of records in the result set.
        /// </remarks>
        Page<T> Page<T>(long page, long itemsPerPage, Sql sql);

        /// <summary>
        ///     Retrieves a page of records	and the total number of available records
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="page">The 1 based page number to retrieve</param>
        /// <param name="itemsPerPage">The number of records per page</param>
        /// <param name="sqlCount">An SQL builder object representing the SQL to retrieve the total number of records</param>
        /// <param name="sqlPage">An SQL builder object representing the SQL to retrieve a single page of results</param>
        /// <returns>A Page of results</returns>
        /// <remarks>
        ///     This method allows separate SQL statements to be explicitly provided for the two parts of the page query.
        ///     The page and itemsPerPage parameters are not used directly and are used simply to populate the returned Page
        ///     object.
        /// </remarks>
        Page<T> Page<T>(long page, long itemsPerPage, Sql sqlCount, Sql sqlPage);

        /// <summary>
        ///     Retrieves a page of records (without the total count)
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="page">The 1 based page number to retrieve</param>
        /// <param name="itemsPerPage">The number of records per page</param>
        /// <param name="sql">The base SQL query</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL statement</param>
        /// <returns>A List of results</returns>
        /// <remarks>
        ///     PetaPoco will automatically modify the supplied SELECT statement to only retrieve the
        ///     records for the specified page.
        /// </remarks>
        List<T> Fetch<T>(long page, long itemsPerPage, string sql, params object[] args);

        /// <summary>
        ///     Retrieves a page of records (without the total count)
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="page">The 1 based page number to retrieve</param>
        /// <param name="itemsPerPage">The number of records per page</param>
        /// <param name="sql">An SQL builder object representing the base SQL query and it's arguments</param>
        /// <returns>A List of results</returns>
        /// <remarks>
        ///     PetaPoco will automatically modify the supplied SELECT statement to only retrieve the
        ///     records for the specified page.
        /// </remarks>
        List<T> Fetch<T>(long page, long itemsPerPage, Sql sql);

        /// <summary>
        ///     Retrieves a range of records from result set
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="skip">The number of rows at the start of the result set to skip over</param>
        /// <param name="take">The number of rows to retrieve</param>
        /// <param name="sql">The base SQL query</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL statement</param>
        /// <returns>A List of results</returns>
        /// <remarks>
        ///     PetaPoco will automatically modify the supplied SELECT statement to only retrieve the
        ///     records for the specified range.
        /// </remarks>
        List<T> SkipTake<T>(long skip, long take, string sql, params object[] args);

        /// <summary>
        ///     Retrieves a range of records from result set
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="skip">The number of rows at the start of the result set to skip over</param>
        /// <param name="take">The number of rows to retrieve</param>
        /// <param name="sql">An SQL builder object representing the base SQL query and it's arguments</param>
        /// <returns>A List of results</returns>
        /// <remarks>
        ///     PetaPoco will automatically modify the supplied SELECT statement to only retrieve the
        ///     records for the specified range.
        /// </remarks>
        List<T> SkipTake<T>(long skip, long take, Sql sql);

        /// <summary>
        ///     Checks for the existence of a row with the specified primary key value.
        /// </summary>
        /// <typeparam name="T">The Type representing the table being queried</typeparam>
        /// <param name="primaryKey">The primary key value to look for</param>
        /// <returns>True if a record with the specified primary key value exists.</returns>
        bool Exists<T>(object primaryKey);

        /// <summary>
        ///     Checks for the existence of a row matching the specified condition
        /// </summary>
        /// <typeparam name="T">The Type representing the table being queried</typeparam>
        /// <param name="sqlCondition">The SQL expression to be tested for (ie: the WHERE expression)</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL statement</param>
        /// <returns>True if a record matching the condition is found.</returns>
        bool Exists<T>(string sqlCondition, params object[] args);

        /// <summary>
        ///     Returns the record with the specified primary key value
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="primaryKey">The primary key value of the record to fetch</param>
        /// <returns>The single record matching the specified primary key value</returns>
        /// <remarks>
        ///     Throws an exception if there are zero or more than one record with the specified primary key value.
        /// </remarks>
        T Single<T>(object primaryKey);

        /// <summary>
        ///     Runs a query that should always return a single row.
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="sql">The SQL query</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL statement</param>
        /// <returns>The single record matching the specified primary key value</returns>
        /// <remarks>
        ///     Throws an exception if there are zero or more than one matching record
        /// </remarks>
        T Single<T>(string sql, params object[] args);

        /// <summary>
        ///     Runs a query that should always return a single row.
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>The single record matching the specified primary key value</returns>
        /// <remarks>
        ///     Throws an exception if there are zero or more than one matching record
        /// </remarks>
        T Single<T>(Sql sql);

        /// <summary>
        ///     Runs a query that should always return either a single row, or no rows
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>The single record matching the specified primary key value, or default(T) if no matching rows</returns>
        T SingleOrDefault<T>(Sql sql);

        /// <summary>
        ///     Returns the record with the specified primary key value, or the default value if not found
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="primaryKey">The primary key value of the record to fetch</param>
        /// <returns>The single record matching the specified primary key value</returns>
        /// <remarks>
        ///     If there are no records with the specified primary key value, default(T) (typically null) is returned.
        /// </remarks>
        T SingleOrDefault<T>(object primaryKey);

        /// <summary>
        ///     Runs a query that should always return either a single row, or no rows
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="sql">The SQL query</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL statement</param>
        /// <returns>The single record matching the specified primary key value, or default(T) if no matching rows</returns>
        T SingleOrDefault<T>(string sql, params object[] args);

        /// <summary>
        ///     Runs a query that should always return at least one return
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="sql">The SQL query</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL statement</param>
        /// <returns>The first record in the result set</returns>
        T First<T>(string sql, params object[] args);

        /// <summary>
        ///     Runs a query that should always return at least one return
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>The first record in the result set</returns>
        T First<T>(Sql sql);

        /// <summary>
        ///     Runs a query and returns the first record, or the default value if no matching records
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="sql">The SQL query</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL statement</param>
        /// <returns>The first record in the result set, or default(T) if no matching rows</returns>
        T FirstOrDefault<T>(string sql, params object[] args);

        /// <summary>
        ///     Runs a query and returns the first record, or the default value if no matching records
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>The first record in the result set, or default(T) if no matching rows</returns>
        T FirstOrDefault<T>(Sql sql);

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="TRet">The returned list POCO type</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as a List</returns>
        List<TRet> Fetch<T1, T2, TRet>(Func<T1, T2, TRet> cb, string sql, params object[] args);

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="TRet">The returned list POCO type</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as a List</returns>
        List<TRet> Fetch<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, string sql, params object[] args);

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="TRet">The returned list POCO type</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as a List</returns>
        List<TRet> Fetch<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, string sql, params object[] args);

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="T5">The fifth POCO type</typeparam>
        /// <typeparam name="TRet">The returned list POCO type</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as a List</returns>
        List<TRet> Fetch<T1, T2, T3, T4, T5, TRet>(Func<T1, T2, T3, T4, T5, TRet> cb, string sql, params object[] args);

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="TRet">The returned list POCO type</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as a List</returns>
        List<TRet> Fetch<T1, T2, TRet>(Func<T1, T2, TRet> cb, Sql sql);

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="TRet">The returned list POCO type</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as a List</returns>
        List<TRet> Fetch<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, Sql sql);

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="TRet">The returned list POCO type</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as a List</returns>
        List<TRet> Fetch<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, Sql sql);

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="T5">The fifth POCO type</typeparam>
        /// <typeparam name="TRet">The returned list POCO type</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as a List</returns>
        List<TRet> Fetch<T1, T2, T3, T4, T5, TRet>(Func<T1, T2, T3, T4, T5, TRet> cb, Sql sql);

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as a List</returns>
        List<T1> Fetch<T1, T2>(string sql, params object[] args);

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as a List</returns>
        List<T1> Fetch<T1, T2, T3>(string sql, params object[] args);

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as a List</returns>
        List<T1> Fetch<T1, T2, T3, T4>(string sql, params object[] args);

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="T5">The fifth POCO type</typeparam>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>A collection of POCO's as a List</returns>
        List<T1> Fetch<T1, T2, T3, T4, T5>(string sql, params object[] args);

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as a List</returns>
        List<T1> Fetch<T1, T2>(Sql sql);

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as a List</returns>
        List<T1> Fetch<T1, T2, T3>(Sql sql);

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as a List</returns>
        List<T1> Fetch<T1, T2, T3, T4>(Sql sql);

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="T5">The fourth POCO type</typeparam>
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param>
        /// <returns>A collection of POCO's as a List</returns>
        List<T1> Fetch<T1, T2, T3, T4, T5>(Sql sql);

        /// <summary> 
        /// Perform a multi-results set query 
        /// </summary> 
        /// <param name="sql">An SQL builder object representing the query and it's arguments</param> 
        /// <returns>A GridReader to be queried</returns> 
        IGridReader QueryMultiple(Sql sql);


        /// <summary> 
        /// Perform a multi-results set query 
        /// </summary> 
        /// <param name="sql">The SQL query to be executed</param> 
        /// <param name="args">Arguments to any embedded parameters in the SQL</param> 
        /// <returns>A GridReader to be queried</returns>
        IGridReader QueryMultiple(string sql, params object[] args);
    }


    /// <summary>
    ///     Represents a contract which exposes the current <see cref="IDbTransaction" /> instance.
    /// </summary>
    public interface ITransactionAccessor
    {
        /// <summary>
        ///     Gets the current transaction instance.
        /// </summary>
        /// <returns>
        ///     The current transaction instance; else, <c>null</c> if not transaction is in progress.
        /// </returns>
        IDbTransaction Transaction { get; }
    }


    /* 
	Thanks to Adam Schroder (@schotime) for this.
	
	This extra file provides an implementation of DbProviderFactory for early versions of the Oracle
	drivers that don't include include it.  For later versions of Oracle, the standard OracleProviderFactory
	class should work fine
	
	Uses reflection to load Oracle.DataAccess assembly and in-turn create connections and commands
	
	Currently untested.
	
	Usage:   
		
			new PetaPoco.Database("<connstring>", new PetaPoco.OracleProvider())
	
	Or in your app/web config (be sure to change ASSEMBLYNAME to the name of your 
	assembly containing OracleProvider.cs)
	
		<connectionStrings>
			<add
				name="oracle"
				connectionString="WHATEVER"
				providerName="Oracle"
				/>
		</connectionStrings>

		<system.data>
			<DbProviderFactories>
				<add name="PetaPoco Oracle Provider" invariant="Oracle" description="PetaPoco Oracle Provider" 
								type="PetaPoco.OracleProvider, ASSEMBLYNAME" />
			</DbProviderFactories>
		</system.data>

	 */

    public class OracleProvider : DbProviderFactory
    {
        private const string _assemblyName = "Oracle.DataAccess";
        private const string _connectionTypeName = "Oracle.DataAccess.Client.OracleConnection";
        private const string _commandTypeName = "Oracle.DataAccess.Client.OracleCommand";
        private static Type _connectionType;
        private static Type _commandType;

        // Required for DbProviderFactories.GetFactory() to work.
        public static OracleProvider Instance = new OracleProvider();

        public OracleProvider()
        {
            _connectionType = TypeFromAssembly(_connectionTypeName, _assemblyName);
            _commandType = TypeFromAssembly(_commandTypeName, _assemblyName);
            if (_connectionType == null)
                throw new InvalidOperationException("Can't find Connection type: " + _connectionTypeName);
        }

        public override DbConnection CreateConnection()
        {
            return (DbConnection)Activator.CreateInstance(_connectionType);
        }

        public override DbCommand CreateCommand()
        {
            DbCommand command = (DbCommand)Activator.CreateInstance(_commandType);

            var oracleCommandBindByName = _commandType.GetProperty("BindByName");
            oracleCommandBindByName.SetValue(command, true, null);

            return command;
        }

        public static Type TypeFromAssembly(string typeName, string assemblyName)
        {
            try
            {
                // Try to get the type from an already loaded assembly
                Type type = Type.GetType(typeName);

                if (type != null)
                {
                    return type;
                }

                if (assemblyName == null)
                {
                    // No assembly was specified for the type, so just fail
                    string message = "Could not load type " + typeName + ". Possible cause: no assembly name specified.";
                    throw new TypeLoadException(message);
                }

                Assembly assembly = Assembly.Load(assemblyName);

                if (assembly == null)
                {
                    throw new InvalidOperationException("Can't find assembly: " + assemblyName);
                }

                type = assembly.GetType(typeName);

                if (type == null)
                {
                    return null;
                }

                return type;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }


    /// <summary>
    ///     Represents an attribute which can decorate a Poco property to mark the property as a column. It may also optionally
    ///     supply the DB column name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        /// <summary>
        ///     The SQL name of the column
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     True if time and date values returned through this column should be forced to UTC DateTimeKind. (no conversion is
        ///     applied - the Kind of the DateTime property
        ///     is simply set to DateTimeKind.Utc instead of DateTimeKind.Unknown.
        /// </summary>
        public bool ForceToUtc { get; set; }

        /// <summary>
        ///     The insert template. If not null, this template is used for generating the insert section instead of the deafult
        ///     string.Format("{0}{1}", paramPrefix, index"). Setting this allows DB related interactions, such as "CAST({0}{1} AS
        ///     json)"
        /// </summary>
        public string InsertTemplate { get; set; }

        /// <summary>
        ///     The update template. If not null, this template is used for generating the update section instead of the deafult
        ///     string.Format("{0} = {1}{2}", colName, paramPrefix, index"). Setting this allows DB related interactions, such as "{0} = CAST({1}{2} AS
        ///     json)"
        /// </summary>
        public string UpdateTemplate { get; set; }

        /// <summary>
        ///     Constructs a new instance of the <seealso cref="ColumnAttribute" />.
        /// </summary>
        public ColumnAttribute()
        {
            ForceToUtc = false;
        }

        /// <summary>
        ///     Constructs a new instance of the <seealso cref="ColumnAttribute" />.
        /// </summary>
        /// <param name="name">The name of the column.</param>
        public ColumnAttribute(string name)
        {
            Name = name;
            ForceToUtc = false;
        }
    }


    /// <summary>
    ///     Represents the attribute which decorates a poco class to state all columns must be explicitly mapped using either a
    ///     <seealso cref="ColumnAttribute" /> or <seealso cref="ResultColumnAttribute" />.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ExplicitColumnsAttribute : Attribute
    {
    }


    /// <summary>
    ///     Represents an attribute which can decorate a Poco property to ensure PetaPoco does not map column, and therefore
    ///     ignores the column.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreAttribute : Attribute
    {
    }


    /// <summary>
    ///     Is an attribute, which when applied to a Poco class, specifies primary key column. Additionally, specifies whether
    ///     the column is auto incrementing and the optional sequence name for Oracle sequence columns.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PrimaryKeyAttribute : Attribute
    {
        /// <summary>
        ///     The column name.
        /// </summary>
        /// <returns>
        ///     The column name.
        /// </returns>
        public string Value { get; private set; }

        /// <summary>
        ///     The sequence name.
        /// </summary>
        /// <returns>
        ///     The sequence name.
        /// </returns>
        public string SequenceName { get; set; }

        /// <summary>
        ///     A flag which specifies if the primary key is auto incrementing.
        /// </summary>
        /// <returns>
        ///     True if the primary key is auto incrementing; else, False.
        /// </returns>
        public bool AutoIncrement { get; set; }

        /// <summary>
        ///     Constructs a new instance of the <seealso cref="PrimaryKeyAttribute" />.
        /// </summary>
        /// <param name="primaryKey">The name of the primary key column.</param>
        public PrimaryKeyAttribute(string primaryKey)
        {
            Value = primaryKey;
            AutoIncrement = true;
        }
    }


    /// <summary>
    ///     Represents an attribute which can decorate a poco property as a result only column. A result only column is a
    ///     column that is only populated in queries and is not used for updates or inserts operations.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ResultColumnAttribute : ColumnAttribute
    {
        /// <summary>
        ///     Constructs a new instance of the <seealso cref="ResultColumnAttribute" />.
        /// </summary>
        public ResultColumnAttribute()
        {
        }

        /// <summary>
        ///     Constructs a new instance of the <seealso cref="ResultColumnAttribute" />.
        /// </summary>
        /// <param name="name">The name of the DB column.</param>
        public ResultColumnAttribute(string name)
            : base(name)
        {
        }
    }


    /// <summary>
    ///     Represents an attribute, which when applied to a Poco class, specifies the the DB table name which it maps to
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TableNameAttribute : Attribute
    {
        /// <summary>
        ///     The table nane of the database that this entity maps to.
        /// </summary>
        /// <returns>
        ///     The table nane of the database that this entity maps to.
        /// </returns>
        public string Value { get; private set; }

        /// <summary>
        ///     Constructs a new instance of the <seealso cref="TableNameAttribute" />.
        /// </summary>
        /// <param name="tableName">The table nane of the database that this entity maps to.</param>
        public TableNameAttribute(string tableName)
        {
            Value = tableName;
        }
    }


    /// <summary>
    /// Represents an attribute which can decorate a Poco property conver value from database type to property type and conversely.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class ValueConverterAttribute : Attribute
    {
        /// <summary>
        /// Function to convert property value to database type value.
        /// </summary>
        /// <param name="value">Property value</param>
        /// <returns>Converted database value</returns>
        public abstract object ConvertToDb(object value);
        /// <summary>
        /// Function to convert database value to property type value.
        /// </summary>
        /// <param name="value">Database value</param>
        /// <returns>Converted property type value</returns>
        public abstract object ConvertFromDb(object value);
    }


    /// <summary>
    ///     Wrap strings in an instance of this class to force use of DBType.AnsiString
    /// </summary>
    public class AnsiString
    {
        /// <summary>
        ///     The string value
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        ///     Constructs an AnsiString
        /// </summary>
        /// <param name="str">The C# string to be converted to ANSI before being passed to the DB</param>
        public AnsiString(string str)
        {
            Value = str;
        }
    }


    /// <summary>
    ///     Hold information about a column in the database.
    /// </summary>
    /// <remarks>
    ///     Typically ColumnInfo is automatically populated from the attributes on a POCO object and it's properties. It can
    ///     however also be returned from the IMapper interface to provide your owning bindings between the DB and your POCOs.
    /// </remarks>
    public class ColumnInfo
    {
        /// <summary>
        ///     The SQL name of the column
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        ///     True if this column returns a calculated value from the database and shouldn't be used in Insert and Update
        ///     operations.
        /// </summary>
        public bool ResultColumn { get; set; }

        /// <summary>
        ///     True if time and date values returned through this column should be forced to UTC DateTimeKind. (no conversion is
        ///     applied - the Kind of the DateTime property
        ///     is simply set to DateTimeKind.Utc instead of DateTimeKind.Unknown.
        /// </summary>
        public bool ForceToUtc { get; set; }

        /// <summary>
        ///     The insert template. If not null, this template is used for generating the insert section instead of the deafult
        ///     string.Format("{0}{1}", paramPrefix, index"). Setting this allows DB related interactions, such as "CAST({0}{1} AS
        ///     json)"
        /// </summary>
        public string InsertTemplate { get; set; }

        /// <summary>
        ///     The update template. If not null, this template is used for generating the update section instead of the deafult
        ///     string.Format("{0} = {1}{2}", colName, paramPrefix, index"). Setting this allows DB related interactions, such as "{0} = CAST({1}{2} AS
        ///     json)"
        /// </summary>
        public string UpdateTemplate { get; set; }

        /// <summary>
        ///     Creates and populates a ColumnInfo from the attributes of a POCO property.
        /// </summary>
        /// <param name="propertyInfo">The property whose column info is required</param>
        /// <returns>A ColumnInfo instance</returns>
        public static ColumnInfo FromProperty(PropertyInfo propertyInfo)
        {
            // Check if declaring poco has [Explicit] attribute
            var explicitColumns =
                propertyInfo.DeclaringType.GetCustomAttributes(typeof(ExplicitColumnsAttribute), true).Length > 0;

            // Check for [Column]/[Ignore] Attributes
            var colAttrs = propertyInfo.GetCustomAttributes(typeof(ColumnAttribute), true);
            if (explicitColumns)
            {
                if (colAttrs.Length == 0)
                    return null;
            }
            else
            {
                if (propertyInfo.GetCustomAttributes(typeof(IgnoreAttribute), true).Length != 0)
                    return null;
            }

            var ci = new ColumnInfo();

            // Read attribute
            if (colAttrs.Length > 0)
            {
                var colattr = (ColumnAttribute)colAttrs[0];
                ci.InsertTemplate = colattr.InsertTemplate;
                ci.UpdateTemplate = colattr.UpdateTemplate;
                ci.ColumnName = colattr.Name == null ? propertyInfo.Name : colattr.Name;
                ci.ForceToUtc = colattr.ForceToUtc;
                if ((colattr as ResultColumnAttribute) != null)
                    ci.ResultColumn = true;
            }
            else
            {
                ci.ColumnName = propertyInfo.Name;
                ci.ForceToUtc = false;
                ci.ResultColumn = false;
            }

            return ci;
        }
    }


    /// <summary>
    ///     Represents a configurable convention mapper.
    /// </summary>
    /// <remarks>
    ///     By default this mapper replaces <see cref="StandardMapper" /> without change, which means backwards compatibility
    ///     is kept.
    /// </remarks>
    public class ConventionMapper : IMapper
    {
        /// <summary>
        ///     Gets or sets the get sequence name logic.
        /// </summary>
        public Func<Type, PropertyInfo, string> GetSequenceName { get; set; }

        /// <summary>
        ///     Gets or sets the inflect column name logic.
        /// </summary>
        public Func<IInflector, string, string> InflectColumnName { get; set; }

        /// <summary>
        ///     Gets or sets the inflect table name logic.
        /// </summary>
        public Func<IInflector, string, string> InflectTableName { get; set; }

        /// <summary>
        ///     Gets or sets the is primary key auto increment logic.
        /// </summary>
        public Func<Type, bool> IsPrimaryKeyAutoIncrement { get; set; }

        /// <summary>
        ///     Gets or sets the map column logic.
        /// </summary>
        public Func<ColumnInfo, Type, PropertyInfo, bool> MapColumn { get; set; }

        /// <summary>
        ///     Gets or set the map primary key logic.
        /// </summary>
        public Func<TableInfo, Type, bool> MapPrimaryKey { get; set; }

        /// <summary>
        ///     Gets or sets the map table logic.
        /// </summary>
        public Func<TableInfo, Type, bool> MapTable { get; set; }

        /// <summary>
        ///     Gets or sets the from db convert logic.
        /// </summary>
        public Func<PropertyInfo, Type, Func<object, object>> FromDbConverter { get; set; }

        /// <summary>
        ///     Gets or sets the to db converter logic.
        /// </summary>
        public Func<PropertyInfo, Func<object, object>> ToDbConverter { get; set; }

        /// <summary>
        ///     Constructs a new instance of convention mapper.
        /// </summary>
        public ConventionMapper()
        {
            GetSequenceName = (t, pi) => null;
            InflectColumnName = (inflect, cn) => cn;
            InflectTableName = (inflect, tn) => tn;
            MapPrimaryKey = (ti, t) =>
            {
                var primaryKey = t.GetCustomAttributes(typeof(PrimaryKeyAttribute), true).FirstOrDefault() as PrimaryKeyAttribute;

                if (primaryKey != null)
                {
                    ti.PrimaryKey = primaryKey.Value;
                    ti.SequenceName = primaryKey.SequenceName;
                    ti.AutoIncrement = primaryKey.AutoIncrement;
                    return true;
                }

                var prop = t.GetProperties().FirstOrDefault(p =>
                {
                    if (p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                        return true;
                    if (p.Name.Equals(t.Name + "Id", StringComparison.OrdinalIgnoreCase))
                        return true;
                    if (p.Name.Equals(t.Name + "_Id", StringComparison.OrdinalIgnoreCase))
                        return true;
                    return false;
                });

                if (prop == null)
                    return false;

                ti.PrimaryKey = InflectColumnName(Inflector.Instance, prop.Name);
                ti.AutoIncrement = IsPrimaryKeyAutoIncrement(prop.PropertyType);
                ti.SequenceName = GetSequenceName(t, prop);
                return true;
            };
            MapTable = (ti, t) =>
            {
                var tableName = t.GetCustomAttributes(typeof(TableNameAttribute), true).FirstOrDefault() as TableNameAttribute;
                ti.TableName = tableName != null ? tableName.Value : InflectTableName(Inflector.Instance, t.Name);
                MapPrimaryKey(ti, t);
                return true;
            };
            IsPrimaryKeyAutoIncrement = t =>
            {
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                    t = t.GetGenericArguments()[0];

                if (t == typeof(long) || t == typeof(ulong))
                    return true;
                if (t == typeof(int) || t == typeof(uint))
                    return true;
                if (t == typeof(short) || t == typeof(ushort))
                    return true;

                return false;
            };
            MapColumn = (ci, t, pi) =>
            {
                // Check if declaring poco has [Explicit] attribute
                var isExplicit = t.GetCustomAttributes(typeof(ExplicitColumnsAttribute), true).Any();

                // Check for [Column]/[Ignore] Attributes
                var column = pi.GetCustomAttributes(typeof(ColumnAttribute), true).FirstOrDefault() as ColumnAttribute;

                if (isExplicit && column == null)
                    return false;

                if (pi.GetCustomAttributes(typeof(IgnoreAttribute), true).Any())
                    return false;

                // Read attribute
                if (column != null)
                {
                    ci.ColumnName = column.Name ?? InflectColumnName(Inflector.Instance, pi.Name);
                    ci.ForceToUtc = column.ForceToUtc;
                    ci.ResultColumn = (column as ResultColumnAttribute) != null;
                    ci.InsertTemplate = column.InsertTemplate;
                    ci.UpdateTemplate = column.UpdateTemplate;
                }
                else
                {
                    ci.ColumnName = InflectColumnName(Inflector.Instance, pi.Name);
                }

                return true;
            };
            FromDbConverter = (pi, t) =>
            {
                if (pi != null)
                {
                    var valueConverter = pi.GetCustomAttributes(typeof(ValueConverterAttribute), true).FirstOrDefault() as ValueConverterAttribute;
                    if (valueConverter != null)
                        return valueConverter.ConvertFromDb;
                }
                return null;
            };
            ToDbConverter = (pi) =>
            {
                if (pi != null)
                {
                    var valueConverter = pi.GetCustomAttributes(typeof(ValueConverterAttribute), true).FirstOrDefault() as ValueConverterAttribute;
                    if (valueConverter != null)
                        return valueConverter.ConvertToDb;
                }
                return null;
            };
        }

        /// <summary>
        ///     Get information about the table associated with a POCO class
        /// </summary>
        /// <param name="pocoType">The poco type.</param>
        /// <returns>A TableInfo instance</returns>
        /// <remarks>
        ///     This method must return a valid TableInfo.
        ///     To create a TableInfo from a POCO's attributes, use TableInfo.FromPoco
        /// </remarks>
        public TableInfo GetTableInfo(Type pocoType)
        {
            var ti = new TableInfo();
            return MapTable(ti, pocoType) ? ti : null;
        }

        /// <summary>
        ///     Get information about the column associated with a property of a POCO
        /// </summary>
        /// <param name="pocoProperty">The PropertyInfo of the property being queried</param>
        /// <returns>A reference to a ColumnInfo instance, or null to ignore this property</returns>
        /// <remarks>
        ///     To create a ColumnInfo from a property's attributes, use PropertyInfo.FromProperty
        /// </remarks>
        public ColumnInfo GetColumnInfo(PropertyInfo pocoProperty)
        {
            var ci = new ColumnInfo();
            return MapColumn(ci, pocoProperty.DeclaringType, pocoProperty) ? ci : null;
        }

        /// <summary>
        ///     Supply a function to convert a database value to the correct property value
        /// </summary>
        /// <param name="targetProperty">The target property</param>
        /// <param name="sourceType">The type of data returned by the DB</param>
        /// <returns>A Func that can do the conversion, or null for no conversion</returns>
        public Func<object, object> GetFromDbConverter(PropertyInfo targetProperty, Type sourceType)
        {
            return FromDbConverter != null ? FromDbConverter(targetProperty, sourceType) : null;
        }

        /// <summary>
        ///     Supply a function to convert a property value into a database value
        /// </summary>
        /// <param name="sourceProperty">The property to be converted</param>
        /// <returns>A Func that can do the conversion</returns>
        /// <remarks>
        ///     This conversion is only used for converting values from POCO's that are
        ///     being Inserted or Updated.
        ///     Conversion is not available for parameter values passed directly to queries.
        /// </remarks>
        public Func<object, object> GetToDbConverter(PropertyInfo sourceProperty)
        {
            return ToDbConverter != null ? ToDbConverter(sourceProperty) : null;
        }
    }


    /// <summary>
    /// 数据库操作基类    
    /// Base class for DatabaseType handlers - provides default/common handling for different database engines
    /// </summary>
    public abstract class DatabaseProvider : IProvider
    {
        /// <summary>
        ///     Gets the DbProviderFactory for this database provider.
        /// </summary>
        /// <returns>The provider factory.</returns>
        public abstract DbProviderFactory GetFactory();

        /// <summary>
        ///     Gets a flag for whether the DB has native support for GUID/UUID.
        /// </summary>
        public virtual bool HasNativeGuidSupport
        {
            get { return false; }
        }

        /// <summary>
        ///     Gets the <seealso cref="IPagingHelper" /> this provider supplies.
        /// </summary>
        public virtual IPagingHelper PagingUtility
        {
            get { return PagingHelper.Instance; }
        }

        /// <summary>
        ///     Escape a tablename into a suitable format for the associated database provider.
        /// </summary>
        /// <param name="tableName">
        ///     The name of the table (as specified by the client program, or as attributes on the associated
        ///     POCO class.
        /// </param>
        /// <returns>The escaped table name</returns>
        public virtual string EscapeTableName(string tableName)
        {
            // Assume table names with "dot" are already escaped
            return tableName.IndexOf('.') >= 0 ? tableName : EscapeSqlIdentifier(tableName);
        }

        /// <summary>
        ///     Escape and arbitary SQL identifier into a format suitable for the associated database provider
        /// </summary>
        /// <param name="sqlIdentifier">The SQL identifier to be escaped</param>
        /// <returns>The escaped identifier</returns>
        public virtual string EscapeSqlIdentifier(string sqlIdentifier)
        {
            return string.Format("[{0}]", sqlIdentifier);
        }

        /// <summary>
        ///     Returns the prefix used to delimit parameters in SQL query strings.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>The providers character for prefixing a query parameter.</returns>
        public virtual string GetParameterPrefix(string connectionString)
        {
            return "@";
        }

        /// <summary>
        ///     Converts a supplied C# object value into a value suitable for passing to the database
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>The converted value</returns>
        public virtual object MapParameterValue(object value)
        {
            if (value is bool)
                return ((bool)value) ? 1 : 0;

            return value;
        }

        /// <summary>
        ///     Called immediately before a command is executed, allowing for modification of the IDbCommand before it's passed to
        ///     the database provider
        /// </summary>
        /// <param name="cmd"></param>
        public virtual void PreExecute(IDbCommand cmd)
        {
        }

        /// <summary>
        ///     Builds an SQL query suitable for performing page based queries to the database
        /// </summary>
        /// <param name="skip">The number of rows that should be skipped by the query</param>
        /// <param name="take">The number of rows that should be retruend by the query</param>
        /// <param name="parts">The original SQL query after being parsed into it's component parts</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL query</param>
        /// <returns>The final SQL query that should be executed.</returns>
        public virtual string BuildPageQuery(long skip, long take, SQLParts parts, ref object[] args)
        {
            var sql = string.Format("{0}\nLIMIT @{1} OFFSET @{2}", parts.Sql, args.Length, args.Length + 1);
            args = args.Concat(new object[] { take, skip }).ToArray();
            return sql;
        }

        /// <summary>
        ///     Returns an SQL Statement that can check for the existence of a row in the database.
        /// </summary>
        /// <returns></returns>
        public virtual string GetExistsSql()
        {
            return "SELECT COUNT(*) FROM {0} WHERE {1}";
        }

        /// <summary>
        ///     Return an SQL expression that can be used to populate the primary key column of an auto-increment column.
        /// </summary>
        /// <param name="tableInfo">Table info describing the table</param>
        /// <returns>An SQL expressions</returns>
        /// <remarks>See the Oracle database type for an example of how this method is used.</remarks>
        public virtual string GetAutoIncrementExpression(TableInfo tableInfo)
        {
            return null;
        }

        /// <summary>
        ///     Returns an SQL expression that can be used to specify the return value of auto incremented columns.
        /// </summary>
        /// <param name="primaryKeyName">The primary key of the row being inserted.</param>
        /// <returns>An expression describing how to return the new primary key value</returns>
        /// <remarks>See the SQLServer database provider for an example of how this method is used.</remarks>
        public virtual string GetInsertOutputClause(string primaryKeyName)
        {
            return string.Empty;
        }

        /// <summary>
        ///     Performs an Insert operation
        /// </summary>
        /// <param name="database">The calling Database object</param>
        /// <param name="cmd">The insert command to be executed</param>
        /// <param name="primaryKeyName">The primary key of the table being inserted into</param>
        /// <returns>The ID of the newly inserted record</returns>
        public virtual object ExecuteInsert(Database database, IDbCommand cmd, string primaryKeyName)
        {
            cmd.CommandText += ";\nSELECT @@IDENTITY AS NewID;";
            return ExecuteScalarHelper(database, cmd);
        }

        /// <summary>
        ///     Returns the .net standard conforming DbProviderFactory.
        /// </summary>
        /// <param name="assemblyQualifiedNames">The assembly qualified name of the provider factory.</param>
        /// <returns>The db provider factory.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="assemblyQualifiedNames" /> does not match a type.</exception>
        protected DbProviderFactory GetFactory(params string[] assemblyQualifiedNames)
        {
            Type ft = null;
            foreach (var assemblyName in assemblyQualifiedNames)
            {
                ft = Type.GetType(assemblyName);
                if (ft != null)
                    break;
            }

            if (ft == null)
                throw new ArgumentException("Could not load the " + GetType().Name + " DbProviderFactory.");

            return (DbProviderFactory)ft.GetField("Instance").GetValue(null);
        }

        /// <summary>
        ///     Look at the type and provider name being used and instantiate a suitable DatabaseType instance.
        /// </summary>
        /// <param name="type">The type name.</param>
        /// <param name="allowDefault">A flag that when set allows the default <see cref="SqlServerDatabaseProvider"/> to be returned if not match is found.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>The database provider.</returns>
        internal static IProvider Resolve(Type type, bool allowDefault, string connectionString)
        {
            var typeName = type.Name;

            // Try using type name first (more reliable)
            if (typeName.StartsWith("MySql"))
                return Singleton<MySqlDatabaseProvider>.Instance;
            if (typeName.StartsWith("MariaDb"))
                return Singleton<MariaDbDatabaseProvider>.Instance;
            if (typeName.StartsWith("SqlCe"))
                return Singleton<SqlServerCEDatabaseProviders>.Instance;
            if (typeName.StartsWith("Npgsql") || typeName.StartsWith("PgSql"))
                return Singleton<PostgreSQLDatabaseProvider>.Instance;
            if (typeName.StartsWith("Oracle"))
                return Singleton<OracleDatabaseProvider>.Instance;
            if (typeName.StartsWith("SQLite"))
                return Singleton<SQLiteDatabaseProvider>.Instance;
            if (typeName.Equals("SqlConnection") || typeName.Equals("SqlClientFactory"))
                return Singleton<SqlServerDatabaseProvider>.Instance;
            if (typeName.StartsWith("FbConnection") || typeName.EndsWith("FirebirdClientFactory"))
                return Singleton<FirebirdDbDatabaseProvider>.Instance;

            if (!allowDefault)
                throw new ArgumentException("Could not match `" + type.FullName + "` to a provider.", "type");

            // Assume SQL Server
            return Singleton<SqlServerDatabaseProvider>.Instance;
        }

        /// <summary>
        ///     Look at the type and provider name being used and instantiate a suitable DatabaseType instance.
        /// </summary>
        /// <param name="providerName">The provider name.</param>
        /// <param name="allowDefault">A flag that when set allows the default <see cref="SqlServerDatabaseProvider"/> to be returned if not match is found.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>The database type.</returns>
        internal static IProvider Resolve(string providerName, bool allowDefault, string connectionString)
        {
            // Try again with provider name
            if (providerName.IndexOf("MySql", StringComparison.InvariantCultureIgnoreCase) >= 0)
                return Singleton<MySqlDatabaseProvider>.Instance;
            if (providerName.IndexOf("MariaDb", StringComparison.InvariantCultureIgnoreCase) >= 0)
                return Singleton<MariaDbDatabaseProvider>.Instance;
            if (providerName.IndexOf("SqlServerCe", StringComparison.InvariantCultureIgnoreCase) >= 0 ||
                providerName.IndexOf("SqlCeConnection", StringComparison.InvariantCultureIgnoreCase) >= 0)
                return Singleton<SqlServerCEDatabaseProviders>.Instance;
            if (providerName.IndexOf("Npgsql", StringComparison.InvariantCultureIgnoreCase) >= 0
                || providerName.IndexOf("pgsql", StringComparison.InvariantCultureIgnoreCase) >= 0)
                return Singleton<PostgreSQLDatabaseProvider>.Instance;
            if (providerName.IndexOf("Oracle", StringComparison.InvariantCultureIgnoreCase) >= 0)
                return Singleton<OracleDatabaseProvider>.Instance;
            if (providerName.IndexOf("SQLite", StringComparison.InvariantCultureIgnoreCase) >= 0)
                return Singleton<SQLiteDatabaseProvider>.Instance;
            if (providerName.IndexOf("Firebird", StringComparison.InvariantCultureIgnoreCase) >= 0 ||
                providerName.IndexOf("FbConnection", StringComparison.InvariantCultureIgnoreCase) >= 0)
                return Singleton<FirebirdDbDatabaseProvider>.Instance;

            if (providerName.IndexOf("SqlServer", StringComparison.InvariantCultureIgnoreCase) >= 0 ||
                providerName.IndexOf("System.Data.SqlClient", StringComparison.InvariantCultureIgnoreCase) >= 0)
                return Singleton<SqlServerDatabaseProvider>.Instance;

            if (!allowDefault)
                throw new ArgumentException("Could not match `" + providerName + "` to a provider.", "providerName");

            // Assume SQL Server
            return Singleton<SqlServerDatabaseProvider>.Instance;
        }

        /// <summary>
        ///     Unwraps a wrapped <see cref="DbProviderFactory"/>.
        /// </summary>
        /// <param name="factory">The factory to unwrap.</param>
        /// <returns>The unwrapped factory or the original factory if no wrapping occurred.</returns>
        internal static DbProviderFactory Unwrap(DbProviderFactory factory)
        {
            var sp = factory as IServiceProvider;

            if (sp == null)
                return factory;

            var unwrapped = sp.GetService(factory.GetType()) as DbProviderFactory;
            return unwrapped == null ? factory : Unwrap(unwrapped);
        }

        protected void ExecuteNonQueryHelper(Database db, IDbCommand cmd)
        {
            db.DoPreExecute(cmd);
            cmd.ExecuteNonQuery();
            db.OnExecutedCommand(cmd);
        }

        protected object ExecuteScalarHelper(Database db, IDbCommand cmd)
        {
            db.DoPreExecute(cmd);
            object r = cmd.ExecuteScalar();
            db.OnExecutedCommand(cmd);
            return r;
        }


    }


    internal class ExpandoColumn : PocoColumn
    {
        public override void SetValue(object target, object val)
        {
            (target as IDictionary<string, object>)[ColumnName] = val;
        }

        public override object GetValue(object target)
        {
            object val = null;
            (target as IDictionary<string, object>).TryGetValue(ColumnName, out val);
            return val;
        }

        public override object ChangeType(object val)
        {
            return val;
        }
    }


    public class GridReader : IGridReader
    {
        private IDataReader _reader;
        private IDbCommand _command;
        private readonly Database _db;
        private readonly IMapper _defaultMapper;

        /// <summary>
        /// The control structure for a multi-result set query
        /// </summary>
        /// <param name="database"></param>
        /// <param name="command"></param>
        /// <param name="reader"></param>
        /// <param name="defaultMapper"></param>
        internal GridReader(Database database, IDbCommand command, IDataReader reader, IMapper defaultMapper)
        {
            _db = database;
            _command = command;
            _reader = reader;
            _defaultMapper = defaultMapper;
        }

        #region public Read<T> methods

        /// <summary>
        /// Reads from a GridReader, returning the results as an IEnumerable collection
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <returns>An enumerable collection of result records</returns>
        public IEnumerable<T> Read<T>()
        {
            return SinglePocoFromIDataReader<T>(_gridIndex);
        }

        /// <summary>
        /// Perform a multi-poco read from a GridReader
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        public IEnumerable<T1> Read<T1, T2>()
        {
            return MultiPocoFromIDataReader<T1>(_gridIndex, new Type[] { typeof(T1), typeof(T2) }, null);
        }

        /// <summary>
        /// Perform a multi-poco read from a GridReader
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        public IEnumerable<T1> Read<T1, T2, T3>()
        {
            return MultiPocoFromIDataReader<T1>(_gridIndex, new Type[] { typeof(T1), typeof(T2), typeof(T3) }, null);
        }

        /// <summary>
        /// Perform a multi-poco read from a GridReader
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The forth POCO type</typeparam>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        public IEnumerable<T1> Read<T1, T2, T3, T4>()
        {
            return MultiPocoFromIDataReader<T1>(_gridIndex,
                new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, null);
        }

        /// <summary>
        /// Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        public IEnumerable<TRet> Read<T1, T2, TRet>(Func<T1, T2, TRet> cb)
        {
            return MultiPocoFromIDataReader<TRet>(_gridIndex, new Type[] { typeof(T1), typeof(T2) }, cb);
        }

        /// <summary>
        /// Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        public IEnumerable<TRet> Read<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb)
        {
            return MultiPocoFromIDataReader<TRet>(_gridIndex, new Type[] { typeof(T1), typeof(T2), typeof(T3) }, cb);
        }

        /// <summary>
        /// Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The forth POCO type</typeparam>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        public IEnumerable<TRet> Read<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb)
        {
            return MultiPocoFromIDataReader<TRet>(_gridIndex,
                new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, cb);
        }

        #endregion

        #region PocoFromIDataReader

        /// <summary>
        /// Read data to a single poco
        /// </summary>
        /// <typeparam name="T">The type representing a row in the result set</typeparam>
        /// <param name="index">Reader row to be read from the underlying IDataReader</param>
        /// <returns></returns>
        private IEnumerable<T> SinglePocoFromIDataReader<T>(int index)
        {
            if (_reader == null)
                throw new ObjectDisposedException(GetType().FullName, "The data reader has been disposed");
            if (_consumed)
                throw new InvalidOperationException(
                    "Query results must be consumed in the correct order, and each result can only be consumed once");
            _consumed = true;

            var pd = PocoData.ForType(typeof(T), _defaultMapper);
            try
            {
                while (index == _gridIndex)
                {
                    var factory =
                        pd.GetFactory(_command.CommandText, _command.Connection.ConnectionString, 0, _reader.FieldCount,
                            _reader, _defaultMapper) as Func<IDataReader, T>;

                    while (true)
                    {
                        T poco;
                        try
                        {
                            if (!_reader.Read())
                                yield break;
                            poco = factory(_reader);
                        }
                        catch (Exception x)
                        {
                            if (_db.OnException(x))
                                throw;
                            yield break;
                        }

                        yield return poco;
                    }
                }
            }
            finally // finally so that First etc progresses things even when multiple rows
            {
                if (index == _gridIndex)
                {
                    NextResult();
                }
            }
        }

        /// <summary>
        /// Read data to multiple pocos
        /// </summary>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="index">Reader row to be read from the underlying IDataReader</param>
        /// <param name="types">An array of Types representing the POCO types of the returned result set.</param>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        private IEnumerable<TRet> MultiPocoFromIDataReader<TRet>(int index, Type[] types, object cb)
        {
            if (_reader == null)
                throw new ObjectDisposedException(GetType().FullName, "The data reader has been disposed");
            if (_consumed)
                throw new InvalidOperationException(
                    "Query results must be consumed in the correct order, and each result can only be consumed once");
            _consumed = true;

            try
            {
                var cmd = _command;
                var r = _reader;

                var factory = MultiPocoFactory.GetFactory<TRet>(types, cmd.Connection.ConnectionString, cmd.CommandText, r, _defaultMapper);
                if (cb == null)
                    cb = MultiPocoFactory.GetAutoMapper(types.ToArray());
                bool bNeedTerminator = false;

                while (true)
                {
                    TRet poco;
                    try
                    {
                        if (!r.Read())
                            break;
                        poco = factory(r, cb);
                    }
                    catch (Exception x)
                    {
                        if (_db.OnException(x))
                            throw;
                        yield break;
                    }

                    if (poco != null)
                        yield return poco;
                    else
                        bNeedTerminator = true;
                }
                if (bNeedTerminator)
                {
                    var poco = (TRet)(cb as Delegate).DynamicInvoke(new object[types.Length]);
                    if (poco != null)
                        yield return poco;
                    else
                        yield break;
                }
            }
            finally
            {
                if (index == _gridIndex)
                {
                    NextResult();
                }
            }
        }

        #endregion

        #region DataReader Management

        private int _gridIndex;
        private bool _consumed;

        /// <summary>
        /// Advance the IDataReader to the NextResult, if available
        /// </summary>
        private void NextResult()
        {
            if (!_reader.NextResult())
                return;
            _gridIndex++;
            _consumed = false;
        }

        /// <summary>
        /// Dispose the grid, closing and disposing both the underlying reader, command and shared connection
        /// </summary>
        public void Dispose()
        {
            if (_reader != null)
            {
                if (!_reader.IsClosed && _command != null)
                    _command.Cancel();
                _reader.Dispose();
                _reader = null;
            }

            if (_command != null)
            {
                _command.Dispose();
                _command = null;
            }
            _db.CloseSharedConnection();
        }

        #endregion
    }


    public interface IGridReader : IDisposable
    {
        /// <summary>
        /// Reads from a GridReader, returning the results as an IEnumerable collection
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <returns>An enumerable collection of result records</returns>
        IEnumerable<T> Read<T>();

        /// <summary>
        /// Perform a multi-poco read from a GridReader
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        IEnumerable<T1> Read<T1, T2>();

        /// <summary>
        /// Perform a multi-poco read from a GridReader
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        IEnumerable<T1> Read<T1, T2, T3>();

        /// <summary>
        /// Perform a multi-poco read from a GridReader
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The forth POCO type</typeparam>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        IEnumerable<T1> Read<T1, T2, T3, T4>();

        /// <summary>
        /// Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        IEnumerable<TRet> Read<T1, T2, TRet>(Func<T1, T2, TRet> cb);

        /// <summary>
        /// Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        IEnumerable<TRet> Read<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb);

        /// <summary>
        /// Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The forth POCO type</typeparam>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <returns>A collection of POCO's as an IEnumerable</returns>
        IEnumerable<TRet> Read<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb);
    }


    /// <summary>
    ///     IMapper provides a way to hook into PetaPoco's Database to POCO mapping mechanism to either
    ///     customize or completely replace it.
    /// </summary>
    /// <remarks>
    ///     To use this functionality, instantiate a class that implements IMapper and then pass it to
    ///     PetaPoco through the static method Mappers.Register()
    /// </remarks>
    public interface IMapper
    {
        /// <summary>
        ///     Get information about the table associated with a POCO class
        /// </summary>
        /// <param name="pocoType">The poco type.</param>
        /// <returns>A TableInfo instance</returns>
        /// <remarks>
        ///     This method must return a valid TableInfo.
        ///     To create a TableInfo from a POCO's attributes, use TableInfo.FromPoco
        /// </remarks>
        TableInfo GetTableInfo(Type pocoType);

        /// <summary>
        ///     Get information about the column associated with a property of a POCO
        /// </summary>
        /// <param name="pocoProperty">The PropertyInfo of the property being queried</param>
        /// <returns>A reference to a ColumnInfo instance, or null to ignore this property</returns>
        /// <remarks>
        ///     To create a ColumnInfo from a property's attributes, use PropertyInfo.FromProperty
        /// </remarks>
        ColumnInfo GetColumnInfo(PropertyInfo pocoProperty);

        /// <summary>
        ///     Supply a function to convert a database value to the correct property value
        /// </summary>
        /// <param name="targetProperty">The target property</param>
        /// <param name="sourceType">The type of data returned by the DB</param>
        /// <returns>A Func that can do the conversion, or null for no conversion</returns>
        Func<object, object> GetFromDbConverter(PropertyInfo targetProperty, Type sourceType);

        /// <summary>
        ///     Supply a function to convert a property value into a database value
        /// </summary>
        /// <param name="sourceProperty">The property to be converted</param>
        /// <returns>A Func that can do the conversion</returns>
        /// <remarks>
        ///     This conversion is only used for converting values from POCO's that are
        ///     being Inserted or Updated.
        ///     Conversion is not available for parameter values passed directly to queries.
        /// </remarks>
        Func<object, object> GetToDbConverter(PropertyInfo sourceProperty);
    }


    /// <summary>
    ///     Represents a contract for a database type provider.
    /// </summary>
    public interface IProvider
    {
        /// <summary>
        ///     Gets the <seealso cref="IPagingHelper" /> this provider supplies.
        /// </summary>
        IPagingHelper PagingUtility { get; }

        /// <summary>
        ///     Gets a flag for whether the DB has native support for GUID/UUID.
        /// </summary>
        bool HasNativeGuidSupport { get; }

        /// <summary>
        ///     Escape a tablename into a suitable format for the associated database provider.
        /// </summary>
        /// <param name="tableName">
        ///     The name of the table (as specified by the client program, or as attributes on the associated
        ///     POCO class.
        /// </param>
        /// <returns>The escaped table name</returns>
        string EscapeTableName(string tableName);

        /// <summary>
        ///     Escape and arbitary SQL identifier into a format suitable for the associated database provider
        /// </summary>
        /// <param name="sqlIdentifier">The SQL identifier to be escaped</param>
        /// <returns>The escaped identifier</returns>
        string EscapeSqlIdentifier(string sqlIdentifier);

        /// <summary>
        ///     Builds an SQL query suitable for performing page based queries to the database
        /// </summary>
        /// <param name="skip">The number of rows that should be skipped by the query</param>
        /// <param name="take">The number of rows that should be retruend by the query</param>
        /// <param name="parts">The original SQL query after being parsed into it's component parts</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL query</param>
        /// <returns>The final SQL query that should be executed.</returns>
        string BuildPageQuery(long skip, long take, SQLParts parts, ref object[] args);

        /// <summary>
        ///     Converts a supplied C# object value into a value suitable for passing to the database
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>The converted value</returns>
        object MapParameterValue(object value);

        /// <summary>
        ///     Called immediately before a command is executed, allowing for modification of the IDbCommand before it's passed to
        ///     the database provider
        /// </summary>
        /// <param name="cmd"></param>
        void PreExecute(IDbCommand cmd);

        /// <summary>
        ///     Returns an SQL Statement that can check for the existence of a row in the database.
        /// </summary>
        /// <returns></returns>
        string GetExistsSql();

        /// <summary>
        ///     Performs an Insert operation
        /// </summary>
        /// <param name="database">The calling Database object</param>
        /// <param name="cmd">The insert command to be executed</param>
        /// <param name="primaryKeyName">The primary key of the table being inserted into</param>
        /// <returns>The ID of the newly inserted record</returns>
        object ExecuteInsert(Database database, IDbCommand cmd, string primaryKeyName);

        /// <summary>
        ///     Returns an SQL expression that can be used to specify the return value of auto incremented columns.
        /// </summary>
        /// <param name="primaryKeyName">The primary key of the row being inserted.</param>
        /// <returns>An expression describing how to return the new primary key value</returns>
        /// <remarks>See the SQLServer database provider for an example of how this method is used.</remarks>
        string GetInsertOutputClause(string primaryKeyName);

        /// <summary>
        ///     Returns the prefix used to delimit parameters in SQL query strings.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>The providers character for prefixing a query parameter.</returns>
        string GetParameterPrefix(string connectionString);

        /// <summary>
        ///     Return an SQL expression that can be used to populate the primary key column of an auto-increment column.
        /// </summary>
        /// <param name="tableInfo">Table info describing the table</param>
        /// <returns>An SQL expressions</returns>
        /// <remarks>See the Oracle database type for an example of how this method is used.</remarks>
        string GetAutoIncrementExpression(TableInfo tableInfo);

        DbProviderFactory GetFactory();
    }


    /// <summary>
    ///     Represents the contract for the transaction.
    /// </summary>
    /// <remarks>
    ///     A PetaPoco helper to support transactions using the using syntax.
    /// </remarks>
    public interface ITransaction : IDisposable, IHideObjectMethods
    {
        /// <summary>
        ///  提交事务,出错则回滚事务
        ///  Completes the transaction. Not calling complete will cause the transaction to rollback on dispose.
        /// </summary>
        void Complete();
    }


    /// <summary>
    ///     This static manages registation of IMapper instances with PetaPoco
    /// </summary>
    public static class Mappers
    {
        private static Dictionary<object, IMapper> _mappers = new Dictionary<object, IMapper>();
        private static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        /// <summary>
        ///     Registers a mapper for all types in a specific assembly
        /// </summary>
        /// <param name="assembly">The assembly whose types are to be managed by this mapper</param>
        /// <param name="mapper">The IMapper implementation</param>
        public static void Register(Assembly assembly, IMapper mapper)
        {
            RegisterInternal(assembly, mapper);
        }

        /// <summary>
        ///     Registers a mapper for a single POCO type
        /// </summary>
        /// <param name="type">The type to be managed by this mapper</param>
        /// <param name="mapper">The IMapper implementation</param>
        public static void Register(Type type, IMapper mapper)
        {
            RegisterInternal(type, mapper);
        }

        /// <summary>
        ///     Remove all mappers for all types in a specific assembly
        /// </summary>
        /// <param name="assembly">The assembly whose mappers are to be revoked</param>
        public static void Revoke(Assembly assembly)
        {
            RevokeInternal(assembly);
        }

        /// <summary>
        ///     Remove the mapper for a specific type
        /// </summary>
        /// <param name="type">The type whose mapper is to be removed</param>
        public static void Revoke(Type type)
        {
            RevokeInternal(type);
        }

        /// <summary>
        ///     Revoke an instance of a mapper
        /// </summary>
        /// <param name="mapper">The IMapper to be revkoed</param>
        public static void Revoke(IMapper mapper)
        {
            _lock.EnterWriteLock();
            try
            {
                foreach (var i in _mappers.Where(kvp => kvp.Value == mapper).ToList())
                    _mappers.Remove(i.Key);
            }
            finally
            {
                _lock.ExitWriteLock();
                FlushCaches();
            }
        }

        /// <summary>
        ///     Revokes all registered mappers.
        /// </summary>
        public static void RevokeAll()
        {
            _lock.EnterWriteLock();
            try
            {
                _mappers.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
                FlushCaches();
            }
        }

        /// <summary>
        ///     Retrieve the IMapper implementation to be used for a specified POCO type.
        /// </summary>
        /// <param name="entityType">The entity type to get the mapper for.</param>
        /// <param name="defaultMapper">The default mapper to use when non is registered for the type.</param>
        /// <returns>The mapper for the given type.</returns>
        public static IMapper GetMapper(Type entityType, IMapper defaultMapper)
        {
            _lock.EnterReadLock();
            try
            {
                IMapper val;
                if (_mappers.TryGetValue(entityType, out val))
                    return val;
                if (_mappers.TryGetValue(entityType.Assembly, out val))
                    return val;

                return defaultMapper;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        private static void RegisterInternal(object typeOrAssembly, IMapper mapper)
        {
            _lock.EnterWriteLock();
            try
            {
                _mappers.Add(typeOrAssembly, mapper);
            }
            finally
            {
                _lock.ExitWriteLock();
                FlushCaches();
            }
        }

        private static void RevokeInternal(object typeOrAssembly)
        {
            _lock.EnterWriteLock();
            try
            {
                _mappers.Remove(typeOrAssembly);
            }
            finally
            {
                _lock.ExitWriteLock();
                FlushCaches();
            }
        }

        private static void FlushCaches()
        {
            // Whenever a mapper is registered or revoked, we have to assume any generated code is no longer valid.
            // Since this should be a rare occurrence, the simplest approach is to simply dump everything and start over.
            MultiPocoFactory.FlushCaches();
            PocoData.FlushCaches();
        }
    }


    internal class MultiPocoFactory
    {
        // Various cached stuff
        private static readonly Cache<Tuple<Type, ArrayKey<Type>, string, string>, object> MultiPocoFactories = new Cache<Tuple<Type, ArrayKey<Type>, string, string>, object>();

        private static readonly Cache<ArrayKey<Type>, object> AutoMappers = new Cache<ArrayKey<Type>, object>();

        // Instance data used by the Multipoco factory delegate - essentially a list of the nested poco factories to call
        private List<Delegate> _delegates;

        public Delegate GetItem(int index)
        {
            return _delegates[index];
        }

        // Automagically guess the property relationships between various POCOs and create a delegate that will set them up
        public static object GetAutoMapper(Type[] types)
        {
            // Build a key
            var key = new ArrayKey<Type>(types);

            return AutoMappers.Get(key, () =>
            {
                // Create a method
                var m = new DynamicMethod("petapoco_automapper", types[0], types, true);
                var il = m.GetILGenerator();

                for (var i = 1; i < types.Length; i++)
                {
                    var handled = false;
                    for (var j = i - 1; j >= 0; j--)
                    {
                        // Find the property
                        var candidates = (from p in types[j].GetProperties() where p.PropertyType == types[i] select p).ToArray();
                        if (!candidates.Any())
                            continue;
                        if (candidates.Length > 1)
                            throw new InvalidOperationException(string.Format("Can't auto join {0} as {1} has more than one property of type {0}", types[i],
                                types[j]));

                        // Generate code
                        il.Emit(OpCodes.Ldarg_S, j);
                        il.Emit(OpCodes.Ldarg_S, i);
                        il.Emit(OpCodes.Callvirt, candidates.First().GetSetMethod(true));
                        handled = true;
                    }

                    if (!handled)
                        throw new InvalidOperationException(string.Format("Can't auto join {0}", types[i]));
                }

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ret);

                // Cache it
                return m.CreateDelegate(Expression.GetFuncType(types.Concat(types.Take(1)).ToArray()));
            });
        }

        // Find the split point in a result set for two different pocos and return the poco factory for the first
        private static Delegate FindSplitPoint(Type typeThis, Type typeNext, string connectionString, string sql, IDataReader r, ref int pos, IMapper defaultMapper)
        {
            // Last?
            if (typeNext == null)
                return PocoData.ForType(typeThis, defaultMapper).GetFactory(sql, connectionString, pos, r.FieldCount - pos, r, defaultMapper);

            // Get PocoData for the two types
            var pdThis = PocoData.ForType(typeThis, defaultMapper);
            var pdNext = PocoData.ForType(typeNext, defaultMapper);

            // Find split point
            var firstColumn = pos;
            var usedColumns = new Dictionary<string, bool>();
            for (; pos < r.FieldCount; pos++)
            {
                // Split if field name has already been used, or if the field doesn't exist in current poco but does in the next
                var fieldName = r.GetName(pos);
                if (usedColumns.ContainsKey(fieldName) || (!pdThis.Columns.ContainsKey(fieldName) && pdNext.Columns.ContainsKey(fieldName)))
                {
                    return pdThis.GetFactory(sql, connectionString, firstColumn, pos - firstColumn, r, defaultMapper);
                }
                usedColumns.Add(fieldName, true);
            }

            throw new InvalidOperationException(string.Format("Couldn't find split point between {0} and {1}", typeThis, typeNext));
        }

        // Create a multi-poco factory
        private static Func<IDataReader, object, TRet> CreateMultiPocoFactory<TRet>(Type[] types, string connectionString, string sql, IDataReader r, IMapper defaultMapper)
        {
            var m = new DynamicMethod("petapoco_multipoco_factory", typeof(TRet), new[] { typeof(MultiPocoFactory), typeof(IDataReader), typeof(object) },
                typeof(MultiPocoFactory));
            var il = m.GetILGenerator();

            // Load the callback
            il.Emit(OpCodes.Ldarg_2);

            // Call each delegate
            var dels = new List<Delegate>();
            var pos = 0;
            for (var i = 0; i < types.Length; i++)
            {
                // Add to list of delegates to call
                var del = FindSplitPoint(types[i], i + 1 < types.Length ? types[i + 1] : null, connectionString, sql, r, ref pos, defaultMapper);
                dels.Add(del);

                // Get the delegate
                il.Emit(OpCodes.Ldarg_0); // callback,this
                il.Emit(OpCodes.Ldc_I4, i); // callback,this,Index
                il.Emit(OpCodes.Callvirt, typeof(MultiPocoFactory).GetMethod("GetItem")); // callback,Delegate
                il.Emit(OpCodes.Ldarg_1); // callback,delegate, datareader

                // Call Invoke
                var tDelInvoke = del.GetType().GetMethod("Invoke");
                il.Emit(OpCodes.Callvirt, tDelInvoke); // Poco left on stack
            }

            // By now we should have the callback and the N pocos all on the stack.  Call the callback and we're done
            il.Emit(OpCodes.Callvirt, Expression.GetFuncType(types.Concat(new[] { typeof(TRet) }).ToArray()).GetMethod("Invoke"));
            il.Emit(OpCodes.Ret);

            // Finish up
            return (Func<IDataReader, object, TRet>)m.CreateDelegate(typeof(Func<IDataReader, object, TRet>), new MultiPocoFactory() { _delegates = dels });
        }

        internal static void FlushCaches()
        {
            MultiPocoFactories.Flush();
            AutoMappers.Flush();
        }

        // Get (or create) the multi-poco factory for a query
        public static Func<IDataReader, object, TRet> GetFactory<TRet>(Type[] types, string connectionString, string sql, IDataReader r, IMapper defaultMapper)
        {
            var key = Tuple.Create(typeof(TRet), new ArrayKey<Type>(types), connectionString, sql);

            return
                (Func<IDataReader, object, TRet>)MultiPocoFactories.Get(key, () => CreateMultiPocoFactory<TRet>(types, connectionString, sql, r, defaultMapper));
        }
    }


    #region 分页结果
    /// <summary>
    /// 保存分页结果
    /// </summary>
    /// <typeparam name="T">The type of Poco in the returned result set</typeparam>
    public class Page<T>
    {
        /// <summary>
        /// 当前页
        /// </summary>
        public long CurrentPage { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public long TotalPages { get; set; }

        /// <summary>
        ///总记录数
        /// </summary>
        public long TotalItems { get; set; }

        /// <summary>
        /// 每页记录数
        /// </summary>
        public long ItemsPerPage { get; set; }

        /// <summary>
        /// 记录列表
        /// </summary>
        public List<T> Items { get; set; }

        /// <summary>
        ///     User property to hold anything.
        /// </summary>
        public object Context { get; set; }
    }

    #endregion

    public class PocoColumn
    {
        public string ColumnName;
        public bool ForceToUtc;
        public PropertyInfo PropertyInfo;
        public bool ResultColumn;
        public string InsertTemplate { get; set; }
        public string UpdateTemplate { get; set; }

        public virtual void SetValue(object target, object val)
        {
            PropertyInfo.SetValue(target, val, null);
        }

        public virtual object GetValue(object target)
        {
            return PropertyInfo.GetValue(target, null);
        }

        public virtual object ChangeType(object val)
        {
            var t = PropertyInfo.PropertyType;
            if (val.GetType().IsValueType && PropertyInfo.PropertyType.IsGenericType && PropertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                t = t.GetGenericArguments()[0];

            return Convert.ChangeType(val, t);
        }
    }


    public class PocoData
    {
        private static Cache<Type, PocoData> _pocoDatas = new Cache<Type, PocoData>();
        private static List<Func<object, object>> _converters = new List<Func<object, object>>();
        private static object _converterLock = new object();
        private static MethodInfo fnGetValue = typeof(IDataRecord).GetMethod("GetValue", new Type[] { typeof(int) });
        private static MethodInfo fnIsDBNull = typeof(IDataRecord).GetMethod("IsDBNull");
        private static FieldInfo fldConverters = typeof(PocoData).GetField("_converters", BindingFlags.Static | BindingFlags.GetField | BindingFlags.NonPublic);
        private static MethodInfo fnListGetItem = typeof(List<Func<object, object>>).GetProperty("Item").GetGetMethod();
        private static MethodInfo fnInvoke = typeof(Func<object, object>).GetMethod("Invoke");
        private Cache<Tuple<string, string, int, int>, Delegate> PocoFactories = new Cache<Tuple<string, string, int, int>, Delegate>();
        public Type Type;
        public string[] QueryColumns { get; private set; }

        public string[] UpdateColumns
        {
            // No need to cache as it's not used by PetaPoco internally
            get { return (from c in Columns where !c.Value.ResultColumn && c.Value.ColumnName != TableInfo.PrimaryKey select c.Key).ToArray(); }
        }

        public TableInfo TableInfo { get; private set; }
        public Dictionary<string, PocoColumn> Columns { get; private set; }

        public PocoData()
        {
        }

        public PocoData(Type type, IMapper defaultMapper)
        {
            Type = type;

            // Get the mapper for this type
            var mapper = Mappers.GetMapper(type, defaultMapper);

            // Get the table info
            TableInfo = mapper.GetTableInfo(type);

            // Work out bound properties
            Columns = new Dictionary<string, PocoColumn>(StringComparer.OrdinalIgnoreCase);
            foreach (var pi in type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                ColumnInfo ci = mapper.GetColumnInfo(pi);
                if (ci == null)
                    continue;

                var pc = new PocoColumn();
                pc.PropertyInfo = pi;
                pc.ColumnName = ci.ColumnName;
                pc.ResultColumn = ci.ResultColumn;
                pc.ForceToUtc = ci.ForceToUtc;
                pc.InsertTemplate = ci.InsertTemplate;
                pc.UpdateTemplate = ci.UpdateTemplate;

                // Store it
                Columns.Add(pc.ColumnName, pc);
            }

            // Build column list for automatic select
            QueryColumns = (from c in Columns where !c.Value.ResultColumn select c.Key).ToArray();
        }

        public static PocoData ForObject(object obj, string primaryKeyName, IMapper defaultMapper)
        {
            var t = obj.GetType();
            if (t == typeof(System.Dynamic.ExpandoObject))
            {
                var pd = new PocoData();
                pd.TableInfo = new TableInfo();
                pd.Columns = new Dictionary<string, PocoColumn>(StringComparer.OrdinalIgnoreCase);
                pd.Columns.Add(primaryKeyName, new ExpandoColumn() { ColumnName = primaryKeyName });
                pd.TableInfo.PrimaryKey = primaryKeyName;
                pd.TableInfo.AutoIncrement = true;
                foreach (var col in (obj as IDictionary<string, object>).Keys)
                {
                    if (col != primaryKeyName)
                        pd.Columns.Add(col, new ExpandoColumn() { ColumnName = col });
                }
                return pd;
            }
            return ForType(t, defaultMapper);
        }

        public static PocoData ForType(Type type, IMapper defaultMapper)
        {
            if (type == typeof(System.Dynamic.ExpandoObject))
                throw new InvalidOperationException("Can't use dynamic types with this method");

            return _pocoDatas.Get(type, () => new PocoData(type, defaultMapper));
        }

        private static bool IsIntegralType(Type type)
        {
            var tc = Type.GetTypeCode(type);
            return tc >= TypeCode.SByte && tc <= TypeCode.UInt64;
        }

        // Create factory function that can convert a IDataReader record into a POCO
        public Delegate GetFactory(string sql, string connectionString, int firstColumn, int countColumns, IDataReader reader, IMapper defaultMapper)
        {
            // Check cache
            var key = Tuple.Create<string, string, int, int>(sql, connectionString, firstColumn, countColumns);

            return PocoFactories.Get(key, () =>
            {
                // Create the method
                var m = new DynamicMethod("petapoco_factory_" + PocoFactories.Count.ToString(), Type, new Type[] { typeof(IDataReader) }, true);
                var il = m.GetILGenerator();
                var mapper = Mappers.GetMapper(Type, defaultMapper);

                if (Type == typeof(object))
                {
                    // var poco=new T()
                    il.Emit(OpCodes.Newobj, typeof(System.Dynamic.ExpandoObject).GetConstructor(Type.EmptyTypes));          // obj

                    MethodInfo fnAdd = typeof(IDictionary<string, object>).GetMethod("Add");

                    // Enumerate all fields generating a set assignment for the column
                    for (int i = firstColumn; i < firstColumn + countColumns; i++)
                    {
                        var srcType = reader.GetFieldType(i);

                        il.Emit(OpCodes.Dup); // obj, obj
                        il.Emit(OpCodes.Ldstr, reader.GetName(i)); // obj, obj, fieldname

                        // Get the converter
                        Func<object, object> converter = mapper.GetFromDbConverter((PropertyInfo)null, srcType);

                        /*
						if (ForceDateTimesToUtc && converter == null && srcType == typeof(DateTime))
							converter = delegate(object src) { return new DateTime(((DateTime)src).Ticks, DateTimeKind.Utc); };
						 */

                        // Setup stack for call to converter
                        AddConverterToStack(il, converter);

                        // r[i]
                        il.Emit(OpCodes.Ldarg_0); // obj, obj, fieldname, converter?,    rdr
                        il.Emit(OpCodes.Ldc_I4, i); // obj, obj, fieldname, converter?,  rdr,i
                        il.Emit(OpCodes.Callvirt, fnGetValue); // obj, obj, fieldname, converter?,  value

                        // Convert DBNull to null
                        il.Emit(OpCodes.Dup); // obj, obj, fieldname, converter?,  value, value
                        il.Emit(OpCodes.Isinst, typeof(DBNull)); // obj, obj, fieldname, converter?,  value, (value or null)
                        var lblNotNull = il.DefineLabel();
                        il.Emit(OpCodes.Brfalse_S, lblNotNull); // obj, obj, fieldname, converter?,  value
                        il.Emit(OpCodes.Pop); // obj, obj, fieldname, converter?
                        if (converter != null)
                            il.Emit(OpCodes.Pop); // obj, obj, fieldname, 
                        il.Emit(OpCodes.Ldnull); // obj, obj, fieldname, null
                        if (converter != null)
                        {
                            var lblReady = il.DefineLabel();
                            il.Emit(OpCodes.Br_S, lblReady);
                            il.MarkLabel(lblNotNull);
                            il.Emit(OpCodes.Callvirt, fnInvoke);
                            il.MarkLabel(lblReady);
                        }
                        else
                        {
                            il.MarkLabel(lblNotNull);
                        }

                        il.Emit(OpCodes.Callvirt, fnAdd);
                    }
                }
                else if (Type.IsValueType || Type == typeof(string) || Type == typeof(byte[]))
                {
                    // Do we need to install a converter?
                    var srcType = reader.GetFieldType(0);
                    var converter = GetConverter(mapper, null, srcType, Type);

                    // "if (!rdr.IsDBNull(i))"
                    il.Emit(OpCodes.Ldarg_0); // rdr
                    il.Emit(OpCodes.Ldc_I4_0); // rdr,0
                    il.Emit(OpCodes.Callvirt, fnIsDBNull); // bool
                    var lblCont = il.DefineLabel();
                    il.Emit(OpCodes.Brfalse_S, lblCont);
                    il.Emit(OpCodes.Ldnull); // null
                    var lblFin = il.DefineLabel();
                    il.Emit(OpCodes.Br_S, lblFin);

                    il.MarkLabel(lblCont);

                    // Setup stack for call to converter
                    AddConverterToStack(il, converter);

                    il.Emit(OpCodes.Ldarg_0); // rdr
                    il.Emit(OpCodes.Ldc_I4_0); // rdr,0
                    il.Emit(OpCodes.Callvirt, fnGetValue); // value

                    // Call the converter
                    if (converter != null)
                        il.Emit(OpCodes.Callvirt, fnInvoke);

                    il.MarkLabel(lblFin);
                    il.Emit(OpCodes.Unbox_Any, Type); // value converted
                }
                else
                {
                    // var poco=new T()
                    var ctor = Type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[0], null);
                    if (ctor == null)
                        throw new InvalidOperationException("Type [" + Type.FullName + "] should have default public or non-public constructor");

                    il.Emit(OpCodes.Newobj, ctor);

                    // Enumerate all fields generating a set assignment for the column
                    for (int i = firstColumn; i < firstColumn + countColumns; i++)
                    {
                        // Get the PocoColumn for this db column, ignore if not known
                        PocoColumn pc;
                        if (!Columns.TryGetValue(reader.GetName(i), out pc))
                            continue;

                        // Get the source type for this column
                        var srcType = reader.GetFieldType(i);
                        var dstType = pc.PropertyInfo.PropertyType;

                        // "if (!rdr.IsDBNull(i))"
                        il.Emit(OpCodes.Ldarg_0); // poco,rdr
                        il.Emit(OpCodes.Ldc_I4, i); // poco,rdr,i
                        il.Emit(OpCodes.Callvirt, fnIsDBNull); // poco,bool
                        var lblNext = il.DefineLabel();
                        il.Emit(OpCodes.Brtrue_S, lblNext); // poco

                        il.Emit(OpCodes.Dup); // poco,poco

                        // Do we need to install a converter?
                        var converter = GetConverter(mapper, pc, srcType, dstType);

                        // Fast
                        bool Handled = false;
                        if (converter == null)
                        {
                            var valuegetter = typeof(IDataRecord).GetMethod("Get" + srcType.Name, new Type[] { typeof(int) });
                            if (valuegetter != null
                                && valuegetter.ReturnType == srcType
                                && (valuegetter.ReturnType == dstType || valuegetter.ReturnType == Nullable.GetUnderlyingType(dstType)))
                            {
                                il.Emit(OpCodes.Ldarg_0); // *,rdr
                                il.Emit(OpCodes.Ldc_I4, i); // *,rdr,i
                                il.Emit(OpCodes.Callvirt, valuegetter); // *,value

                                // Convert to Nullable
                                if (Nullable.GetUnderlyingType(dstType) != null)
                                {
                                    il.Emit(OpCodes.Newobj, dstType.GetConstructor(new Type[] { Nullable.GetUnderlyingType(dstType) }));
                                }

                                il.Emit(OpCodes.Callvirt, pc.PropertyInfo.GetSetMethod(true)); // poco
                                Handled = true;
                            }
                        }

                        // Not so fast
                        if (!Handled)
                        {
                            // Setup stack for call to converter
                            AddConverterToStack(il, converter);

                            // "value = rdr.GetValue(i)"
                            il.Emit(OpCodes.Ldarg_0); // *,rdr
                            il.Emit(OpCodes.Ldc_I4, i); // *,rdr,i
                            il.Emit(OpCodes.Callvirt, fnGetValue); // *,value

                            // Call the converter
                            if (converter != null)
                                il.Emit(OpCodes.Callvirt, fnInvoke);

                            // Assign it
                            il.Emit(OpCodes.Unbox_Any, pc.PropertyInfo.PropertyType); // poco,poco,value
                            il.Emit(OpCodes.Callvirt, pc.PropertyInfo.GetSetMethod(true)); // poco
                        }

                        il.MarkLabel(lblNext);
                    }

                    var fnOnLoaded = RecurseInheritedTypes<MethodInfo>(Type,
                        (x) => x.GetMethod("OnLoaded", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[0], null));
                    if (fnOnLoaded != null)
                    {
                        il.Emit(OpCodes.Dup);
                        il.Emit(OpCodes.Callvirt, fnOnLoaded);
                    }
                }

                il.Emit(OpCodes.Ret);

                // Cache it, return it
                return m.CreateDelegate(Expression.GetFuncType(typeof(IDataReader), Type));
            }
                );
        }

        private static void AddConverterToStack(ILGenerator il, Func<object, object> converter)
        {
            if (converter != null)
            {
                // Add the converter
                int converterIndex;

                lock (_converterLock)
                {
                    converterIndex = _converters.Count;
                    _converters.Add(converter);
                }

                // Generate IL to push the converter onto the stack
                il.Emit(OpCodes.Ldsfld, fldConverters);
                il.Emit(OpCodes.Ldc_I4, converterIndex);
                il.Emit(OpCodes.Callvirt, fnListGetItem); // Converter
            }
        }

        private static Func<object, object> GetConverter(IMapper mapper, PocoColumn pc, Type srcType, Type dstType)
        {
            Func<object, object> converter = null;

            // Get converter from the mapper
            if (pc != null)
            {
                converter = mapper.GetFromDbConverter(pc.PropertyInfo, srcType);
                if (converter != null)
                    return converter;
            }

            // Standard DateTime->Utc mapper
            if (pc != null && pc.ForceToUtc && srcType == typeof(DateTime) && (dstType == typeof(DateTime) || dstType == typeof(DateTime?)))
            {
                return delegate (object src) { return new DateTime(((DateTime)src).Ticks, DateTimeKind.Utc); };
            }

            // unwrap nullable types
            Type underlyingDstType = Nullable.GetUnderlyingType(dstType);
            if (underlyingDstType != null)
            {
                dstType = underlyingDstType;
            }

            // Forced type conversion including integral types -> enum
            if (dstType.IsEnum && IsIntegralType(srcType))
            {
                var backingDstType = Enum.GetUnderlyingType(dstType);
                if (underlyingDstType != null)
                {
                    // if dstType is Nullable<Enum>, convert to enum value
                    return delegate (object src) { return Enum.ToObject(dstType, src); };
                }
                else if (srcType != backingDstType)
                {
                    return delegate (object src) { return Convert.ChangeType(src, backingDstType, null); };
                }
            }
            else if (!dstType.IsAssignableFrom(srcType))
            {
                if (dstType.IsEnum && srcType == typeof(string))
                {
                    return delegate (object src) { return EnumMapper.EnumFromString(dstType, (string)src); };
                }
                else if (dstType == typeof(Guid) && srcType == typeof(string))
                {
                    return delegate (object src) { return Guid.Parse((string)src); };
                }
                else
                {
                    return delegate (object src) { return Convert.ChangeType(src, dstType, null); };
                }
            }

            return null;
        }

        private static T RecurseInheritedTypes<T>(Type t, Func<Type, T> cb)
        {
            while (t != null)
            {
                T info = cb(t);
                if (info != null)
                    return info;
                t = t.BaseType;
            }
            return default(T);
        }

        internal static void FlushCaches()
        {
            _pocoDatas.Flush();
        }

        public string GetColumnName(string propertyName)
        {
            return Columns.Values.First(c => c.PropertyInfo.Name.Equals(propertyName)).ColumnName;
        }
    }


    /// <summary>
    ///     A simple helper class for build SQL statements
    /// </summary>
    public class Sql
    {
        private object[] _args;
        private object[] _argsFinal;
        private Sql _rhs;

        private string _sql;
        private string _sqlFinal;

        /// <summary>
        ///     Instantiate a new SQL Builder object.  Weirdly implemented as a property but makes
        ///     for more elegantly readable fluent style construction of SQL Statements
        ///     eg: db.Query(Sql.Builder.Append(....))
        /// </summary>
        public static Sql Builder
        {
            get { return new Sql(); }
        }

        /// <summary>
        ///     Returns the final SQL statement represented by this builder
        /// </summary>
        public string SQL
        {
            get
            {
                Build();
                return _sqlFinal;
            }
        }

        /// <summary>
        ///     Gets the complete, final set of arguments collected by this builder.
        /// </summary>
        public object[] Arguments
        {
            get
            {
                Build();
                return _argsFinal;
            }
        }

        /// <summary>
        ///     Default, empty constructor
        /// </summary>
        public Sql()
        {
        }

        /// <summary>
        ///     Construct an SQL statement with the supplied SQL and arguments
        /// </summary>
        /// <param name="sql">The SQL statement or fragment</param>
        /// <param name="args">Arguments to any parameters embedded in the SQL</param>
        public Sql(string sql, params object[] args)
        {
            _sql = sql;
            _args = args;
        }

        private void Build()
        {
            // already built?
            if (_sqlFinal != null)
                return;

            // Build it
            var sb = new StringBuilder();
            var args = new List<object>();
            Build(sb, args, null);
            _sqlFinal = sb.ToString();
            _argsFinal = args.ToArray();
        }

        /// <summary>
        ///     Append another SQL builder instance to the right-hand-side of this SQL builder
        /// </summary>
        /// <param name="sql">A reference to another SQL builder instance</param>
        /// <returns>A reference to this builder, allowing for fluent style concatenation</returns>
        public Sql Append(Sql sql)
        {
            if (_rhs != null)
                _rhs.Append(sql);
            else
                _rhs = sql;

            _sqlFinal = null;
            return this;
        }

        /// <summary>
        ///     Append an SQL fragment to the right-hand-side of this SQL builder
        /// </summary>
        /// <param name="sql">The SQL statement or fragment</param>
        /// <param name="args">Arguments to any parameters embedded in the SQL</param>
        /// <returns>A reference to this builder, allowing for fluent style concatenation</returns>
        public Sql Append(string sql, params object[] args)
        {
            return Append(new Sql(sql, args));
        }

        private static bool Is(Sql sql, string sqltype)
        {
            return sql != null && sql._sql != null && sql._sql.StartsWith(sqltype, StringComparison.InvariantCultureIgnoreCase);
        }

        private void Build(StringBuilder sb, List<object> args, Sql lhs)
        {
            if (!string.IsNullOrEmpty(_sql))
            {
                // Add SQL to the string
                if (sb.Length > 0)
                {
                    sb.Append("\n");
                }

                var sql = ParametersHelper.ProcessParams(_sql, _args, args);

                if (Is(lhs, "WHERE ") && Is(this, "WHERE "))
                    sql = "AND " + sql.Substring(6);
                if (Is(lhs, "ORDER BY ") && Is(this, "ORDER BY "))
                    sql = ", " + sql.Substring(9);
                // add set clause
                if (Is(lhs, "SET ") && Is(this, "SET "))
                    sql = ", " + sql.Substring(4);

                sb.Append(sql);
            }

            // Now do rhs
            if (_rhs != null)
                _rhs.Build(sb, args, this);
        }

        /// <summary>
        ///     Appends an SQL SET clause to this SQL builder
        /// </summary>
        /// <param name="sql">The SET clause like "{field} = {value}"</param>
        /// <param name="args">Arguments to any parameters embedded in the supplied SQL</param>
        /// <returns>A reference to this builder, allowing for fluent style concatenation</returns>
        public Sql Set(string sql, params object[] args)
        {
            return Append(new Sql("SET " + sql, args));
        }

        /// <summary>
        ///     Appends an SQL WHERE clause to this SQL builder
        /// </summary>
        /// <param name="sql">The condition of the WHERE clause</param>
        /// <param name="args">Arguments to any parameters embedded in the supplied SQL</param>
        /// <returns>A reference to this builder, allowing for fluent style concatenation</returns>
        public Sql Where(string sql, params object[] args)
        {
            return Append(new Sql("WHERE (" + sql + ")", args));
        }

        /// <summary>
        ///     Appends an SQL ORDER BY clause to this SQL builder
        /// </summary>
        /// <param name="columns">A collection of SQL column names to order by</param>
        /// <returns>A reference to this builder, allowing for fluent style concatenation</returns>
        public Sql OrderBy(params object[] columns)
        {
            return Append(new Sql("ORDER BY " + string.Join(", ", (from x in columns select x.ToString()).ToArray())));
        }

        /// <summary>
        ///     Appends an SQL SELECT clause to this SQL builder
        /// </summary>
        /// <param name="columns">A collection of SQL column names to select</param>
        /// <returns>A reference to this builder, allowing for fluent style concatenation</returns>
        public Sql Select(params object[] columns)
        {
            return Append(new Sql("SELECT " + string.Join(", ", (from x in columns select x.ToString()).ToArray())));
        }

        /// <summary>
        ///     Appends an SQL FROM clause to this SQL builder
        /// </summary>
        /// <param name="tables">A collection of table names to be used in the FROM clause</param>
        /// <returns>A reference to this builder, allowing for fluent style concatenation</returns>
        public Sql From(params object[] tables)
        {
            return Append(new Sql("FROM " + string.Join(", ", (from x in tables select x.ToString()).ToArray())));
        }

        /// <summary>
        ///     Appends an SQL GROUP BY clause to this SQL builder
        /// </summary>
        /// <param name="columns">A collection of column names to be grouped by</param>
        /// <returns>A reference to this builder, allowing for fluent style concatenation</returns>
        public Sql GroupBy(params object[] columns)
        {
            return Append(new Sql("GROUP BY " + string.Join(", ", (from x in columns select x.ToString()).ToArray())));
        }

        private SqlJoinClause Join(string joinType, string table)
        {
            return new SqlJoinClause(Append(new Sql(joinType + table)));
        }

        /// <summary>
        ///     Appends an SQL INNER JOIN clause to this SQL builder
        /// </summary>
        /// <param name="table">The name of the table to join</param>
        /// <returns>A reference an SqlJoinClause through which the join condition can be specified</returns>
        public SqlJoinClause InnerJoin(string table)
        {
            return Join("INNER JOIN ", table);
        }

        /// <summary>
        ///     Appends an SQL LEFT JOIN clause to this SQL builder
        /// </summary>
        /// <param name="table">The name of the table to join</param>
        /// <returns>A reference an SqlJoinClause through which the join condition can be specified</returns>
        public SqlJoinClause LeftJoin(string table)
        {
            return Join("LEFT JOIN ", table);
        }

        /// <summary>
        ///     Returns the SQL statement.
        /// </summary>
        /// <summary>
        ///     Returns the final SQL statement represented by this builder
        /// </summary>
        public override string ToString()
        {
            return SQL;
        }

        /// <summary>
        ///     The SqlJoinClause is a simple helper class used in the construction of SQL JOIN statements with the SQL builder
        /// </summary>
        public class SqlJoinClause
        {
            private readonly Sql _sql;

            public SqlJoinClause(Sql sql)
            {
                _sql = sql;
            }

            /// <summary>
            ///     Appends a SQL ON clause after a JOIN statement
            /// </summary>
            /// <param name="onClause">The ON clause to be appended</param>
            /// <param name="args">Arguments to any parameters embedded in the supplied SQL</param>
            /// <returns>A reference to the parent SQL builder, allowing for fluent style concatenation</returns>
            public Sql On(string onClause, params object[] args)
            {
                return _sql.Append("ON " + onClause, args);
            }
        }
    }


    /// <summary>
    ///     StandardMapper is the default implementation of IMapper used by PetaPoco
    /// </summary>
    public class StandardMapper : IMapper
    {
        /// <summary>
        ///     Get information about the table associated with a POCO class
        /// </summary>
        /// <param name="pocoType">The poco type.</param>
        /// <returns>A TableInfo instance</returns>
        /// <remarks>
        ///     This method must return a valid TableInfo.
        ///     To create a TableInfo from a POCO's attributes, use TableInfo.FromPoco
        /// </remarks>
        public virtual TableInfo GetTableInfo(Type pocoType)
        {
            return TableInfo.FromPoco(pocoType);
        }

        /// <summary>
        ///     Get information about the column associated with a property of a POCO
        /// </summary>
        /// <param name="pocoProperty">The PropertyInfo of the property being queried</param>
        /// <returns>A reference to a ColumnInfo instance, or null to ignore this property</returns>
        /// <remarks>
        ///     To create a ColumnInfo from a property's attributes, use PropertyInfo.FromProperty
        /// </remarks>
        public virtual ColumnInfo GetColumnInfo(PropertyInfo pocoProperty)
        {
            return ColumnInfo.FromProperty(pocoProperty);
        }

        /// <summary>
        ///     Supply a function to convert a database value to the correct property value
        /// </summary>
        /// <param name="targetProperty">The target property</param>
        /// <param name="sourceType">The type of data returned by the DB</param>
        /// <returns>A Func that can do the conversion, or null for no conversion</returns>
        public virtual Func<object, object> GetFromDbConverter(PropertyInfo targetProperty, Type sourceType)
        {
            return null;
        }

        /// <summary>
        ///     Supply a function to convert a property value into a database value
        /// </summary>
        /// <param name="sourceProperty">The property to be converted</param>
        /// <returns>A Func that can do the conversion</returns>
        /// <remarks>
        ///     This conversion is only used for converting values from POCO's that are
        ///     being Inserted or Updated.
        ///     Conversion is not available for parameter values passed directly to queries.
        /// </remarks>
        public virtual Func<object, object> GetToDbConverter(PropertyInfo sourceProperty)
        {
            return null;
        }
    }


    /// <summary>
    /// 表信息    Use by IMapper to override table bindings for an object
    /// </summary>
    public class TableInfo
    {
        /// <summary>
        ///     The database table name
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        ///     The name of the primary key column of the table
        /// </summary>
        public string PrimaryKey { get; set; }

        /// <summary>
        ///     True if the primary key column is an auto-incrementing
        /// </summary>
        public bool AutoIncrement { get; set; }

        /// <summary>
        ///     The name of the sequence used for auto-incrementing Oracle primary key fields
        /// </summary>
        public string SequenceName { get; set; }

        /// <summary>
        ///     Creates and populates a TableInfo from the attributes of a POCO
        /// </summary>
        /// <param name="t">The POCO type</param>
        /// <returns>A TableInfo instance</returns>
        public static TableInfo FromPoco(Type t)
        {
            TableInfo ti = new TableInfo();

            // 获取表名
            var a = t.GetCustomAttributes(typeof(TableNameAttribute), true);
            ti.TableName = a.Length == 0 ? t.Name : (a[0] as TableNameAttribute).Value;

            // 获取主键
            a = t.GetCustomAttributes(typeof(PrimaryKeyAttribute), true);
            ti.PrimaryKey = a.Length == 0 ? null : (a[0] as PrimaryKeyAttribute).Value;
            ti.SequenceName = a.Length == 0 ? null : (a[0] as PrimaryKeyAttribute).SequenceName;
            ti.AutoIncrement = a.Length == 0 ? false : (a[0] as PrimaryKeyAttribute).AutoIncrement;

            if (string.IsNullOrEmpty(ti.PrimaryKey))
            {
                var prop = t.GetProperties().FirstOrDefault(p =>
                {
                    if (p.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
                        return true;
                    if (p.Name.Equals(t.Name + "id", StringComparison.OrdinalIgnoreCase))
                        return true;
                    if (p.Name.Equals(t.Name + "_id", StringComparison.OrdinalIgnoreCase))
                        return true;
                    return false;
                });

                if (prop != null)
                {
                    ti.PrimaryKey = prop.Name;
                    ti.AutoIncrement = prop.PropertyType.IsValueType;
                }
            }

            return ti;
        }
    }


    /// <summary>
    /// 事务处理    Transaction object helps maintain transaction depth counts
    /// </summary>
    public class Transaction : ITransaction
    {
        private Database _db;

        public Transaction(Database db)
        {
            _db = db;
            _db.BeginTransaction();
        }

        public void Complete()
        {
            _db.CompleteTransaction();
            _db = null;
        }

        public void Dispose()
        {
            if (_db != null)
                _db.AbortTransaction();
        }
    }


    /// <summary>
    ///     Author: Originally written (I believe) by Andrew Peters
    ///     Source: Scott Kirkland (https://github.com/srkirkland/Inflector)
    /// </summary>
    public class EnglishInflector : IInflector
    {
        private static readonly List<Rule> Plurals = new List<Rule>();

        private static readonly List<Rule> Singulars = new List<Rule>();

        private static readonly List<string> Uncountables = new List<string>();

        static EnglishInflector()
        {
            AddPlural("$", "s");
            AddPlural("s$", "s");
            AddPlural("(ax|test)is$", "$1es");
            AddPlural("(octop|vir|alumn|fung)us$", "$1i");
            AddPlural("(alias|status)$", "$1es");
            AddPlural("(bu)s$", "$1ses");
            AddPlural("(buffal|tomat|volcan)o$", "$1oes");
            AddPlural("([ti])um$", "$1a");
            AddPlural("sis$", "ses");
            AddPlural("(?:([^f])fe|([lr])f)$", "$1$2ves");
            AddPlural("(hive)$", "$1s");
            AddPlural("([^aeiouy]|qu)y$", "$1ies");
            AddPlural("(x|ch|ss|sh)$", "$1es");
            AddPlural("(matr|vert|ind)ix|ex$", "$1ices");
            AddPlural("([m|l])ouse$", "$1ice");
            AddPlural("^(ox)$", "$1en");
            AddPlural("(quiz)$", "$1zes");

            AddSingular("s$", "");
            AddSingular("(n)ews$", "$1ews");
            AddSingular("([ti])a$", "$1um");
            AddSingular("((a)naly|(b)a|(d)iagno|(p)arenthe|(p)rogno|(s)ynop|(t)he)ses$", "$1$2sis");
            AddSingular("(^analy)ses$", "$1sis");
            AddSingular("([^f])ves$", "$1fe");
            AddSingular("(hive)s$", "$1");
            AddSingular("(tive)s$", "$1");
            AddSingular("([lr])ves$", "$1f");
            AddSingular("([^aeiouy]|qu)ies$", "$1y");
            AddSingular("(s)eries$", "$1eries");
            AddSingular("(m)ovies$", "$1ovie");
            AddSingular("(x|ch|ss|sh)es$", "$1");
            AddSingular("([m|l])ice$", "$1ouse");
            AddSingular("(bus)es$", "$1");
            AddSingular("(o)es$", "$1");
            AddSingular("(shoe)s$", "$1");
            AddSingular("(cris|ax|test)es$", "$1is");
            AddSingular("(octop|vir|alumn|fung)i$", "$1us");
            AddSingular("(alias|status)es$", "$1");
            AddSingular("^(ox)en", "$1");
            AddSingular("(vert|ind)ices$", "$1ex");
            AddSingular("(matr)ices$", "$1ix");
            AddSingular("(quiz)zes$", "$1");

            AddIrregular("person", "people");
            AddIrregular("man", "men");
            AddIrregular("child", "children");
            AddIrregular("sex", "sexes");
            AddIrregular("move", "moves");
            AddIrregular("goose", "geese");
            AddIrregular("alumna", "alumnae");

            AddUncountable("equipment");
            AddUncountable("information");
            AddUncountable("rice");
            AddUncountable("money");
            AddUncountable("species");
            AddUncountable("series");
            AddUncountable("fish");
            AddUncountable("sheep");
            AddUncountable("deer");
            AddUncountable("aircraft");
        }

        /// <summary>
        ///     Pluralises a word.
        /// </summary>
        /// <example>
        ///     inflect.Pluralise("search").ShouldBe("searches");
        ///     inflect.Pluralise("stack").ShouldBe("stacks");
        ///     inflect.Pluralise("fish").ShouldBe("fish");
        /// </example>
        /// <param name="word">The word to pluralise.</param>
        /// <returns>The pluralised word.</returns>
        public string Pluralise(string word)
        {
            return ApplyRules(Plurals, word);
        }

        /// <summary>
        ///     Singularises a word.
        /// </summary>
        /// <example>
        ///     inflect.Singularise("searches").ShouldBe("search");
        ///     inflect.Singularise("stacks").ShouldBe("stack");
        ///     inflect.Singularise("fish").ShouldBe("fish");
        /// </example>
        /// <param name="word">The word to signularise.</param>
        /// <returns>The signularised word.</returns>
        public string Singularise(string word)
        {
            return ApplyRules(Singulars, word);
        }

        /// <summary>
        ///     Titleises the word. (title => Title, the_brown_fox => TheBrownFox)
        /// </summary>
        /// <example>
        ///     inflect.Titleise("some title").ShouldBe("Some Title");
        ///     inflect.Titleise("some-title").ShouldBe("Some Title");
        ///     inflect.Titleise("sometitle").ShouldBe("Sometitle");
        ///     inflect.Titleise("some_title:_the_beginning").ShouldBe("Some Title: The Beginning");
        /// </example>
        /// <param name="word">The word to titleise.</param>
        /// <returns>The titleised word.</returns>
        public string Titleise(string word)
        {
            return Regex.Replace(Humanise(Underscore(word)), @"\b([a-z])",
                match => match.Captures[0].Value.ToUpper());
        }

        /// <summary>
        ///     Humanizes the word.
        /// </summary>
        /// <example>
        ///     inflect.Humanise("some_title").ShouldBe("Some title");
        ///     inflect.Humanise("some-title").ShouldBe("Some-title");
        ///     inflect.Humanise("Some_title").ShouldBe("Some title");
        ///     inflect.Humanise("someTitle").ShouldBe("Sometitle");
        ///     inflect.Humanise("someTitle_Another").ShouldBe("Sometitle another");
        /// </example>
        /// <param name="lowercaseAndUnderscoredWord">The word to humanise.</param>
        /// <returns>The humanized word.</returns>
        public string Humanise(string lowercaseAndUnderscoredWord)
        {
            return Capitalise(Regex.Replace(lowercaseAndUnderscoredWord, @"_", " "));
        }

        /// <summary>
        ///     Pascalises the word.
        /// </summary>
        /// <example>
        ///     inflect.Pascalise("customer").ShouldBe("Customer");
        ///     inflect.Pascalise("customer_name").ShouldBe("CustomerName");
        ///     inflect.Pascalise("customer name").ShouldBe("Customer name");
        /// </example>
        /// <param name="lowercaseAndUnderscoredWord">The word to pascalise.</param>
        /// <returns>The pascalied word.</returns>
        public string Pascalise(string lowercaseAndUnderscoredWord)
        {
            return Regex.Replace(lowercaseAndUnderscoredWord, "(?:^|_)(.)",
                match => match.Groups[1].Value.ToUpper());
        }

        /// <summary>
        ///     Camelises the word.
        /// </summary>
        /// <example>
        ///     inflect.Camelise("Customer").ShouldBe("customer");
        ///     inflect.Camelise("customer_name").ShouldBe("customerName");
        ///     inflect.Camelise("customer_first_name").ShouldBe("customerFirstName");
        ///     inflect.Camelise("customer name").ShouldBe("customer name");
        /// </example>
        /// <param name="lowercaseAndUnderscoredWord">The word to camelise.</param>
        /// <returns>The camelised word.</returns>
        public string Camelise(string lowercaseAndUnderscoredWord)
        {
            return Uncapitalise(Pascalise(lowercaseAndUnderscoredWord));
        }

        /// <summary>
        ///     Underscores the word.
        /// </summary>
        /// <example>
        ///     inflect.Underscore("SomeTitle").ShouldBe("some_title");
        ///     inflect.Underscore("someTitle").ShouldBe("some_title");
        ///     inflect.Underscore("some title that will be underscored").ShouldBe("some_title_that_will_be_underscored");
        ///     inflect.Underscore("SomeTitleThatWillBeUnderscored").ShouldBe("some_title_that_will_be_underscored");
        /// </example>
        /// <param name="pascalCasedWord">The word to underscore.</param>
        /// <returns>The underscored word.</returns>
        public string Underscore(string pascalCasedWord)
        {
            return Regex.Replace(
                Regex.Replace(
                    Regex.Replace(pascalCasedWord, @"([A-Z]+)([A-Z][a-z])", "$1_$2"), @"([a-z\d])([A-Z])",
                    "$1_$2"), @"[-\s]", "_").ToLower();
        }

        /// <summary>
        ///     Capitalises the word.
        /// </summary>
        /// <example>
        ///     inflect.Capitalise("some title").ShouldBe("Some title");
        ///     inflect.Capitalise("some Title").ShouldBe("Some title");
        ///     inflect.Capitalise("SOMETITLE").ShouldBe("Sometitle");
        ///     inflect.Capitalise("someTitle").ShouldBe("Sometitle");
        ///     inflect.Capitalise("some title goes here").ShouldBe("Some title goes here");
        /// </example>
        /// <param name="word">The word to capitalise.</param>
        /// <returns>The capitalised word.</returns>
        public string Capitalise(string word)
        {
            return word.Substring(0, 1).ToUpper() + word.Substring(1).ToLower();
        }

        /// <summary>
        ///     Uncapitalises the word.
        /// </summary>
        /// <example>
        ///     inflect.Uncapitalise("Some title").ShouldBe("some title");
        ///     inflect.Uncapitalise("Some Title").ShouldBe("some Title");
        ///     inflect.Uncapitalise("SOMETITLE").ShouldBe("sOMETITLE");
        ///     inflect.Uncapitalise("someTitle").ShouldBe("someTitle");
        ///     inflect.Uncapitalise("Some title goes here").ShouldBe("some title goes here");
        /// </example>
        /// <param name="word">The word to uncapitalise.</param>
        /// <returns>The uncapitalised word.</returns>
        public string Uncapitalise(string word)
        {
            return word.Substring(0, 1).ToLower() + word.Substring(1);
        }

        /// <summary>
        ///     Ordinalises the number.
        /// </summary>
        /// <example>
        ///     inflect.Ordinalise(0).ShouldBe("0th");
        ///     inflect.Ordinalise(1).ShouldBe("1st");
        ///     inflect.Ordinalise(2).ShouldBe("2nd");
        ///     inflect.Ordinalise(3).ShouldBe("3rd");
        ///     inflect.Ordinalise(101).ShouldBe("101st");
        ///     inflect.Ordinalise(104).ShouldBe("104th");
        ///     inflect.Ordinalise(1000).ShouldBe("1000th");
        ///     inflect.Ordinalise(1001).ShouldBe("1001st");
        /// </example>
        /// <param name="number">The number to ordinalise.</param>
        /// <returns>The ordinalised number.</returns>
        public string Ordinalise(string number)
        {
            return Ordanise(int.Parse(number), number);
        }

        /// <summary>
        ///     Ordinalises the number.
        /// </summary>
        /// <example>
        ///     inflect.Ordinalise("0").ShouldBe("0th");
        ///     inflect.Ordinalise("1").ShouldBe("1st");
        ///     inflect.Ordinalise("2").ShouldBe("2nd");
        ///     inflect.Ordinalise("3").ShouldBe("3rd");
        ///     inflect.Ordinalise("100").ShouldBe("100th");
        ///     inflect.Ordinalise("101").ShouldBe("101st");
        ///     inflect.Ordinalise("1000").ShouldBe("1000th");
        ///     inflect.Ordinalise("1001").ShouldBe("1001st");
        /// </example>
        /// <param name="number">The number to ordinalise.</param>
        /// <returns>The ordinalised number.</returns>
        public string Ordinalise(int number)
        {
            return Ordanise(number, number.ToString());
        }

        /// <summary>
        ///     Dasherises the word.
        /// </summary>
        /// <example>
        ///     inflect.Dasherise("some_title").ShouldBe("some-title");
        ///     inflect.Dasherise("some-title").ShouldBe("some-title");
        ///     inflect.Dasherise("some_title_goes_here").ShouldBe("some-title-goes-here");
        ///     inflect.Dasherise("some_title and_another").ShouldBe("some-title and-another");
        /// </example>
        /// <param name="underscoredWord">The word to dasherise.</param>
        /// <returns>The dasherised word.</returns>
        public string Dasherise(string underscoredWord)
        {
            return underscoredWord.Replace('_', '-');
        }

        private static void AddIrregular(string singular, string plural)
        {
            AddPlural("(" + singular[0] + ")" + singular.Substring(1) + "$", "$1" + plural.Substring(1));
            AddSingular("(" + plural[0] + ")" + plural.Substring(1) + "$", "$1" + singular.Substring(1));
        }

        private static void AddUncountable(string word)
        {
            Uncountables.Add(word.ToLower());
        }

        private static void AddPlural(string rule, string replacement)
        {
            Plurals.Add(new Rule(rule, replacement));
        }

        private static void AddSingular(string rule, string replacement)
        {
            Singulars.Add(new Rule(rule, replacement));
        }

        private static string ApplyRules(IList<Rule> rules, string word)
        {
            var result = word;

            if (Uncountables.Contains(word.ToLower()))
                return result;

            for (var i = rules.Count - 1; i >= 0; i--)
            {
                if ((result = rules[i].Apply(word)) != null)
                {
                    break;
                }
            }

            return result;
        }

        private static string Ordanise(int number, string numberString)
        {
            var nMod100 = number % 100;

            if (nMod100 >= 11 && nMod100 <= 13)
            {
                return numberString + "th";
            }

            switch (number % 10)
            {
                case 1:
                    return numberString + "st";
                case 2:
                    return numberString + "nd";
                case 3:
                    return numberString + "rd";
                default:
                    return numberString + "th";
            }
        }

        private class Rule
        {
            private readonly Regex _regex;

            private readonly string _replacement;

            public Rule(string pattern, string replacement)
            {
                _regex = new Regex(pattern, RegexOptions.IgnoreCase);
                _replacement = replacement;
            }

            public string Apply(string word)
            {
                return !_regex.IsMatch(word) ? null : _regex.Replace(word, _replacement);
            }
        }
    }


    /// <summary>
    ///     Specifies the inflection contract.
    /// </summary>
    public interface IInflector
    {
        /// <summary>
        ///     Pluralises a word.
        /// </summary>
        /// <example>
        ///     inflect.Pluralise("search").ShouldBe("searches");
        ///     inflect.Pluralise("stack").ShouldBe("stacks");
        ///     inflect.Pluralise("fish").ShouldBe("fish");
        /// </example>
        /// <param name="word">The word to pluralise.</param>
        /// <returns>The pluralised word.</returns>
        string Pluralise(string word);

        /// <summary>
        ///     Singularises a word.
        /// </summary>
        /// <example>
        ///     inflect.Singularise("searches").ShouldBe("search");
        ///     inflect.Singularise("stacks").ShouldBe("stack");
        ///     inflect.Singularise("fish").ShouldBe("fish");
        /// </example>
        /// <param name="word">The word to signularise.</param>
        /// <returns>The signularised word.</returns>
        string Singularise(string word);

        /// <summary>
        ///     Titleises the word. (title => Title, the_brown_fox => TheBrownFox)
        /// </summary>
        /// <example>
        ///     inflect.Titleise("some title").ShouldBe("Some Title");
        ///     inflect.Titleise("some-title").ShouldBe("Some Title");
        ///     inflect.Titleise("sometitle").ShouldBe("Sometitle");
        ///     inflect.Titleise("some_title:_the_beginning").ShouldBe("Some Title: The Beginning");
        /// </example>
        /// <param name="word">The word to titleise.</param>
        /// <returns>The titleised word.</returns>
        string Titleise(string word);

        /// <summary>
        ///     Humanizes the word.
        /// </summary>
        /// <example>
        ///     inflect.Humanise("some_title").ShouldBe("Some title");
        ///     inflect.Humanise("some-title").ShouldBe("Some-title");
        ///     inflect.Humanise("Some_title").ShouldBe("Some title");
        ///     inflect.Humanise("someTitle").ShouldBe("Sometitle");
        ///     inflect.Humanise("someTitle_Another").ShouldBe("Sometitle another");
        /// </example>
        /// <param name="lowercaseAndUnderscoredWord">The word to humanise.</param>
        /// <returns>The humanized word.</returns>
        string Humanise(string lowercaseAndUnderscoredWord);

        /// <summary>
        ///     Pascalises the word.
        /// </summary>
        /// <example>
        ///     inflect.Pascalise("customer").ShouldBe("Customer");
        ///     inflect.Pascalise("customer_name").ShouldBe("CustomerName");
        ///     inflect.Pascalise("customer name").ShouldBe("Customer name");
        /// </example>
        /// <param name="lowercaseAndUnderscoredWord">The word to pascalise.</param>
        /// <returns>The pascalied word.</returns>
        string Pascalise(string lowercaseAndUnderscoredWord);

        /// <summary>
        ///     Camelises the word.
        /// </summary>
        /// <example>
        ///     inflect.Camelise("Customer").ShouldBe("customer");
        ///     inflect.Camelise("customer_name").ShouldBe("customerName");
        ///     inflect.Camelise("customer_first_name").ShouldBe("customerFirstName");
        ///     inflect.Camelise("customer name").ShouldBe("customer name");
        /// </example>
        /// <param name="lowercaseAndUnderscoredWord">The word to camelise.</param>
        /// <returns>The camelised word.</returns>
        string Camelise(string lowercaseAndUnderscoredWord);

        /// <summary>
        ///     Underscores the word.
        /// </summary>
        /// <example>
        ///     inflect.Underscore("SomeTitle").ShouldBe("some_title");
        ///     inflect.Underscore("someTitle").ShouldBe("some_title");
        ///     inflect.Underscore("some title that will be underscored").ShouldBe("some_title_that_will_be_underscored");
        ///     inflect.Underscore("SomeTitleThatWillBeUnderscored").ShouldBe("some_title_that_will_be_underscored");
        /// </example>
        /// <param name="pascalCasedWord">The word to underscore.</param>
        /// <returns>The underscored word.</returns>
        string Underscore(string pascalCasedWord);

        /// <summary>
        ///     Capitalises the word.
        /// </summary>
        /// <example>
        ///     inflect.Capitalise("some title").ShouldBe("Some title");
        ///     inflect.Capitalise("some Title").ShouldBe("Some title");
        ///     inflect.Capitalise("SOMETITLE").ShouldBe("Sometitle");
        ///     inflect.Capitalise("someTitle").ShouldBe("Sometitle");
        ///     inflect.Capitalise("some title goes here").ShouldBe("Some title goes here");
        /// </example>
        /// <param name="word">The word to capitalise.</param>
        /// <returns>The capitalised word.</returns>
        string Capitalise(string word);

        /// <summary>
        ///     Uncapitalises the word.
        /// </summary>
        /// <example>
        ///     inflect.Uncapitalise("Some title").ShouldBe("some title");
        ///     inflect.Uncapitalise("Some Title").ShouldBe("some Title");
        ///     inflect.Uncapitalise("SOMETITLE").ShouldBe("sOMETITLE");
        ///     inflect.Uncapitalise("someTitle").ShouldBe("someTitle");
        ///     inflect.Uncapitalise("Some title goes here").ShouldBe("some title goes here");
        /// </example>
        /// <param name="word">The word to uncapitalise.</param>
        /// <returns>The uncapitalised word.</returns>
        string Uncapitalise(string word);

        /// <summary>
        ///     Ordinalises the number.
        /// </summary>
        /// <example>
        ///     inflect.Ordinalise(0).ShouldBe("0th");
        ///     inflect.Ordinalise(1).ShouldBe("1st");
        ///     inflect.Ordinalise(2).ShouldBe("2nd");
        ///     inflect.Ordinalise(3).ShouldBe("3rd");
        ///     inflect.Ordinalise(101).ShouldBe("101st");
        ///     inflect.Ordinalise(104).ShouldBe("104th");
        ///     inflect.Ordinalise(1000).ShouldBe("1000th");
        ///     inflect.Ordinalise(1001).ShouldBe("1001st");
        /// </example>
        /// <param name="number">The number to ordinalise.</param>
        /// <returns>The ordinalised number.</returns>
        string Ordinalise(string number);

        /// <summary>
        ///     Ordinalises the number.
        /// </summary>
        /// <example>
        ///     inflect.Ordinalise("0").ShouldBe("0th");
        ///     inflect.Ordinalise("1").ShouldBe("1st");
        ///     inflect.Ordinalise("2").ShouldBe("2nd");
        ///     inflect.Ordinalise("3").ShouldBe("3rd");
        ///     inflect.Ordinalise("100").ShouldBe("100th");
        ///     inflect.Ordinalise("101").ShouldBe("101st");
        ///     inflect.Ordinalise("1000").ShouldBe("1000th");
        ///     inflect.Ordinalise("1001").ShouldBe("1001st");
        /// </example>
        /// <param name="number">The number to ordinalise.</param>
        /// <returns>The ordinalised number.</returns>
        string Ordinalise(int number);

        /// <summary>
        ///     Dasherises the word.
        /// </summary>
        /// <example>
        ///     inflect.Dasherise("some_title").ShouldBe("some-title");
        ///     inflect.Dasherise("some-title").ShouldBe("some-title");
        ///     inflect.Dasherise("some_title_goes_here").ShouldBe("some-title-goes-here");
        ///     inflect.Dasherise("some_title and_another").ShouldBe("some-title and-another");
        /// </example>
        /// <param name="underscoredWord">The word to dasherise.</param>
        /// <returns>The dasherised word.</returns>
        string Dasherise(string underscoredWord);
    }


    /// <summary>
    ///     Static inflection helper
    /// </summary>
    public static class Inflector
    {
        private static IInflector _inflector;

        /// <summary>
        ///     Gets or sets the <see cref="IInflector" /> instacne.
        /// </summary>
        /// <param name="value">
        ///     The inflector to set as the default instance, or null to restore the default
        ///     <see cref="EnglishInflector" />.
        /// </param>
        /// <remarks>
        ///     By default the <see cref="EnglishInflector" /> instance used.
        /// </remarks>
        /// <returns>
        ///     The currently set <see cref="IInflector" /> instance.
        /// </returns>
        public static IInflector Instance
        {
            get { return _inflector; }
            set { _inflector = value ?? new EnglishInflector(); }
        }

        static Inflector()
        {
            _inflector = new EnglishInflector();
        }
    }

    #region firebird 数据库
    public class FirebirdDbDatabaseProvider : DatabaseProvider
    {
        public override DbProviderFactory GetFactory()
        {
            return GetFactory("FirebirdSql.Data.FirebirdClient.FirebirdClientFactory, FirebirdSql.Data.FirebirdClient");
        }

        public override string BuildPageQuery(long skip, long take, SQLParts parts, ref object[] args)
        {
            var sql = string.Format("{0}\nROWS @{1} TO @{2}", parts.Sql, args.Length, args.Length + 1);
            args = args.Concat(new object[] { skip + 1, skip + take }).ToArray();
            return sql;
        }

        public override object ExecuteInsert(Database database, IDbCommand cmd, string primaryKeyName)
        {
            cmd.CommandText = cmd.CommandText.TrimEnd();

            if (cmd.CommandText.EndsWith(";"))
                cmd.CommandText = cmd.CommandText.Substring(0, cmd.CommandText.Length - 1);

            cmd.CommandText += " RETURNING " + EscapeSqlIdentifier(primaryKeyName) + ";";
            return ExecuteScalarHelper(database, cmd);
        }

        public override string EscapeSqlIdentifier(string sqlIdentifier)
        {
            return string.Format("\"{0}\"", sqlIdentifier);
        }
    }
    #endregion

    #region MariaDb 数据库
    public class MariaDbDatabaseProvider : DatabaseProvider
    {
        public override DbProviderFactory GetFactory()
        {
            // MariaDb currently uses the MySql data provider
            return GetFactory("MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data, Culture=neutral, PublicKeyToken=c5687fc88969c44d");
        }

        public override string GetParameterPrefix(string connectionString)
        {
            if (connectionString != null && connectionString.IndexOf("Allow User Variables=true") >= 0)
                return "?";
            else
                return "@";
        }

        public override string EscapeSqlIdentifier(string sqlIdentifier)
        {
            return string.Format("`{0}`", sqlIdentifier);
        }

        public override string GetExistsSql()
        {
            return "SELECT EXISTS (SELECT 1 FROM {0} WHERE {1})";
        }
    }
    #endregion


    #region MySql 数据库
    public class MySqlDatabaseProvider : DatabaseProvider
    {
        public override DbProviderFactory GetFactory()
        {
            return GetFactory("MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data, Culture=neutral, PublicKeyToken=c5687fc88969c44d");
        }

        public override string GetParameterPrefix(string connectionString)
        {
            if (connectionString != null && connectionString.IndexOf("Allow User Variables=true") >= 0)
                return "?";
            else
                return "@";
        }

        public override string EscapeSqlIdentifier(string sqlIdentifier)
        {
            return string.Format("`{0}`", sqlIdentifier);
        }

        public override string GetExistsSql()
        {
            return "SELECT EXISTS (SELECT 1 FROM {0} WHERE {1})";
        }
    }
    #endregion

    #region Oracle 数据库
    public class OracleDatabaseProvider : DatabaseProvider
    {
        public override string GetParameterPrefix(string connectionString)
        {
            return ":";
        }

        public override void PreExecute(IDbCommand cmd)
        {
            cmd.GetType().GetProperty("BindByName").SetValue(cmd, true, null);
            cmd.GetType().GetProperty("InitialLONGFetchSize").SetValue(cmd, -1, null);
        }

        public override string BuildPageQuery(long skip, long take, SQLParts parts, ref object[] args)
        {
            if (parts.SqlSelectRemoved.StartsWith("*"))
                throw new Exception("Query must alias '*' when performing a paged query.\neg. select t.* from table t order by t.id");

            // Same deal as SQL Server
            return Singleton<SqlServerDatabaseProvider>.Instance.BuildPageQuery(skip, take, parts, ref args);
        }

        public override DbProviderFactory GetFactory()
        {
            // "Oracle.ManagedDataAccess.Client.OracleClientFactory, Oracle.ManagedDataAccess" is for Oracle.ManagedDataAccess.dll
            // "Oracle.DataAccess.Client.OracleClientFactory, Oracle.DataAccess" is for Oracle.DataAccess.dll
            return GetFactory("Oracle.ManagedDataAccess.Client.OracleClientFactory, Oracle.ManagedDataAccess, Culture=neutral, PublicKeyToken=89b483f429c47342",
                              "Oracle.DataAccess.Client.OracleClientFactory, Oracle.DataAccess");
        }

        public override string EscapeSqlIdentifier(string sqlIdentifier)
        {
            return string.Format("\"{0}\"", sqlIdentifier.ToUpperInvariant());
        }

        public override string GetAutoIncrementExpression(TableInfo ti)
        {
            if (!string.IsNullOrEmpty(ti.SequenceName))
                return string.Format("{0}.nextval", ti.SequenceName);

            return null;
        }

        public override object ExecuteInsert(Database db, IDbCommand cmd, string primaryKeyName)
        {
            if (primaryKeyName != null)
            {
                cmd.CommandText += string.Format(" returning {0} into :newid", EscapeSqlIdentifier(primaryKeyName));
                var param = cmd.CreateParameter();
                param.ParameterName = ":newid";
                param.Value = DBNull.Value;
                param.Direction = ParameterDirection.ReturnValue;
                param.DbType = DbType.Int64;
                cmd.Parameters.Add(param);
                ExecuteNonQueryHelper(db, cmd);
                return param.Value;
            }
            else
            {
                ExecuteNonQueryHelper(db, cmd);
                return -1;
            }
        }
    }
    #endregion

    #region PostgreSQL 数据库

    public class PostgreSQLDatabaseProvider : DatabaseProvider
    {
        public override bool HasNativeGuidSupport
        {
            get { return true; }
        }

        public override DbProviderFactory GetFactory()
        {
            return GetFactory("Npgsql.NpgsqlFactory, Npgsql, Culture=neutral, PublicKeyToken=5d8b90d52f46fda7");
        }

        public override string GetExistsSql()
        {
            return "SELECT CASE WHEN EXISTS(SELECT 1 FROM {0} WHERE {1}) THEN 1 ELSE 0 END";
        }

        public override object MapParameterValue(object value)
        {
            // Don't map bools to ints in PostgreSQL
            if (value.GetType() == typeof(bool))
                return value;

            return base.MapParameterValue(value);
        }

        public override string EscapeSqlIdentifier(string sqlIdentifier)
        {
            return string.Format("\"{0}\"", sqlIdentifier);
        }

        public override object ExecuteInsert(Database db, System.Data.IDbCommand cmd, string primaryKeyName)
        {
            if (primaryKeyName != null)
            {
                cmd.CommandText += string.Format("returning {0} as NewID", EscapeSqlIdentifier(primaryKeyName));
                return ExecuteScalarHelper(db, cmd);
            }
            else
            {
                ExecuteNonQueryHelper(db, cmd);
                return -1;
            }
        }
    }
    #endregion

    #region SQLite 数据库
    public class SQLiteDatabaseProvider : DatabaseProvider
    {
        public override DbProviderFactory GetFactory()
        {
            return GetFactory("System.Data.SQLite.SQLiteFactory, System.Data.SQLite, Culture=neutral, PublicKeyToken=db937bc2d44ff139");
        }

        public override object MapParameterValue(object value)
        {
            if (value.GetType() == typeof(uint))
                return (long)((uint)value);

            return base.MapParameterValue(value);
        }

        public override object ExecuteInsert(Database db, System.Data.IDbCommand cmd, string primaryKeyName)
        {
            if (primaryKeyName != null)
            {
                cmd.CommandText += ";\nSELECT last_insert_rowid();";
                return ExecuteScalarHelper(db, cmd);
            }
            else
            {
                ExecuteNonQueryHelper(db, cmd);
                return -1;
            }
        }

        public override string GetExistsSql()
        {
            return "SELECT EXISTS (SELECT 1 FROM {0} WHERE {1})";
        }
    }
    #endregion

    #region SqlServerCE 数据库

    public class SqlServerCEDatabaseProviders : DatabaseProvider
    {
        public override DbProviderFactory GetFactory()
        {
            return GetFactory("System.Data.SqlServerCe.SqlCeProviderFactory, System.Data.SqlServerCe, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
        }

        public override string BuildPageQuery(long skip, long take, SQLParts parts, ref object[] args)
        {
            if (string.IsNullOrEmpty(parts.SqlOrderBy))
                parts.Sql += " ORDER BY ABS(1)";
            var sqlPage = string.Format("{0}\nOFFSET @{1} ROWS FETCH NEXT @{2} ROWS ONLY", parts.Sql, args.Length, args.Length + 1);
            args = args.Concat(new object[] { skip, take }).ToArray();
            return sqlPage;
        }

        public override object ExecuteInsert(Database db, System.Data.IDbCommand cmd, string primaryKeyName)
        {
            ExecuteNonQueryHelper(db, cmd);
            return db.ExecuteScalar<object>("SELECT @@@IDENTITY AS NewID;");
        }
    }
    #endregion


    #region SqlServer 数据库

    public class SqlServerDatabaseProvider : DatabaseProvider
    {
        public override DbProviderFactory GetFactory()
        {
            return GetFactory("System.Data.SqlClient.SqlClientFactory, System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
        }

        public override string BuildPageQuery(long skip, long take, SQLParts parts, ref object[] args)
        {
            var helper = (PagingHelper)PagingUtility;
            // when the query does not contain an "order by", it is very slow
            if (helper.SimpleRegexOrderBy.IsMatch(parts.SqlSelectRemoved))
            {
                var m = helper.SimpleRegexOrderBy.Match(parts.SqlSelectRemoved);
                if (m.Success)
                {
                    var g = m.Groups[0];
                    parts.SqlSelectRemoved = parts.SqlSelectRemoved.Substring(0, g.Index);
                }
            }
            if (helper.RegexDistinct.IsMatch(parts.SqlSelectRemoved))
            {
                parts.SqlSelectRemoved = "peta_inner.* FROM (SELECT " + parts.SqlSelectRemoved + ") peta_inner";
            }
            var sqlPage = string.Format("SELECT * FROM (SELECT ROW_NUMBER() OVER ({0}) peta_rn, {1}) peta_paged WHERE peta_rn > @{2} AND peta_rn <= @{3}", parts.SqlOrderBy ?? "ORDER BY (SELECT NULL)", parts.SqlSelectRemoved, args.Length, args.Length + 1);
            args = args.Concat(new object[] { skip, skip + take }).ToArray();
            return sqlPage;
        }

        public override object ExecuteInsert(Database db, System.Data.IDbCommand cmd, string primaryKeyName)
        {
            return ExecuteScalarHelper(db, cmd);
        }

        public override string GetExistsSql()
        {
            return "IF EXISTS (SELECT 1 FROM {0} WHERE {1}) SELECT 1 ELSE SELECT 0";
        }

        public override string GetInsertOutputClause(string primaryKeyName)
        {
            return String.Format(" OUTPUT INSERTED.[{0}]", primaryKeyName);
        }
    }
    #endregion

    internal class ArrayKey<T>
    {
        private int _hashCode;

        private T[] _keys;

        public ArrayKey(T[] keys)
        {
            // Store the keys
            _keys = keys;

            // Calculate the hashcode
            _hashCode = 17;
            foreach (var k in keys)
            {
                _hashCode = _hashCode * 23 + (k == null ? 0 : k.GetHashCode());
            }
        }

        private bool Equals(ArrayKey<T> other)
        {
            if (other == null)
                return false;

            if (other._hashCode != _hashCode)
                return false;

            if (other._keys.Length != _keys.Length)
                return false;

            for (int i = 0; i < _keys.Length; i++)
            {
                if (!object.Equals(_keys[i], other._keys[i]))
                    return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ArrayKey<T>);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }
    }


    internal static class AutoSelectHelper
    {
        private static Regex rxSelect = new Regex(@"\A\s*(SELECT|EXECUTE|CALL|WITH|SET|DECLARE)\s",
            RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static Regex rxFrom = new Regex(@"\A\s*FROM\s",
            RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        public static string AddSelectClause<T>(IProvider provider, string sql, IMapper defaultMapper)
        {
            if (sql.StartsWith(";"))
                return sql.Substring(1);

            if (!rxSelect.IsMatch(sql))
            {
                var pd = PocoData.ForType(typeof(T), defaultMapper);
                var tableName = provider.EscapeTableName(pd.TableInfo.TableName);
                string cols = pd.Columns.Count != 0
                    ? string.Join(", ", (from c in pd.QueryColumns select tableName + "." + provider.EscapeSqlIdentifier(c)).ToArray())
                    : "NULL";
                if (!rxFrom.IsMatch(sql))
                    sql = string.Format("SELECT {0} FROM {1} {2}", cols, tableName, sql);
                else
                    sql = string.Format("SELECT {0} {1}", cols, sql);
            }
            return sql;
        }
    }


    internal class Cache<TKey, TValue>
    {
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private Dictionary<TKey, TValue> _map = new Dictionary<TKey, TValue>();

        public int Count
        {
            get { return _map.Count; }
        }

        public TValue Get(TKey key, Func<TValue> factory)
        {
            // Check cache
            _lock.EnterReadLock();
            TValue val;
            try
            {
                if (_map.TryGetValue(key, out val))
                    return val;
            }
            finally
            {
                _lock.ExitReadLock();
            }

            // Cache it
            _lock.EnterWriteLock();
            try
            {
                // Check again
                if (_map.TryGetValue(key, out val))
                    return val;

                // Create it
                val = factory();

                // Store it
                _map.Add(key, val);

                // Done
                return val;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Flush()
        {
            // Cache it
            _lock.EnterWriteLock();
            try
            {
                _map.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }


    internal static class EnumMapper
    {
        private static Cache<Type, Dictionary<string, object>> _types = new Cache<Type, Dictionary<string, object>>();

        public static object EnumFromString(Type enumType, string value)
        {
            Dictionary<string, object> map = _types.Get(enumType, () =>
            {
                var values = Enum.GetValues(enumType);

                var newmap = new Dictionary<string, object>(values.Length, StringComparer.InvariantCultureIgnoreCase);

                foreach (var v in values)
                {
                    newmap.Add(v.ToString(), v);
                }

                return newmap;
            });

            return map[value];
        }
    }


    /// <summary>
    ///     Represents the contract for a paging helper.
    /// </summary>
    public interface IPagingHelper
    {
        /// <summary>
        ///     Splits the given <paramref name="sql" /> into <paramref name="parts" />;
        /// </summary>
        /// <param name="sql">The SQL to split.</param>
        /// <param name="parts">The SQL parts.</param>
        /// <returns><c>True</c> if the SQL could be split; else, <c>False</c>.</returns>
        bool SplitSQL(string sql, out SQLParts parts);
    }


    public class PagingHelper : IPagingHelper
    {
        public Regex RegexColumns = new Regex(@"\A\s*SELECT\s+((?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|.)*?)(?<!,\s+)\bFROM\b",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

        public Regex RegexDistinct = new Regex(@"\ADISTINCT\s",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

        public Regex RegexOrderBy =
            new Regex(
                @"\bORDER\s+BY\s+(?!.*?(?:\)|\s+)AS\s)(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\[\]`""\w\(\)\.])+(?:\s+(?:ASC|DESC))?(?:\s*,\s*(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\[\]`""\w\(\)\.])+(?:\s+(?:ASC|DESC))?)*",
                RegexOptions.RightToLeft | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

        public Regex SimpleRegexOrderBy = new Regex(@"\bORDER\s+BY\s+", RegexOptions.RightToLeft | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

        public static IPagingHelper Instance { get; private set; }

        static PagingHelper()
        {
            Instance = new PagingHelper();
        }

        /// <summary>
        ///     Splits the given <paramref name="sql" /> into <paramref name="parts" />;
        /// </summary>
        /// <param name="sql">The SQL to split.</param>
        /// <param name="parts">The SQL parts.</param>
        /// <returns><c>True</c> if the SQL could be split; else, <c>False</c>.</returns>
        public bool SplitSQL(string sql, out SQLParts parts)
        {
            parts.Sql = sql;
            parts.SqlSelectRemoved = null;
            parts.SqlCount = null;
            parts.SqlOrderBy = null;

            // Extract the columns from "SELECT <whatever> FROM"
            var m = RegexColumns.Match(sql);
            if (!m.Success)
                return false;

            // Save column list and replace with COUNT(*)
            var g = m.Groups[1];
            parts.SqlSelectRemoved = sql.Substring(g.Index);

            if (RegexDistinct.IsMatch(parts.SqlSelectRemoved))
                parts.SqlCount = sql.Substring(0, g.Index) + "COUNT(" + m.Groups[1].ToString().Trim() + ") " + sql.Substring(g.Index + g.Length);
            else
                parts.SqlCount = sql.Substring(0, g.Index) + "COUNT(*) " + sql.Substring(g.Index + g.Length);

            // Look for the last "ORDER BY <whatever>" clause not part of a ROW_NUMBER expression
            m = SimpleRegexOrderBy.Match(parts.SqlCount);
            if (m.Success)
            {
                g = m.Groups[0];
                parts.SqlOrderBy = g + parts.SqlCount.Substring(g.Index + g.Length);
                parts.SqlCount = parts.SqlCount.Substring(0, g.Index);
            }

            return true;
        }
    }


    internal static class ParametersHelper
    {
        private static Regex rxParams = new Regex(@"(?<!@)@\w+", RegexOptions.Compiled);
        // Helper to handle named parameters from object properties
        public static string ProcessParams(string sql, object[] args_src, List<object> args_dest)
        {
            return rxParams.Replace(sql, m =>
            {
                string param = m.Value.Substring(1);

                object arg_val;

                int paramIndex;
                if (int.TryParse(param, out paramIndex))
                {
                    // Numbered parameter
                    if (paramIndex < 0 || paramIndex >= args_src.Length)
                        throw new ArgumentOutOfRangeException(string.Format("Parameter '@{0}' specified but only {1} parameters supplied (in `{2}`)", paramIndex,
                            args_src.Length, sql));
                    arg_val = args_src[paramIndex];
                }
                else
                {
                    // Look for a property on one of the arguments with this name
                    bool found = false;
                    arg_val = null;
                    foreach (var o in args_src)
                    {
                        var pi = o.GetType().GetProperty(param);
                        if (pi != null)
                        {
                            arg_val = pi.GetValue(o, null);
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                        throw new ArgumentException(
                            string.Format("Parameter '@{0}' specified but none of the passed arguments have a property with this name (in '{1}')", param, sql));
                }

                // Expand collections to parameter lists
                if ((arg_val as System.Collections.IEnumerable) != null &&
                    (arg_val as string) == null &&
                    (arg_val as byte[]) == null)
                {
                    var sb = new StringBuilder();
                    foreach (var i in arg_val as System.Collections.IEnumerable)
                    {
                        sb.Append((sb.Length == 0 ? "@" : ",@") + args_dest.Count.ToString());
                        args_dest.Add(i);
                    }
                    return sb.ToString();
                }
                else
                {
                    args_dest.Add(arg_val);
                    return "@" + (args_dest.Count - 1).ToString();
                }
            }
                );
        }
    }


    internal static class Singleton<T> where T : new()
    {
        public static T Instance = new T();
    }


    /// <summary>
    ///     Presents the SQL parts.
    /// </summary>
    public struct SQLParts
    {
        /// <summary>
        ///     The SQL.
        /// </summary>
        public string Sql;

        /// <summary>
        ///     The SQL count.
        /// </summary>
        public string SqlCount;

        /// <summary>
        ///     The SQL Select
        /// </summary>
        public string SqlSelectRemoved;

        /// <summary>
        ///     The SQL Order By
        /// </summary>
        public string SqlOrderBy;
    }

#pragma warning restore 1066,1570,1573,1591
}