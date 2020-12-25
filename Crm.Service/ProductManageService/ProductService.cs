using Crm.Repository.DB;
using Crm.Repository.TbEntity;
using Crm.Service.BaseHelper;
using Currency.Common.LogManange;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Crm.Service.ProductManageService
{
    /// <summary>
    /// 产品(课程)
    /// </summary>
    public class ProductService : BaseServiceRepository<ProductEntity>, IProductService
    {
        public ProductService(MyDbContext mydb) : base(mydb)
        {
        }

        #region 产品信息

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
            var list = myDbContext.Product.Where(a => a.IsDelete == 0);
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
        /// 前台搜索查询
        /// </summary>
        /// <param name="title"></param>
        /// <param name="typeId"></param>
        /// <param name="isMoney"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<ProductEntity> WebAppGetPageList(string title, Guid typeId, int isMoney, int page, int rows, ref int count)
        {
            var list = myDbContext.Product.Where(a => a.IsDelete == 0 && a.OnShelfStatus == 1);
            if (!string.IsNullOrEmpty(title))
            {
                list = list.Where(a => a.Title.Contains(title) || a.Subtitle.Contains(title));
            }
            if (typeId != Guid.Empty)
            {
                list = list.Where(a => a.ProductTypeId == typeId);
            }
            if (isMoney == 0)
            {
                list = list.Where(a => a.Price == 0);
            }
            else if (isMoney == 1)
            {
                list = list.Where(a => a.Price > 0);
            }
            var data = list.OrderByDescending(a => a.CreateTime)
                .Skip((page - 1) * rows).Take(rows).ToList();
            count = list.Count();
            return data;
        }

        #endregion

        #region 产品视频详情

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="proId">产品ID</param>
        /// <returns></returns>
        public async Task<List<ProductVideoEntity>> GetAllVideo(Guid proId)
        {
            var list = await myDbContext.ProductVideo
                .Where(a => a.IsDelete == 0 && a.ProId == proId)
                .OrderBy(a => a.VideoName)
                .ToListAsync();
            return list;
        }

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
            var list = myDbContext.ProductVideo.Where(a => a.ProId == proId);
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
            return myDbContext.ProductVideo.AsNoTracking().FirstOrDefault(a => a.Id == gid);
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
                myDbContext.ProductVideo.Add(model);
            }
            else
            {
                myDbContext.Update(model);
            }
            myDbContext.SaveChanges();
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
                myDbContext.ProductVideo.Remove(model);
            }
            else
            {
                model.IsDelete = 1;
                myDbContext.ProductVideo.Update(model);
            }
            myDbContext.SaveChanges();
            return true;
        }
        #endregion

    }
}
