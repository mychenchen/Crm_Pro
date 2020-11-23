using AutoMapper;
using Crm.Repository.MapperEntity;
using Crm.Repository.TbEntity;
using Crm.Service.SystemService;
using Crm.WebApp.Models;
using Currency.Common;
using Currency.Common.LogManange;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace Crm.WebApp.API
{
    /// <summary>
    /// 系统菜单管理
    /// </summary>
    [EnableCors("allow_all")]
    [Route("api/[controller]/[action]")]
    public class SystemMenuController : ApiBaseController
    {
        protected readonly ISystemMenuService _user;
        public SystemMenuController(
            IOptions<CmsAppSettingModel> configStr,
            IMapper mapper,
            ISystemMenuService user
            ) : base(configStr, mapper)
        {

            _user = user;
        }

        /// <summary>
        /// 查询列表
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="page">页码</param>
        /// <param name="limit">每页条数</param>
        /// <returns></returns>
        [HttpGet]
        public ResultObject GetData(string name, int page, int limit)
        {
            try
            {
                var count = 0;
                var data = _user.GetPageList(name, page, limit, ref count);
                var list = _mapper.Map<List<SystemMenuMapper>>(data);

                return SuccessPage(page, limit, count, list);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 查询账号详情,根据ID
        /// </summary>
        /// <param name="gid">名称</param>
        /// <returns></returns>
        [HttpGet]
        public ResultObject GetSystemMenuDetailByGid(Guid gid)
        {
            try
            {
                var data = _user.GetModel(gid);
                var info = _mapper.Map<SystemMenuMapper>(data);

                return Success(info);
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
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ResultObject SaveModel(SystemMenuMapper model)
        {
            string optEvent = "添加";
            string errStr = "成功";
            try
            {
                var userInfo = GetLoginUserDetail();
                if (userInfo.LoginUser != "admin")
                {
                    return Error("非最高管理员,无法修改");
                }
                var entity = new SystemMenuEntity();
                if (model.Id == Guid.Empty)
                {
                    entity.CreateTime = DateTime.Now;
                    entity.IsDelete = 0;
                }
                else
                {
                    entity = _user.GetModel(model.Id);
                    if (entity == null)
                    {
                        return Error("信息不存在,修改失败");
                    }
                    optEvent = "修改";
                }

                _user.AddUpdateModel(entity);
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
                SaveUserOperation("SystemMenuController", optEvent, $"系统菜单({model.Name},结果:{errStr})");
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="gid"></param>
        /// <returns></returns>
        [HttpGet]
        public ResultObject DeleteModel(string gid)
        {
            string errStr = "成功";
            try
            {
                _user.Delete(Guid.Parse(gid));
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
                SaveUserOperation("SystemMenuController", "删除", $"系统菜单({gid},结果:{errStr})");
            }
        }

    }
}
