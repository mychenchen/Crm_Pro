using Crm.Repository.MapperEntity;
using Crm.Repository.TbEntity;
using Crm.Service.BaseHelper;
using System.Collections.Generic;

namespace Crm.Service.CustomerService
{
    /// <summary>
    /// 用户学生
    /// </summary>
    public interface IUserStudentService : IBaseServiceRepository<UserStudentEntity>
    {
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isVip"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<UserStudentMapper> GetMapperPageList(string name, int isVip, int page, int rows, ref int count);

    }
}
