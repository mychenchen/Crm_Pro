using Crm.Repository.DB;
using Crm.Repository.TbEntity;
using Crm.Service.BaseHelper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crm.Service.GatewayService
{
    /// <summary>
    /// tab菜单
    /// </summary>
    public class TabMenuService : BaseServiceRepository<TabMenuEntity>, ITabMenuService
    {

        public TabMenuService(MyDbContext mydb) : base(mydb)
        {
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="name"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<TabMenuEntity> GetPageList(string name, int page, int rows, ref int count)
        {
            var list = myDbContext.TabMenu.Where(a => a.IsDelete == 0 && a.ParentGid == Guid.Empty);
            if (!string.IsNullOrEmpty(name))
            {
                list = list.Where(a => a.Name.Contains(name));
            }
            var data = list.OrderBy(a => a.SortNum)
                .Skip((page - 1) * rows).Take(rows).ToList();
            count = list.Count();
            return data;
        }

    }
}
