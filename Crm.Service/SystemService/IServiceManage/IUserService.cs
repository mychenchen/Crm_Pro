using Crm.Repository.MapperEntity;
using Crm.Repository.TbEntity;
using Crm.Service.BaseHelper;
using System;
using System.Collections.Generic;

namespace Crm.Service.SystemService
{
    /// <summary>
    /// 系统用户接口
    /// </summary>
    public interface IUserService : IBaseServiceRepository<User>
    {
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="page"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<UserMapper> GetMapperPageList(string name, int page, int rows, ref int count);

    }
}
