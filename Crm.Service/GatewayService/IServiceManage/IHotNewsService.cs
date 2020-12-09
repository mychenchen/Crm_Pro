using Crm.Repository.TbEntity;
using Crm.Service.BaseHelper;
using System;
using System.Collections.Generic;

namespace Crm.Service.GatewayService
{
    /// <summary>
    /// 新闻
    /// </summary>
    public interface IHotNewsService : IBaseServiceRepository<HotNewsEntity>
    {
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="title"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<HotNewsEntity> GetPageList(string title, int page, int rows, ref int count);

    }
}
