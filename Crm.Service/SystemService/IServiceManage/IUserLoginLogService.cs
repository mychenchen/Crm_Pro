using Crm.Repository.TbEntity;
using Crm.Service.BaseHelper;
using System.Collections.Generic;

namespace Crm.Service.SystemService
{
    /// <summary>
    /// 用户登陆日志
    /// </summary>
    public interface IUserLoginLogService: IBaseServiceRepository<UserLoginLog>
    {
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="page"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<UserLoginLog> GetPageList(string name, int page, int rows, ref int count);

    }
}
