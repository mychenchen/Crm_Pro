using Crm.Repository.DB;
using Crm.Repository.TbEntity;
using Crm.Service.BaseHelper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crm.Service.SystemService
{
    /// <summary>
    /// 系统菜单
    /// </summary>
    public class SystemMenuService : BaseServiceRepository<SystemMenuEntity>, ISystemMenuService
    {

        public SystemMenuService(MyDbContext mydb) : base(mydb)
        {
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="page"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<SystemMenuEntity> GetPageList(string name, int page, int rows, ref int count)
        {
            var list = myDbContext.SystemMenu.Where(a => a.IsDelete == 0 && a.ParentGid == Guid.Empty);

            if (!string.IsNullOrEmpty(name))
            {
                list = list.Where(a => a.Name.Contains(name));
            }
            var data = list.OrderBy(a => a.SortNum)
                .Skip((page - 1) * rows).Take(rows).ToList();
            count = list.Count();
            return data;
        }


        ///// <summary>
        ///// 查询子级菜单
        ///// </summary>
        ///// <param name="pid"></param>
        //public List<SystemMenuEntity> GetListByPid(Guid pid)
        //{
        //    return _mydb.SystemMenu.Where(a => a.IsDelete == 0 && a.ParentGid == pid).OrderBy(a => a.SortNum).ToList();
        //}

        ///// <summary>
        ///// 修改排序
        ///// </summary>
        ///// <param name="id"></param>
        ///// <param name="pid"></param>
        ///// <param name="sortNum"></param>
        ///// <returns></returns>
        //public bool UpdateSortNum(Guid id, Guid pid, int sortNum)
        //{
        //    var model_1 = _mydb.SystemMenu.FirstOrDefault(a => a.Id == id);
        //    var model_2 = _mydb.SystemMenu.FirstOrDefault(a => a.IsDelete == 0 && a.ParentGid == pid && a.SortNum == sortNum);

        //    //交换sortNum
        //    if (model_2 != null)
        //    {
        //        model_2.SortNum = model_1.SortNum;
        //        model_1.SortNum = sortNum;

        //        _mydb.SaveChanges();
        //    }

        //    return true;
        //}

        ///// <summary>
        ///// 验证排序是否存在
        ///// </summary>
        ///// <param name="id"></param>
        ///// <param name="pid"></param>
        ///// <param name="sortNum"></param>
        //public bool VerifySortNum(Guid id, Guid pid, int sortNum)
        //{
        //    var num = _mydb.SystemMenu.Count(a => a.IsDelete == 0 && a.ParentGid == pid && a.Id != id && a.SortNum == sortNum);

        //    if (num > 0)
        //        return true;
        //    else
        //        return false;
        //}
    }
}
