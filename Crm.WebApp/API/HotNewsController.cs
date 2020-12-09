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

namespace Crm.WebApp.API
{
    /// <summary>
    /// 新闻管理
    /// </summary>
    [EnableCors("allow_all")]
    [Route("api/[controller]/[action]")]
    public class HotNewsController : ApiBaseController
    {
        protected readonly IHotNewsService _hotNews;
        public HotNewsController(
            IOptions<CmsAppSettingModel> configStr,
            IMapper mapper,
            IHotNewsService hotNews
            ) : base(configStr, mapper)
        {
            _hotNews = hotNews;
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
                var data = _hotNews.GetPageList(title, page, limit, ref count);
                var list = _mapper.Map<List<HotNewsMapper>>(data);

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
                var data = _hotNews.GetEntity(a => a.Id == gid);
                var info = _mapper.Map<HotNewsMapper>(data);

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
        public ResultObject SaveModel(HotNewsMapper model)
        {
            var optEvent = "添加";
            var errStr = "成功";
            try
            {
                var saveEntity = _mapper.Map<HotNewsEntity>(model);
                var entity = _hotNews.GetEntity(a => a.Id == saveEntity.Id);
                if (entity != null)
                {
                    saveEntity.CreateTime = entity.CreateTime;
                    saveEntity.IsDelete = entity.IsDelete;
                    optEvent = "修改";
                    _hotNews.Update(saveEntity);
                }
                else
                {
                    saveEntity.CreateTime = DateTime.Now;
                    saveEntity.IsDelete = 0;
                    _hotNews.Insert(saveEntity);
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
                SaveUserOperation("HotNewsController", optEvent, $"添加新闻,结果:{errStr})");
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
                var info = _hotNews.GetEntity(a => a.Id == gid);
                info.IsDelete = 1;
                _hotNews.Update(info);
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
                SaveUserOperation("HotNewsController", "删除", $"删除新闻,结果:{errStr}");
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
                var data = _hotNews.Select();
                var list = _mapper.Map<List<HotNewsMapper>>(data);

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
