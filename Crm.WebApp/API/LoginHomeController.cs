using AutoMapper;
using Crm.Repository.TbEntity;
using Crm.Service.SystemService;
using Crm.WebApp.AuthorizeHelper;
using Crm.WebApp.Infrastructure;
using Crm.WebApp.Models;
using Currency.Common;
using Currency.Common.LogManange;
using Currency.Quartz;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crm.WebApp.API
{
    /// <summary>
    /// 用户登陆接口
    /// </summary>
    [Route("api/[controller]/[action]")]
    [EnableCors("allow_all")]
    public class LoginHomeController : ApiBaseController
    {
        protected readonly IUserStudentService _student;
        protected readonly IUserService _user;
        protected readonly IUserLoginLogService _log;
        /// <summary>
        /// 用户登陆接口
        /// </summary>
        /// <param name="configStr"></param>
        /// <param name="mapper"></param>
        /// <param name="student"></param>
        /// <param name="user"></param>
        /// <param name="log"></param>
        public LoginHomeController(
            //IOptions<DataSettingsModel> configDbStr,
            IOptions<CmsAppSettingModel> configStr,
            IMapper mapper,
             IUserStudentService student,
             IUserService user,
             IUserLoginLogService log
            ) : base(configStr, mapper)
        {
            _student = student;
            _user = user;
            _log = log;

        }

        #region 前台用户

        /// <summary>
        /// 用户登陆
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        [HttpPost, NoSign]
        public ResultObject UserLogin(LoginUserPwd login)
        {
            try
            {
                var user = _student.UserLoginModel(login.account);
                if (user == null)
                {
                    return Error("账号不存在");
                }
                string pwd = (login.password + user.Salt).ToMD5();
                if (pwd != user.LoginPwd)
                {
                    return Error("账号或密码错误");
                }

                LoginUserInfo info = new LoginUserInfo()
                {
                    Gid = user.Id.ToString(),
                    LoginUser = user.LoginName,
                    Name = user.NickName,
                    Token = Guid.NewGuid().ToString("N"),
                    LoginTime = DateTime.Now
                };

                _redis.SetStringKey("userLogin_" + info.Token, info, TimeSpan.FromDays(7));
                SaveUserLoginLog(info.Gid, info.LoginUser);

                return Success(info);
            }
            catch (Exception ex)
            {
                LogHelper.Error("登陆失败 =>>" + ex.ToString());
                return Error(ex.Message);
            }
        }

        #endregion

        #region 后台用户

        /// <summary>
        /// 用户登陆
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        [HttpPost]
        [NoSign]
        public ResultObject SystemLogin(LoginUserPwd login)
        {
            try
            {
                var user = _user.UserLoginModel(login.account);
                if (user == null)
                {
                    return Error("账号不存在");
                }
                string pwd = (login.password + user.Salt).ToMD5();
                if (pwd != user.LoginPwd)
                {
                    return Error("账号或密码错误");
                }

                LoginUserInfo info = new LoginUserInfo()
                {
                    Gid = user.Id.ToString(),
                    LoginUser = user.LoginName,
                    Name = user.NickName,
                    Token = Guid.NewGuid().ToString("N"),
                    LoginTime = DateTime.Now,
                    RoleId = user.RoleId
                };

                _redis.SetStringKey("userLogin_" + info.Token, info, TimeSpan.FromDays(7));
                SaveUserLoginLog(info.Gid, info.LoginUser);

                return Success(info);
            }
            catch (Exception ex)
            {
                LogHelper.Error("登陆失败 =>>" + ex.ToString());
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 退出登陆
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet]
        public ResultObject LogOut(string token)
        {
            _redis.KeyDelete("userLogin_" + token);
            return Success();
        }

        #endregion

        /// <summary>
        /// 自动登陆
        /// </summary>
        /// <param name="token">用户token</param>
        /// <returns></returns>
        [HttpGet, NoSign]
        public ResultObject AutomaticLogin(string token)
        {
            try
            {
                var info = _redis.GetKey<LoginUserInfo>("userLogin_" + token);
                if (info == null || info.LoginTime == null)
                {
                    return Error("自动登陆过期,请重新登陆!!!");
                }
                if (Utils.TimeHoursContrast(info.LoginTime, DateTime.Now) > 3)
                {
                    SaveUserLoginLog(info.Gid, info.LoginUser);
                }
                return Success(info);
            }
            catch (Exception ex)
            {
                LogHelper.Error("登陆失败 =>>" + ex.ToString());
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 保存登陆记录
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        protected void SaveUserLoginLog(string userId, string userName)
        {

            UserLoginLog model = new UserLoginLog()
            {
                Id = Guid.NewGuid(),
                Ip = HttpContext.Connection.RemoteIpAddress.ToString(),
                IsDelete = 0,
                CreateTime = DateTime.Now,
                UserId = Guid.Parse(userId),
                UserName = userName
            };

            _log.SaveLog(model);
        }

    }
}
