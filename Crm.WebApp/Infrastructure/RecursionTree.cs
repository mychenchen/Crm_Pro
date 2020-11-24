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
        /// 递归树形结构
        /// </summary>
        /// <param name="list"></param>
        /// <param name="pid"></param>
        /// <returns></returns>
        public static List<LayuiTreeModel> LayuiTreeList(List<SystemMenuEntity> list, Guid pid)
        {
            var resList = new List<LayuiTreeModel>();
            var list_p = list.Where(a => a.ParentGid == pid).ToList();
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

    }
}
