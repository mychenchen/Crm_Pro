using AutoMapper;
using Crm.WebApp.Models;
using Currency.Common;
using Currency.Common.Caching;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;

namespace Crm.WebApp.API
{
    /// <summary>
    /// api基类
    /// </summary>
    public class ApiBaseController : Controller
    {
        ///// <summary>
        ///// 项目数据库配置文件
        ///// </summary>
        //protected readonly DataSettingsModel _configDbStr;

        /// <summary>
        /// 项目配置文件   
        /// </summary>
        protected readonly CmsAppSettingModel _configStr;

        /// <summary>
        /// 实体映射
        /// </summary>
        protected readonly IMapper _mapper;

        protected readonly IStaticCacheManager _cache;

        public ApiBaseController(
            //IOptions<DataSettingsModel> configDbStr,
            IOptions<CmsAppSettingModel> configStr,
            IMapper mapper,
            IStaticCacheManager cache
            )
        {
            //_configDbStr = configDbStr.Value;
            _configStr = configStr.Value;
            _mapper = mapper;
            _cache = cache;
        }

        #region Success

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected ResultObject Success()
        {
            ResultObject res = new ResultObject()
            {
                code = (int)ErrorCode.Success,
                message = "成功",
                resultTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                data = ""
            };
            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        protected ResultObject SuccessNoObj(string msg)
        {
            ResultObject res = new ResultObject()
            {
                code = (int)ErrorCode.Success,
                message = msg,
                resultTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                data = null
            };
            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected ResultObject Success(object obj)
        {
            ResultObject res = new ResultObject()
            {
                code = (int)ErrorCode.Success,
                message = "成功",
                resultTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                data = obj
            };
            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected ResultObject Success(string msg, object obj)
        {
            ResultObject res = new ResultObject()
            {
                code = (int)ErrorCode.Success,
                message = msg,
                resultTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                data = obj
            };
            return res;
        }

        /// <summary>
        /// 分页列表返回
        /// </summary>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        protected ResultObject SuccessPage(int page, int rows, int count, object list)
        {
            ResultObject res = new ResultObject()
            {
                code = (int)ErrorCode.Success,
                message = "查询成功",
                resultTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                data = new
                {
                    page,
                    rows,
                    count,
                    list
                }
            };
            return res;
        }
        #endregion

        #region Error

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected ResultObject Error()
        {
            ResultObject res = new ResultObject()
            {
                code = (int)ErrorCode.Error,
                message = "失败",
                resultTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                data = ""
            };
            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected ResultObject Error(string msg, object obj = null)
        {
            ResultObject res = new ResultObject()
            {
                code = (int)ErrorCode.Error,
                message = msg,
                resultTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                data = obj
            };
            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected ResultObject Error(ErrorCode code, string msg, object obj)
        {
            ResultObject res = new ResultObject()
            {
                code = code.ToInt(),
                message = msg,
                resultTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                data = obj
            };
            return res;
        }
        #endregion
    }
}
