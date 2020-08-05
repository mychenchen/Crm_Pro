using Crm.Repository.TbEntity;
using System;
using System.Collections.Generic;

namespace Crm.Service.GatewayService
{
    /// <summary>
    /// 新闻
    /// </summary>
    public interface IHotNewsService
    {
        /// <summary>
        /// 查询列表
        /// </summary>
        /// <returns></returns>
        List<HotNewsEntity> GetList();

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="title"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<HotNewsEntity> GetPageList(string title, int page, int rows, ref int count);

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="model"></param>
        HotNewsEntity GetModel(Guid gid);

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="model"></param>
        void AddUpdateModel(HotNewsEntity model);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="gid"></param>
        /// <param name="isDelete">true 真删除 false 假删除</param>
        bool Delete(Guid gid, bool isDelete = false);

    }
}
