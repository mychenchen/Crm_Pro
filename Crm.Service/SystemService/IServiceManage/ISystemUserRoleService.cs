using Crm.Repository.TbEntity;
using System;
using System.Collections.Generic;

namespace Crm.Service.SystemService
{
    /// <summary>
    /// 系统用户角色
    /// </summary>
    public interface ISystemUserRoleService
    {
        /// <summary>
        /// 查询列表
        /// </summary>
        /// <returns></returns>
        List<UserRoleEntity> SysUser_GetList();

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="page"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<UserRoleEntity> SysUser_GetPageList(string name, int page, int rows, ref int count);

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="model"></param>
        UserRoleEntity SysUser_GetModel(Guid gid);

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="model"></param>
        void SysUser_AddUpdateModel(UserRoleEntity model);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="gid"></param>
        /// <param name="isDelete">true 真删除 false 假删除</param>
        bool SysUser_Delete(Guid gid, bool isDelete = false);

        /// <summary>
        /// 查询角色菜单
        /// </summary>
        /// <param name="model"></param>
        RoleMenuEntity RoleMenu_GetModel(Guid rid);

    }
}
