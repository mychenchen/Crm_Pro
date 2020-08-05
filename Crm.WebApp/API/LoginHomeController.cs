using AutoMapper;
using Crm.WebApp.Models;
using Crm.Repository.TbEntity;
using Crm.Service.SystemService;
using Currency.Common;
using Currency.Common.Caching;
using Currency.Common.LogManange;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Crm.WebApp.API
{
    /// <summary>
    /// 用户登陆接口
    /// </summary>
    [Route("api/[controller]/[action]")]
    [EnableCors("allow_all")]
    public class LoginHomeController : ApiBaseController
    {
        protected readonly IUserService _user;
        protected readonly IUserLoginLogService _log;
        /// <summary>
        /// 用户登陆接口
        /// </summary>
        /// <param name="configStr"></param>
        /// <param name="mapper"></param>
        /// <param name="cache"></param>
        /// <param name="user"></param>
        /// <param name="log"></param>
        public LoginHomeController(
            //IOptions<DataSettingsModel> configDbStr,
            IOptions<CmsAppSettingModel> configStr,
            IMapper mapper,
            IStaticCacheManager cache,
             IUserService user,
             IUserLoginLogService log
            ) : base(configStr, mapper, cache)
        {
            _user = user;
            _log = log;
        }

        /// <summary>
        /// 用户登陆
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        [HttpPost]
        public ResultObject Login(LoginUserPwd login)
        {
            try
            {
                var user = _user.UserLoginModel(login.account);
                if (user == null)
                {
                    return Error("账号不存在");
                }
                string pwd = (login.password + user.Salt).ToMD5(); ;
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

                _cache.Set("userLogin_" + info.Token, info, 24 * 60);
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
        /// 自动登陆
        /// </summary>
        /// <param name="token">用户token</param>
        /// <returns></returns>
        [HttpGet]
        public ResultObject AutomaticLogin(string token)
        {
            try
            {
                var info = _cache.Get<LoginUserInfo>("userLogin_" + token);
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
        /// 退出登陆
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet]
        public ResultObject LogOut(string token)
        {
            _cache.Remove("userLogin_" + token);
            return Success();
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
