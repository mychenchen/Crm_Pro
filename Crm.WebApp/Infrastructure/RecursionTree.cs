using Crm.Repository.MapperEntity;
using Crm.Repository.TbEntity;
using Crm.WebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crm.WebApp.Infrastructure
{
    /// <summary>
    /// 递归树行结构
    /// </summary>
    public class RecursionTree
    {
        /// <summary>
        /// layui树行组件
        /// </summary>
        /// <param name="list"></param>
        /// <param name="pid"></param>
        /// <returns></returns>
        public static List<LayuiTreeModel> LayuiTreeList(List<ProductTypeEntity> list, Guid pid)
        {
            var resList = new List<LayuiTreeModel>();
            var list_p = list.Where(a => a.PID == pid).ToList();
            list_p.ForEach(a =>
            {
                LayuiTreeModel model = new LayuiTreeModel()
                {
                    title = a.TypeName,
                    id = a.Id.ToString()
                };
                model.children = LayuiTreeList(list, a.Id);

                resList.Add(model);
            });

            return resList;
        }

        /// <summary>
        /// layui树行组件
        /// </summary>
        /// <param name="list"></param>
        /// <param name="pid"></param>
        /// <returns></returns>
        public static List<LayuiTreeModel> LayuiTreeList(List<SystemMenuEntity> list, Guid pid)
        {
            var resList = new List<LayuiTreeModel>();
            var list_p = list.Where(a => a.ParentGid == pid).OrderBy(a => a.SortNum).ToList();
            list_p.ForEach(a =>
            {
                LayuiTreeModel model = new LayuiTreeModel()
                {
                    title = a.Name,
                    id = a.Id.ToString()
                };
                model.children = LayuiTreeList(list, a.Id);

                resList.Add(model);
            });

            return resList;
        }

        /// <summary>
        /// 系统菜单组件
        /// </summary>
        /// <param name="list"></param>
        /// <param name="pid"></param>
        /// <returns></returns>
        public static List<SystemLeftMenuModel> SystemMenuList(List<SystemMenuMapper> list, Guid pid)
        {
            var resList = new List<SystemLeftMenuModel>();
            var list_p = list.Where(a => a.ParentGid == pid).OrderBy(a => a.SortNum).ToList();
            list_p.ForEach(a =>
            {
                SystemLeftMenuModel model = new SystemLeftMenuModel()
                {
                    name = a.Name,
                    icon = a.Icon,
                    link = a.Location
                };
                model.children = SystemMenuList(list, a.Id);

                resList.Add(model);
            });

            return resList;
        }

    }
}
