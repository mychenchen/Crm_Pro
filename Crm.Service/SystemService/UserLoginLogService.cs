using Crm.Repository.DB;
using Crm.Repository.TbEntity;
using System.Collections.Generic;
using System.Linq;

namespace Crm.Service.SystemService
{
    /// <summary>
    /// 用户管理
    /// </summary>
    public class UserLoginLogService : IUserLoginLogService
    {
        /// <summary>
        /// 数据库
        /// </summary>
        protected readonly MyDbContext _mydb;

        public UserLoginLogService(MyDbContext mydb)
        {
            _mydb = mydb;
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="model"></param>
        public List<UserLoginLog> GetList()
        {
            return _mydb.UserLoginLog.ToList();
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="page"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<UserLoginLog> GetPageList(string name, int page, int rows, ref int count)
        {
            var list = _mydb.UserLoginLog.Where(a => a.IsDelete == 0);
            if (!string.IsNullOrEmpty(name))
            {
                list = list.Where(a => a.UserName.Contains(name));
            }
            var data = list.OrderByDescending(a => a.CreateTime)
                .Skip((page - 1) * rows).Take(rows).ToList();
            count = list.Count();
            return data;
        }

        /// <summary>
        /// 保存日志
        /// </summary>
        /// <param name="model"></param>
        public void SaveLog(UserLoginLog model)
        {
            _mydb.UserLoginLog.Add(model);
            _mydb.SaveChanges();
        }
    }
}
