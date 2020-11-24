using AutoMapper;
using Crm.Repository.MapperEntity;
using Crm.Repository.TbEntity;
using Crm.Service.GatewayService;
using Crm.Service.SystemService;
using Crm.WebApp.AuthorizeHelper;
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
        public SystemUserRoleController(
            IOptions<CmsAppSettingModel> configStr,
            IMapper mapper,
            ISystemUserRoleService sysUser
            ) : base(configStr, mapper)
        {
            _sysUser = sysUser;
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
        /// <param name="model">热点轮播</param>
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
                SaveUserOperation("SystemUserRoleController", optEvent, $"tab菜单({model.RoleName},结果:{errStr})");
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
                SaveUserOperation("SystemUserRoleController", "删除", $"tab菜单{gid},结果:{errStr}");
            }
        }

    }
}
