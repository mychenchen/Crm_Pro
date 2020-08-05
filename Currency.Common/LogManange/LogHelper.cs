using log4net;
using log4net.Config;
using log4net.Repository;
using System.IO;

namespace Currency.Common.LogManange
{
    public class LogHelper
    {
        private static ILoggerRepository repository { get; set; }
        private static ILog _log;
        private static ILog log
        {
            get
            {
                if (_log == null)
                {
                    Configure();
                }
                return _log;
            }
        }

        public static void Configure(string repositoryName = "NETCoreRepository", string configFile = "log4net.config")
        {
            repository = LogManager.CreateRepository(repositoryName);
            XmlConfigurator.Configure(repository, new FileInfo(configFile));
            _log = LogManager.GetLogger(repositoryName, "");
        }

        public static void Info(string msg)
        {
            log.Info(msg);
        }

        public static void Warn(string msg)
        {
            log.Warn(msg);
        }

        public static void Error(string msg)
        {
            log.Error(msg);
        }

    }
}
