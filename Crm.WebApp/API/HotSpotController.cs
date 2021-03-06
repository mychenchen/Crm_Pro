﻿using AutoMapper;
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

namespace Crm.WebApp.API
{
    /// <summary>
    /// 热点轮播
    /// </summary>
    [EnableCors("allow_all")]
    [Route("api/[controller]/[action]")]
    public class HotSpotController : ApiBaseController
    {
        protected readonly IHotSpotService _hotSpot;
        public HotSpotController(
            IOptions<CmsAppSettingModel> configStr,
            IMapper mapper,
            IHotSpotService hotSpot
            ) : base(configStr, mapper)
        {
            _hotSpot = hotSpot;
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
                var data = _hotSpot.GetPageList(title, page, limit, ref count);
                var list = _mapper.Map<List<HotSpotMapper>>(data);

                return SuccessPage(page, limit, count, list);
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
                var data = _hotSpot.GetEntity(a => a.Id == gid);
                var info = _mapper.Map<HotSpotMapper>(data);

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
        public ResultObject SaveModel(HotSpotMapper model)
        {
            var optEvent = "添加";
            var errStr = "成功";
            try
            {
                var saveEntity = _mapper.Map<HotSpotEntity>(model);
                var entity = _hotSpot.GetEntity(a => a.Id == saveEntity.Id);
                if (entity != null)
                {
                    saveEntity.CreateTime = entity.CreateTime;
                    saveEntity.IsDelete = entity.IsDelete;
                    if (string.IsNullOrEmpty(saveEntity.ImgPath))
                    {
                        saveEntity.ImgPath = entity.ImgPath;
                    }
                    optEvent = "修改";
                    _hotSpot.Update(saveEntity);
                }
                else
                {
                    saveEntity.CreateTime = DateTime.Now;
                    saveEntity.IsDelete = 0;
                    saveEntity.Id = Guid.NewGuid();
                    _hotSpot.Insert(saveEntity);
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
                SaveUserOperation("HotSpotController", optEvent, $"热点轮播图({model.ImgTitle},结果:{errStr})");
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
                var info = _hotSpot.GetEntity(a => a.Id == gid);
                info.IsDelete = 1;
                _hotSpot.Update(info);
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
                SaveUserOperation("HotSpotController", "删除", $"热点轮播图{gid},结果:{errStr}");
            }
        }

        #endregion

        #region 前端接口

        /// <summary>
        /// 查询列表
        /// </summary>
        /// <param name="page">页码</param>
        /// <param name="limit">每页条数</param>
        /// <returns></returns>
        [HttpGet]
        [NoSign]
        public ResultObject GetAllData(int page, int limit)
        {
            try
            {
                var count = 0;
                var data = _hotSpot.GetPageList("", page, limit, ref count);
                var list = _mapper.Map<List<HotSpotMapper>>(data);

                return SuccessPage(page, limit, count, list);
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
