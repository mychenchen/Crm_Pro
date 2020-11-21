using AutoMapper;
using Crm.Repository.TbEntity;
using Crm.Service.SystemService;
using Crm.WebApp.Models;
using Currency.Common.Caching;
using Currency.Common.LogManange;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;

namespace Crm.WebApp.API
{
    /// <summary>
    /// 用户标签
    /// </summary>
    [EnableCors("allow_all")]
    [Route("api/[controller]/[action]")]
    public class UserLabelController : ApiBaseController
    {
        protected readonly IUserLabelService _label;
        public UserLabelController(
            IOptions<CmsAppSettingModel> configStr,
            IMapper mapper,
            IStaticCacheManager cache,
            IUserLabelService label
            ) : base(configStr, mapper, cache)
        {
            _label = label;
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
                var data = _label.GetPageList(title, page, limit, ref count);

                return SuccessPage(page, limit, count, data);
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
                var data = _label.GetModel(gid);

                return Success(data);
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
        public ResultObject SaveModel(UserLabel model)
        {
            var optEvent = "添加";
            var errStr = "成功";
            try
            {
                var entity = _label.GetModel(model.Id);
                if (entity != null)
                {
                    model.CreateTime = entity.CreateTime;
                    model.IsDelete = 0;
                    optEvent = "修改";
                }

                _label.AddUpdateModel(model);
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
                SaveUserOperation("UserLabelController", optEvent, $"用户标签({model.LabelName},结果:{errStr})");
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
                _label.Delete(gid, true);
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
                SaveUserOperation("UserLabelController", "删除", $"用户标签{gid},结果:{errStr}");
            }
        }

        #endregion

    }
}
