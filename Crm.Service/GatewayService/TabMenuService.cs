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
    /// tab菜单
    /// UseDI特性用于注入必须加
    /// </summary>
    [UseDI(ServiceLifetime.Scoped, typeof(ITabMenuService))]
    public class TabMenuService : ITabMenuService
    {
        /// <summary>
        /// 数据库
        /// </summary>
        protected readonly MyDbContext _mydb;

        public TabMenuService(MyDbContext mydb)
        {
            _mydb = mydb;
        }

        /// <summary>
        /// 查询
        /// </summary>
        public List<TabMenuEntity> GetList()
        {
            return _mydb.TabMenu.OrderBy(a => a.SortNum).ThenBy(a => a.ParentGid).ToList();
        }

        /// <summary>
        /// 查询子级菜单
        /// </summary>
        /// <param name="pid"></param>
        public List<TabMenuEntity> GetListByPid(Guid pid)
        {
            return _mydb.TabMenu.Where(a => a.ParentGid == pid).OrderBy(a => a.SortNum).ToList();
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
            var list = _mydb.TabMenu.Where(a => a.IsDelete == 0 && a.ParentGid == Guid.Empty);
            if (!string.IsNullOrEmpty(name))
            {
                list = list.Where(a => a.Name.Contains(name));
            }
            var data = list.OrderBy(a => a.SortNum)
                .Skip((page - 1) * rows).Take(rows).ToList();
            count = list.Count();
            return data;
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="model"></param>
        public TabMenuEntity GetModel(Guid gid)
        {
            return _mydb.TabMenu.AsNoTracking().FirstOrDefault(a => a.Id == gid);
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="model"></param>
        public void AddUpdateModel(TabMenuEntity model)
        {
            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();
                model.CreateTime = DateTime.Now;
                _mydb.TabMenu.Add(model);
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
                _mydb.TabMenu.Remove(model);
            }
            else
            {
                model.IsDelete = 1;
                _mydb.TabMenu.Update(model);
            }
            _mydb.SaveChanges();
            return true;
        }

    }
}
