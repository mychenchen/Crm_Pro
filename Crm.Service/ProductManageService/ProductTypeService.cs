using Crm.Repository.DB;
using Crm.Repository.TbEntity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crm.Service.ProductManageService
{
    /// <summary>
    /// 产品分类(课程分类)
    /// </summary>
    public class ProductTypeService : IProductTypeService
    {
        /// <summary>
        /// 数据库
        /// </summary>
        protected readonly MyDbContext _mydb;

        public ProductTypeService(MyDbContext mydb)
        {
            _mydb = mydb;
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="model"></param>
        public List<ProductTypeEntity> GetList()
        {
            return _mydb.ProductType.ToList();
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
            var list = _mydb.ProductType.Where(a =>a.PID == Guid.Empty && a.IsDelete == 0);
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

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="model"></param>
        public ProductTypeEntity GetModel(Guid gid)
        {
            return _mydb.ProductType.AsNoTracking().FirstOrDefault(a => a.Id == gid);
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="model"></param>
        public List<ProductTypeEntity> GetListByPid(Guid pid)
        {
            return _mydb.ProductType.Where(a => a.PID == pid).ToList();
        }

        /// <summary>
        /// 添加修改
        /// </summary>
        /// <param name="model"></param>
        public void AddUpdateModel(ProductTypeEntity model)
        {
            model.CreateTime = DateTime.Now;
            model.IsDelete = 0;
            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();
                _mydb.ProductType.Add(model);
            }
            else
            {
                _mydb.Update(model);
            }
            _mydb.SaveChanges();
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="gid"></param>
        /// <param name="isDelete">true 真删除 false 假删除</param>
        public bool Delete(Guid gid, bool isDelete = false)
        {
            var model = GetModel(gid);
            if (model == null)
                return false;

            if (isDelete)
            {
                _mydb.ProductType.Remove(model);
            }
            else
            {
                model.IsDelete = 1;
                _mydb.ProductType.Update(model);
            }
            _mydb.SaveChanges();
            return true;
        }
    }
}
