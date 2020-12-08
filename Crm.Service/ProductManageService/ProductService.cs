using Crm.Repository.DB;
using Crm.Repository.TbEntity;
using Currency.Common.LogManange;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crm.Service.ProductManageService
{
    /// <summary>
    /// 产品(课程)
    /// </summary>
    public class ProductService : IProductService
    {
        /// <summary>
        /// 数据库
        /// </summary>
        protected readonly MyDbContext _mydb;

        public ProductService(MyDbContext mydb)
        {
            _mydb = mydb;
        }

        #region 产品信息

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="model"></param>
        public List<ProductEntity> GetList()
        {
            return _mydb.Product.ToList();
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="title"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<ProductEntity> GetPageList(string title, int page, int rows, ref int count)
        {
            var list = _mydb.Product.Where(a => a.IsDelete == 0);
            if (!string.IsNullOrEmpty(title))
            {
                list = list.Where(a => a.Title.Contains(title) || a.Subtitle.Contains(title));
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
        public ProductEntity GetModel(Guid gid)
        {
            return _mydb.Product.AsNoTracking().FirstOrDefault(a => a.Id == gid);
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="model"></param>
        public void AddUpdateModel(ProductEntity model)
        {
            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();
                model.CreateTime = DateTime.Now;
                _mydb.Product.Add(model);
            }
            else
            {
                _mydb.Update(model);
            }
            _mydb.SaveChanges();
        }

        /// <summary>
        /// 产品上架/下架
        /// </summary>
        /// <param name="gid"></param>
        /// <param name="onShelfStatus"></param>
        public bool UpdateOnShelf(Guid gid, int onShelfStatus)
        {
            try
            {
                var info = _mydb.Product.AsNoTracking().FirstOrDefault(a => a.Id == gid);
                info.IssueDateTime = DateTime.Now;
                info.OnShelfStatus = onShelfStatus;
                _mydb.Update(info);
                _mydb.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
                return false;
            }
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
                _mydb.Product.Remove(model);
            }
            else
            {
                model.IsDelete = 1;
                _mydb.Product.Update(model);
            }
            _mydb.SaveChanges();
            return true;
        }

        #endregion

        #region 产品视频详情

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="proId">产品ID</param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<ProductVideoEntity> GetVideoList(Guid proId, int page, int rows, ref int count)
        {
            var list = _mydb.ProductVideo.Where(a => a.ProId == proId);
            var data = list.OrderBy(a => a.VideoName)
                .Skip((page - 1) * rows).Take(rows).ToList();
            count = list.Count();
            return data;
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="model"></param>
        public ProductVideoEntity GetVideoModel(Guid gid)
        {
            return _mydb.ProductVideo.AsNoTracking().FirstOrDefault(a => a.Id == gid);
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="model"></param>
        public void AddUpdateVideoModel(ProductVideoEntity model)
        {
            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();
                model.CreateTime = DateTime.Now;
                _mydb.ProductVideo.Add(model);
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
        public bool DeleteVideo(Guid gid, bool isDelete = false)
        {
            var model = GetVideoModel(gid);
            if (model == null)
                return false;

            if (isDelete)
            {
                _mydb.ProductVideo.Remove(model);
            }
            else
            {
                model.IsDelete = 1;
                _mydb.ProductVideo.Update(model);
            }
            _mydb.SaveChanges();
            return true;
        }
        #endregion

    }
}
