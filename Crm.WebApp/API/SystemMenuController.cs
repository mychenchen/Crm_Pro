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
        protected readonly ISystemMenuService _menu;
        public SystemMenuController(
            IOptions<CmsAppSettingModel> configStr,
            IMapper mapper,
            ISystemMenuService menu
            ) : base(configStr, mapper)
        {

            _menu = menu;
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
                var data = _menu.GetPageList(name, page, limit, ref count);
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
        /// 查询列表
        /// </summary>
        /// <param name="pid">父级ID</param>
        /// <returns></returns>
        [HttpGet]
        public ResultObject GetDataByPid(Guid pid)
        {
            try
            {
                var data = _menu.GetListByPid(pid);
                var list = _mapper.Map<List<SystemMenuMapper>>(data);

                return Success(list);
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
                var data = _menu.GetModel(gid);
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

                //判断排序是否重复
                if (_menu.VerifySortNum(model.Id, model.ParentGid, model.SortNum))
                {
                    return Error("排序已存在,请更换");
                }

                var saveEntity = _mapper.Map<SystemMenuEntity>(model);
                var entity = _menu.GetModel(saveEntity.Id);
                if (entity != null)
                {
                    saveEntity.CreateTime = entity.CreateTime;
                    saveEntity.IsDelete = entity.IsDelete;
                    optEvent = "修改";
                }
                else
                {
                    saveEntity.CreateTime = DateTime.Now; ;
                }

                _menu.AddUpdateModel(saveEntity);
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
                _menu.Delete(Guid.Parse(gid));
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

        /// <summary>
        /// 更换排序
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pid"></param>
        /// <param name="sortNum"></param>
        /// <returns></returns>
        [HttpGet]
        public ResultObject UpdateSortNum(Guid id, Guid pid, int sortNum)
        {
            string errStr = "成功";
            try
            {
                var flag = _menu.UpdateSortNum(id, pid, sortNum);
                if (flag)
                    return Success();
                else
                    return Error("修改失败");
            }
            catch (Exception ex)
            {
                errStr = "失败,请查看日志";
                LogHelper.Error(ex.ToString());
                return Error(ex.Message);
            }
            finally
            {
                SaveUserOperation("SystemMenuController", "更新", $"更新菜单,结果:{errStr})");
            }
        }
    }
}
