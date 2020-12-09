using Crm.Repository.DB;
using Crm.Repository.TbEntity;
using Crm.Service.BaseHelper;
using System.Collections.Generic;
using System.Linq;

namespace Crm.Service.SystemService
{
    /// <summary>
    /// 用户管理
    /// </summary>
    public class UserLoginLogService : BaseServiceRepository<UserLoginLog>, IUserLoginLogService
    {

        public UserLoginLogService(MyDbContext mydb) : base(mydb)
        {
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
            var list = myDbContext.UserLoginLog.Where(a => a.IsDelete == 0);
            if (!string.IsNullOrEmpty(name))
            {
                list = list.Where(a => a.UserName.Contains(name));
            }
            var data = list.OrderByDescending(a => a.CreateTime)
                .Skip((page - 1) * rows).Take(rows).ToList();
            count = list.Count();
            return data;
        }

    }
}
