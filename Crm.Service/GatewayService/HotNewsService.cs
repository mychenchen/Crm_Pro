using Crm.Repository.DB;
using Crm.Repository.TbEntity;
using Crm.Service.BaseHelper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crm.Service.GatewayService
{
    /// <summary>
    /// 新闻
    /// </summary>
    public class HotNewsService : BaseServiceRepository<HotNewsEntity>, IHotNewsService
    {

        public HotNewsService(MyDbContext mydb) : base(mydb)
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
        public List<HotNewsEntity> GetPageList(string title, int page, int rows, ref int count)
        {
            var list = myDbContext.HotNews.Where(a => a.IsDelete == 0);
            if (!string.IsNullOrEmpty(title))
            {
                list = list.Where(a => a.Title.Contains(title) || a.Subtitle.Contains(title));
            }
            var data = list.OrderByDescending(a => a.CreateTime)
                .Skip((page - 1) * rows).Take(rows).ToList();
            count = list.Count();
            return data;
        }

    }
}
