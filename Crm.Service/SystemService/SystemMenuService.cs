using Crm.Repository.DB;
using Crm.Repository.TbEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using static Currency.Common.NetCoreDIModuleRegister;

namespace Crm.Service.SystemService
{
    /// <summary>
    /// 系统菜单
    /// UseDI特性用于注入必须加
    /// </summary>
    [UseDI(ServiceLifetime.Scoped, typeof(ISystemMenuService))]
    public class SystemMenuService : ISystemMenuService
    {
        /// <summary>
        /// 数据库
        /// </summary>
        protected readonly MyDbContext _mydb;

        public SystemMenuService(MyDbContext mydb)
        {
            _mydb = mydb;
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="model"></param>
        public List<SystemMenuEntity> GetList()
        {
            return _mydb.SystemMenu.AsNoTracking().Where(a => a.IsDelete == 0).ToList();
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
            var list = _mydb.SystemMenu.Where(a => a.IsDelete == 0 && a.ParentGid == Guid.Empty);

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
        public SystemMenuEntity GetModel(Guid gid)
        {
            return _mydb.SystemMenu.AsNoTracking().FirstOrDefault(a => a.Id == gid);
        }

        /// <summary>
        /// 查询子级菜单
        /// </summary>
        /// <param name="pid"></param>
        public List<SystemMenuEntity> GetListByPid(Guid pid)
        {
            return _mydb.SystemMenu.Where(a => a.IsDelete == 0 && a.ParentGid == pid).OrderBy(a => a.SortNum).ToList();
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="model"></param>
        public void AddUpdateModel(SystemMenuEntity model)
        {
            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();
                _mydb.SystemMenu.Add(model);
            }
            else
            {
                _mydb.SystemMenu.Update(model);
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
                _mydb.SystemMenu.Remove(model);
            else
            {
                model.IsDelete = 1;
                _mydb.SystemMenu.Update(model);
            }
            _mydb.SaveChanges();
            return true;
        }

        /// <summary>
        /// 修改排序
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pid"></param>
        /// <param name="sortNum"></param>
        /// <returns></returns>
        public bool UpdateSortNum(Guid id, Guid pid, int sortNum)
        {
            var model_1 = _mydb.SystemMenu.FirstOrDefault(a => a.Id == id);
            var model_2 = _mydb.SystemMenu.FirstOrDefault(a => a.IsDelete == 0 && a.ParentGid == pid && a.SortNum == sortNum);

            //交换sortNum
            if (model_2 != null)
            {
                model_2.SortNum = model_1.SortNum;
                model_1.SortNum = sortNum;

                _mydb.SaveChanges();
            }

            return true;
        }

        /// <summary>
        /// 验证排序是否存在
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pid"></param>
        /// <param name="sortNum"></param>
        public bool VerifySortNum(Guid id, Guid pid, int sortNum)
        {
            var num = _mydb.SystemMenu.Count(a => a.IsDelete == 0 && a.ParentGid == pid && a.Id != pid && a.SortNum == sortNum);

            if (num > 0)
                return true;
            else
                return false;
        }
    }
}
