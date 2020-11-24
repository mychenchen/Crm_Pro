using AutoMapper;
using Crm.Repository.MapperEntity;
using Crm.Repository.TbEntity;
using Crm.Service.GatewayService;
using Crm.Service.SystemService;
using Crm.WebApp.AuthorizeHelper;
using Crm.WebApp.Infrastructure;
using Crm.WebApp.Models;
using Currency.Common.LogManange;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crm.WebApp.API
{
    /// <summary>
    /// 系统用户角色权限
    /// </summary>
    [EnableCors("allow_all")]
    [Route("api/[controller]/[action]")]
    public class SystemUserRoleController : ApiBaseController
    {
        protected readonly ISystemUserRoleService _sysUser;
        protected readonly IUserService _user;
        protected readonly ISystemMenuService _sysMenu;
        public SystemUserRoleController(
            IOptions<CmsAppSettingModel> configStr,
            IMapper mapper,
            ISystemUserRoleService sysUser,
            IUserService user,
            ISystemMenuService sysMenu
            ) : base(configStr, mapper)
        {
            _sysUser = sysUser;
            _user = user;
            _sysMenu = sysMenu;
        }

        #region 角色菜单

        /// <summary>
        /// 保存或修改
        /// </summary>
        /// <param name="model">菜单</param>
        /// <returns></returns>
        [HttpPost]
        public ResultObject RoleMenuSaveModel(RoleMenuMapper model)
        {
            var optEvent = "添加";
            var errStr = "成功";
            try
            {
                var saveEntity = _mapper.Map<RoleMenuEntity>(model);
                var entity = _sysUser.RoleMenu_GetModel(saveEntity.Id);
                if (entity != null)
                {
                    saveEntity.CreateTime = entity.CreateTime;
                    saveEntity.IsDelete = entity.IsDelete;
                    optEvent = "修改";
                }

                _sysUser.RoleMenu_AddUpdateModel(saveEntity);
                return Success();
            }
            catch (Exception ex)
            {
                errStr = "失败,请查看日志";
                LogHelper.Error(ex.ToString());
                return Error(ex.Message);
            }
            finally
            {
                SaveUserOperation("SystemUserRoleController", optEvent, $"角色菜单,结果:{errStr})");
            }
        }
        /// <summary>
        /// 查询角色菜单
        /// </summary>
        /// <param name="rId">角色ID</param>
        /// <returns></returns>
        [HttpGet]
        public ResultObject GetRoleMenu(Guid rId)
        {
            try
            {
                List<string> list = new List<string>();
                var info = _sysUser.RoleMenu_GetModel(rId);
                if (info != null)
                {
                    list = info.MenuIds.Split(',').ToList();
                }
                return Success(list);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
                return Error(ex.Message);
            }
        }
        
        /// <summary>
        /// 查询角色菜单
        /// </summary>
        /// <param name="rId">角色ID</param>
        /// <returns></returns>
        [HttpGet]
        public ResultObject GetRoleMenuMenuTree(Guid rId)
        {
            try
            {
                var info = _sysUser.RoleMenu_GetModel(rId);
                if (info != null)
                {
                    //获取角色权限对应的菜单ID
                    List<Guid> list = info.MenuIds.Split(',').Select(a => Guid.Parse(a)).ToList();
                    //筛选指定菜单ID的数据
                    var menuList = _sysMenu.GetList().Where(a => list.Contains(a.Id)).ToList();
                    //返回树组件实体模型
                    var reslist = RecursionTree.LayuiTreeList(menuList, Guid.Empty);
                    return Success(reslist);
                }

                return Success(null);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
                return Error(ex.Message);
            }
        }

        #endregion

        #region 系统角色

        /// <summary>
        /// 查询列表
        /// </summary>
        /// <param name="title">名称</param>
        /// <param name="page">页码</param>
        /// <param name="limit">每页条数</param>
        /// <returns></returns>
        [HttpGet]
        public ResultObject SysUserGetData(string title, int page, int limit)
        {
            try
            {
                var count = 0;
                var data = _sysUser.SysUser_GetPageList(title, page, limit, ref count);
                var list = _mapper.Map<List<UserRoleMapper>>(data);

                return SuccessPage(page, limit, count, list);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 保存或修改
        /// </summary>
        /// <param name="model">菜单</param>
        /// <returns></returns>
        [HttpPost]
        public ResultObject SysUserSaveModel(UserRoleMapper model)
        {
            var optEvent = "添加";
            var errStr = "成功";
            try
            {
                var saveEntity = _mapper.Map<UserRoleEntity>(model);
                var entity = _sysUser.SysUser_GetModel(saveEntity.Id);
                if (entity != null)
                {
                    saveEntity.CreateTime = entity.CreateTime;
                    saveEntity.IsDelete = entity.IsDelete;
                    optEvent = "修改";
                }

                _sysUser.SysUser_AddUpdateModel(saveEntity);
                return Success();
            }
            catch (Exception ex)
            {
                errStr = "失败,请查看日志";
                LogHelper.Error(ex.ToString());
                return Error(ex.Message);
            }
            finally
            {
                SaveUserOperation("SystemUserRoleController", optEvent, $"系统权限({model.RoleName},结果:{errStr})");
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="gid"></param>
        /// <returns></returns>
        [HttpGet]
        public ResultObject SysUserDeleteModel(Guid gid)
        {
            var errStr = "成功";
            try
            {
                _sysUser.SysUser_Delete(gid);
                return Success();
            }
            catch (Exception ex)
            {
                errStr = "失败,请查看日志";
                LogHelper.Error(ex.ToString());
                return Error(ex.Message);
            }
            finally
            {
                SaveUserOperation("SystemUserRoleController", "删除", $"系统权限{gid},结果:{errStr}");
            }
        }

        #endregion

        #region 用户绑定角色

        /// <summary>
        /// 用户绑定角色
        /// </summary>
        /// <param name="userId">菜单</param>
        /// <param name="roleId">菜单</param>
        /// <returns></returns>
        [HttpPost]
        public ResultObject UserBindRole(Guid userId, Guid roleId)
        {
            var optEvent = "添加";
            var errStr = "成功";
            try
            {
                var user = _user.GetModel(userId);
                if (user == null)
                {
                    return Error("用户不存在,请刷新页面");
                }
                user.RoleId = roleId;
                _user.AddUpdateModel(user);

                return Success();
            }
            catch (Exception ex)
            {
                errStr = "失败,请查看日志";
                LogHelper.Error(ex.ToString());
                return Error(ex.Message);
            }
            finally
            {
                SaveUserOperation("SystemUserRoleController", optEvent, $"用户绑定角色,结果:{errStr}");
            }
        }


        #endregion

    }
}
