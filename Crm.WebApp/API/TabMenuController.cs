using AutoMapper;
using Crm.Repository.MapperEntity;
using Crm.Repository.TbEntity;
using Crm.Service.GatewayService;
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
    /// tab菜单
    /// </summary>
    [EnableCors("allow_all")]
    [Route("api/[controller]/[action]")]
    public class TabMenuController : ApiBaseController
    {
        protected readonly ITabMenuService _tabMenu;
        public TabMenuController(
            IOptions<CmsAppSettingModel> configStr,
            IMapper mapper,
            ITabMenuService tabMenu
            ) : base(configStr, mapper)
        {
            _tabMenu = tabMenu;
        }

        #region 后台服务接口

        /// <summary>
        /// 查询列表
        /// </summary>
        /// <param name="title">名称</param>
        /// <param name="page">页码</param>
        /// <param name="limit">每页条数</param>
        /// <returns></returns>
        [HttpGet]
        public ResultObject GetData(string title, int page, int limit)
        {
            try
            {
                var count = 0;
                var data = _tabMenu.GetPageList(title, page, limit, ref count);
                var list = _mapper.Map<List<TabMenuMapper>>(data);

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
                var data = _tabMenu.SelectWhere(a => a.IsDelete == 0 && a.ParentGid == pid).OrderBy(a => a.SortNum).ToList();
                var list = _mapper.Map<List<TabMenuMapper>>(data);

                return Success(list);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 查询详情,根据ID
        /// </summary>
        /// <param name="gid">名称</param>
        /// <returns></returns>
        [HttpGet]
        public ResultObject GetDetailByGid(Guid gid)
        {
            try
            {
                var data = _tabMenu.GetEntity(a => a.Id == gid);
                var info = _mapper.Map<TabMenuMapper>(data);

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
        /// <param name="model">热点轮播</param>
        /// <returns></returns>
        [HttpPost]
        public ResultObject SaveModel(TabMenuMapper model)
        {
            var optEvent = "添加";
            var errStr = "成功";
            try
            {
                var saveEntity = _mapper.Map<TabMenuEntity>(model);
                var entity = _tabMenu.GetEntity(a => a.Id == saveEntity.Id);
                if (entity != null)
                {
                    saveEntity.CreateTime = entity.CreateTime;
                    saveEntity.IsDelete = entity.IsDelete;
                    optEvent = "修改";
                    _tabMenu.Update(saveEntity);
                }
                else
                {
                    saveEntity.Id = Guid.NewGuid();
                    saveEntity.IsDelete = 0;
                    saveEntity.CreateTime = DateTime.Now;
                    _tabMenu.Insert(saveEntity);
                }

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
                SaveUserOperation("TabMenuController", optEvent, $"tab菜单({model.Name},结果:{errStr})");
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="gid"></param>
        /// <returns></returns>
        [HttpGet]
        public ResultObject DeleteModel(Guid gid)
        {
            var errStr = "成功";
            try
            {
                var info = _tabMenu.GetEntity(a => a.Id == gid);
                info.IsDelete = 1;
                _tabMenu.Delete(info);
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
                SaveUserOperation("TabMenuController", "删除", $"tab菜单{gid},结果:{errStr}");
            }
        }

        #endregion

        #region 前台调用接口

        /// <summary>
        /// 查询所有tab菜单
        /// </summary>
        /// <returns></returns>
        [HttpGet, NoSign]
        public ResultObject GetAllData()
        {
            try
            {
                var data = _tabMenu.Select();
                var list = _mapper.Map<List<TabMenuMapper>>(data);

                return Success(list);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
                return Error(ex.Message);
            }
        }

        #endregion
    }
}
