using Crm.Repository.TbEntity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Crm.Service.SystemService
{
    /// <summary>
    /// 用户登陆日志
    /// </summary>
    public interface IUserLoginLogService
    {
        /// <summary>
        /// 查询列表
        /// </summary>
        /// <returns></returns>
        List<UserLoginLog> GetList();

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="page"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<UserLoginLog> GetPageList(string name, int page, int rows, ref int count);

        /// <summary>
        /// 保存日志
        /// </summary>
        /// <param name="model"></param>
        void SaveLog(UserLoginLog model);
    }
}
