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
    /// 用户标签
    /// UseDI特性用于注入必须加
    /// </summary>
    [UseDI(ServiceLifetime.Scoped, typeof(ISystemUserRoleService))]
    public class SystemUserRoleService : ISystemUserRoleService
    {
        /// <summary>
        /// 数据库
        /// </summary>
        protected readonly MyDbContext _mydb;

        public SystemUserRoleService(MyDbContext mydb)
        {
            _mydb = mydb;
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="model"></param>
        public List<UserRoleEntity> SysUser_GetList()
        {
            return _mydb.UserRole.ToList();
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="page"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<UserRoleEntity> SysUser_GetPageList(string name, int page, int rows, ref int count)
        {
            var list = _mydb.UserRole.Where(a => a.IsDelete == 0);

            if (!string.IsNullOrEmpty(name))
            {
                list = list.Where(a => a.RoleName.Contains(name));
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
        public UserRoleEntity SysUser_GetModel(Guid gid)
        {
            return _mydb.UserRole.AsNoTracking().FirstOrDefault(a => a.Id == gid);
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="model"></param>
        public void SysUser_AddUpdateModel(UserRoleEntity model)
        {
            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();
                model.CreateTime = DateTime.Now;
                _mydb.UserRole.Add(model);
            }
            else
            {
                _mydb.UserRole.Update(model);
            }
            _mydb.SaveChanges();
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="gid"></param>
        /// <param name="isDelete">true 真删除 false 假删除</param>
        public bool SysUser_Delete(Guid gid, bool isDelete = false)
        {
            var model = SysUser_GetModel(gid);
            if (model == null)
                return false;
            if (isDelete)
                _mydb.UserRole.Remove(model);
            else
            {
                model.IsDelete = 1;
                _mydb.UserRole.Update(model);
            }
            _mydb.SaveChanges();
            return true;
        }

        /// <summary>
        /// 查询角色菜单
        /// </summary>
        /// <param name="rid"></param>
        public RoleMenuEntity RoleMenu_GetModel(Guid rid)
        {
            return _mydb.RoleMenu.AsNoTracking().FirstOrDefault(a => a.RoleId == rid);
        }
    }
}
