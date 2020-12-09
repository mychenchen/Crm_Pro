using Crm.Repository.TbEntity;
using Crm.Service.BaseHelper;
using System.Collections.Generic;

namespace Crm.Service.GatewayService
{
    /// <summary>
    /// tab菜单
    /// </summary>
    public interface ITabMenuService : IBaseServiceRepository<TabMenuEntity>
    {
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="name"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<TabMenuEntity> GetPageList(string name, int page, int rows, ref int count);

    }
}
