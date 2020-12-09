using Crm.Repository.TbEntity;
using Crm.Service.BaseHelper;
using System;
using System.Collections.Generic;

namespace Crm.Service.ProductManageService
{
    /// <summary>
    /// 产品分类(课程分类)
    /// </summary>
    public interface IProductTypeService: IBaseServiceRepository<ProductTypeEntity>
    {
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="title"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<ProductTypeEntity> GetPageList(string title, int page, int rows, ref int count);

    }
}
