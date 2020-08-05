using Crm.Repository.DB;
using Crm.Repository.TbEntity;
using Currency.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Crm.Service.RabbitService
{
    /// <summary>
    /// 处理
    /// </summary>
    public class BatchHandle
    {

        /// <summary>
        /// 数据库
        /// </summary>
        protected readonly MyDbContext _mydb;

        public BatchHandle(MyDbContext mydb)
        {
            //_mydb = DI.GetService<MyDbContext>();
            _mydb = mydb;
        }

        public void SaveHotNews(string jsonStr)
        {
            var info = jsonStr.ToObject<HotNewsEntity>();
            _mydb.HotNews.Add(info);
            _mydb.SaveChanges();
        }
    }
}
