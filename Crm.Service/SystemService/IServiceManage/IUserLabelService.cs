using Crm.Repository.TbEntity;
using Crm.Service.BaseHelper;
using System.Collections.Generic;

namespace Crm.Service.SystemService
{
    /// <summary>
    /// 用户标签
    /// </summary>
    public interface IUserLabelService : IBaseServiceRepository<UserLabel>
    {
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="page"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<UserLabel> GetPageList(string name, int page, int rows, ref int count);

    }
}
