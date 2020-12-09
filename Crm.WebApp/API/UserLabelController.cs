using AutoMapper;
using Crm.Repository.TbEntity;
using Crm.Service.SystemService;
using Crm.WebApp.Models;
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
        protected readonly IUserService _user;
        public UserLabelController(
            IOptions<CmsAppSettingModel> configStr,
            IMapper mapper,
            IUserLabelService label,
            IUserService user
            ) : base(configStr, mapper)
        {
            _label = label;
            _user = user;
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
                var data = _label.GetEntity(a => a.Id == gid);

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
                var entity = _label.GetEntity(a => a.Id == model.Id);
                if (entity != null)
                {
                    model.CreateTime = entity.CreateTime;
                    model.IsDelete = 0;
                    optEvent = "修改";
                    _label.Update(model);
                }
                else
                {
                    model.Id = Guid.NewGuid();
                    model.CreateTime = DateTime.Now;
                    _label.Insert(model);
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
                var user = _user.GetEntity(a => a.LabelId == gid);
                if (user != null)
                {
                    return Error("标签已使用,请先删除使用");
                }
                var info = _label.GetEntity(a => a.Id == gid);
                _label.Delete(info);
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
