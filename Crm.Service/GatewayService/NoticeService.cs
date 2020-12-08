using Crm.Repository.DB;
using Crm.Repository.TbEntity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crm.Service.GatewayService
{
    /// <summary>
    /// 公告公示
    /// </summary>
    public class NoticeService : INoticeService
    {
        /// <summary>
        /// 数据库
        /// </summary>
        protected readonly MyDbContext _mydb;

        public NoticeService(MyDbContext mydb)
        {
            _mydb = mydb;
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="model"></param>
        public List<NoticeEntity> GetList()
        {
            return _mydb.Notice.ToList();
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="title"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<NoticeEntity> GetPageList(string title, int page, int rows, ref int count)
        {
            var list = _mydb.Notice.Where(a => a.IsDelete == 0);
            if (!string.IsNullOrEmpty(title))
            {
                list = list.Where(a => a.Title.Contains(title));
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
        public NoticeEntity GetModel(Guid gid)
        {
            return _mydb.Notice.AsNoTracking().FirstOrDefault(a => a.Id == gid);
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="model"></param>
        public void AddUpdateModel(NoticeEntity model)
        {
            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();
                _mydb.Notice.Add(model);
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
                _mydb.Notice.Remove(model);
            }
            else
            {
                model.IsDelete = 1;
                _mydb.Notice.Update(model);
            }
            _mydb.SaveChanges();
            return true;
        }

    }
}
