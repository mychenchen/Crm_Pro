using Crm.Repository.TbEntity;
using System;
using System.Collections.Generic;

namespace Crm.Service.ProductManageService
{
    /// <summary>
    /// 新闻
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// 查询列表
        /// </summary>
        /// <returns></returns>
        List<ProductEntity> GetList();

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="title"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<ProductEntity> GetPageList(string title, int page, int rows, ref int count);

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="model"></param>
        ProductEntity GetModel(Guid gid);

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="model"></param>
        void AddUpdateModel(ProductEntity model);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="gid"></param>
        /// <param name="isDelete">true 真删除 false 假删除</param>
        bool Delete(Guid gid, bool isDelete = false);

        /// <summary>
        /// 产品上架/下架
        /// </summary>
        /// <param name="gid"></param>
        /// <param name="onShelfStatus"></param>
        bool UpdateOnShelf(Guid gid, int onShelfStatus);

        #region 产品视频详情

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="proId">产品ID</param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<ProductVideoEntity> GetVideoList(Guid proId, int page, int rows, ref int count);

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="model"></param>
        ProductVideoEntity GetVideoModel(Guid gid);

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="model"></param>
        void AddUpdateVideoModel(ProductVideoEntity model);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="gid"></param>
        /// <param name="isDelete">true 真删除 false 假删除</param>
        bool DeleteVideo(Guid gid, bool isDelete = false);
        #endregion

    }
}
