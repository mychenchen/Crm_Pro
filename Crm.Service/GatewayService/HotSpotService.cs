using Crm.Repository.DB;
using Crm.Repository.TbEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using static Currency.Common.NetCoreDIModuleRegister;

namespace Crm.Service.GatewayService
{
    /// <summary>
    /// 热点轮播
    /// UseDI特性用于注入必须加
    /// </summary>
    [UseDI(ServiceLifetime.Scoped, typeof(IHotSpotService))]
    public class HotSpotService : IHotSpotService
    {
        /// <summary>
        /// 数据库
        /// </summary>
        protected readonly MyDbContext _mydb;

        public HotSpotService(MyDbContext mydb)
        {
            _mydb = mydb;
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="model"></param>
        public List<HotSpotEntity> GetList()
        {
            return _mydb.HotSpot.ToList();
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="title"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<HotSpotEntity> GetPageList(string title, int page, int rows, ref int count)
        {
            var list = _mydb.HotSpot.Where(a => a.IsDelete == 0);
            if (!string.IsNullOrEmpty(title))
            {
                list = list.Where(a => a.ImgTitle.Contains(title));
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
        public HotSpotEntity GetModel(Guid gid)
        {
            return _mydb.HotSpot.AsNoTracking().FirstOrDefault(a => a.Id == gid);
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="model"></param>
        public void AddUpdateModel(HotSpotEntity model)
        {
            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();
                model.CreateTime = DateTime.Now;
                _mydb.HotSpot.Add(model);
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
                _mydb.HotSpot.Remove(model);
            }
            else
            {
                model.IsDelete = 1;
                _mydb.HotSpot.Update(model);
            }
            _mydb.SaveChanges();
            return true;
        }

    }
}
