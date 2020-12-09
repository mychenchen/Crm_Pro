using Crm.Repository.DB;
using Crm.Repository.TbEntity;
using Crm.Service.BaseHelper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crm.Service.ProductManageService
{
    /// <summary>
    /// 产品分类(课程分类)
    /// </summary>
    public class ProductTypeService : BaseServiceRepository<ProductTypeEntity>, IProductTypeService
    {
        public ProductTypeService(MyDbContext mydb) : base(mydb)
        {
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="title"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<ProductTypeEntity> GetPageList(string title, int page, int rows, ref int count)
        {
            var list = myDbContext.ProductType.Where(a => a.PID == Guid.Empty && a.IsDelete == 0);
            if (!string.IsNullOrEmpty(title))
            {
                title = title.ToUpper();
                list = list.Where(a => a.TypeName.Contains(title) || a.TypeCode.Contains(title));
            }
            var data = list.OrderByDescending(a => a.CreateTime)
                .Skip((page - 1) * rows).Take(rows).ToList();
            count = list.Count();
            return data;
        }

    }
}
