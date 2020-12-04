using Crm.Repository.TbEntity;
using System;
using System.Collections.Generic;

namespace Crm.Service.ProductManageService
{
    /// <summary>
    /// 产品分类(课程分类)
    /// </summary>
    public interface IProductTypeService
    {
        /// <summary>
        /// 查询列表
        /// </summary>
        /// <returns></returns>
        List<ProductTypeEntity> GetList();

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="title"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<ProductTypeEntity> GetPageList(string title, int page, int rows, ref int count);

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="model"></param>
        ProductTypeEntity GetModel(Guid gid);

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="model"></param>
        List<ProductTypeEntity> GetListByPid(Guid pid);

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="model"></param>
        void AddUpdateModel(ProductTypeEntity model);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="gid"></param>
        /// <param name="isDelete">true 真删除 false 假删除</param>
        bool Delete(Guid gid, bool isDelete = false);

    }
}
