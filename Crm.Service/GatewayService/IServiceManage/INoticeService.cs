using Crm.Repository.TbEntity;
using Crm.Service.BaseHelper;
using System;
using System.Collections.Generic;

namespace Crm.Service.GatewayService
{
    /// <summary>
    /// 公告公示
    /// </summary>
    public interface INoticeService : IBaseServiceRepository<NoticeEntity>
    {
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="title"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<NoticeEntity> GetPageList(string title, int page, int rows, ref int count);

    }
}
