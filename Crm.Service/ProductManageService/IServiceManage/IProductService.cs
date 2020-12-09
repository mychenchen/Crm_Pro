using Crm.Repository.TbEntity;
using Crm.Service.BaseHelper;
using System;
using System.Collections.Generic;

namespace Crm.Service.ProductManageService
{
    /// <summary>
    /// 产品(课程)
    /// </summary>
    public interface IProductService : IBaseServiceRepository<ProductEntity>
    {
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="title"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<ProductEntity> GetPageList(string title, int page, int rows, ref int count);

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
