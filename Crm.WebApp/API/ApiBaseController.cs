﻿using AutoMapper;
using Crm.Repository.TbEntity;
using Crm.Service.SystemService;
using Crm.WebApp.Infrastructure;
using Crm.WebApp.Models;
using Currency.Common;
using Currency.Common.Redis;
using Currency.Common.SystemRegister;
using Microsoft.AspNetCore.Http;
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

        protected readonly RedisCommon _redis;

        protected readonly IOperationLogService _opt;

        public ApiBaseController(
            //IOptions<DataSettingsModel> configDbStr,
            IOptions<CmsAppSettingModel> configStr,
            IMapper mapper
            )
        {
            //_configDbStr = configDbStr.Value;
            _configStr = configStr.Value;
            _mapper = mapper;
            _redis = RedisHelperNetCore.Default;
            _opt = DI.GetService<IOperationLogService>();
        }

        /// <summary>
        /// 获取当前登录用户信息
        /// </summary>
        /// <returns></returns>
        //[TimeAspect] AOP切面
        protected LoginUserInfo GetLoginUserDetail()
        {
            return _redis.GetKey<LoginUserInfo>("userLogin_" + Request.Headers["ToKenStr"]);
        }

        /// <summary>
        /// 记录用户操作
        /// </summary>
        /// <param name="controllerStr">模块</param>
        /// <param name="optEvent">操作事件</param>
        /// <param name="txt">操作内容</param>
        protected async void SaveUserOperation(string controllerStr, string optEvent, string txt)
        {
            var userLogin = GetLoginUserDetail();
            if (userLogin != null)
            {
                OperationLogEntity optModel = new OperationLogEntity
                {
                    CreateTime = DateTime.Now,
                    OpentionControllerStr = controllerStr,
                    IsDelete = 0,
                    OperationEvent = optEvent,
                    OperationTime = DateTime.Now,
                    OperationUser = $"昵称:{userLogin.Name},账号:{userLogin.LoginUser}",
                    OpentionContext = txt
                };
                await _opt.InsertAsync(optModel);
            }
        }

        /// <summary>
        /// 设置Session
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        protected void SetSession(string key, string value)
        {
            HttpContext.Session.SetString(key, value);
        }

        /// <summary>
        /// 获取Session
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>返回对应的值</returns>
        protected string GetSession(string key)
        {
            var value = HttpContext.Session.GetString(key);
            if (string.IsNullOrEmpty(value))
                value = string.Empty;
            return value;
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
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected ResultObject Success(int code, string msg, object obj)
        {
            ResultObject res = new ResultObject()
            {
                code = code,
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
        protected ResultObject Error(string msg = null, object obj = null)
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
        protected ResultObject Error(ErrorCode code, string msg = null, object obj = null)
        {
            if (string.IsNullOrEmpty(msg))
            {
                msg = code.GetDescription();
            }
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
