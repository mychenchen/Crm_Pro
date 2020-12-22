using AutoMapper;
using Crm.Repository.MapperEntity;
using Crm.Repository.TbEntity;
using Crm.Service.SystemService;
using Crm.WebApp.AuthorizeHelper;
using Crm.WebApp.Models;
using Currency.Common;
using Currency.Common.LogManange;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Crm.WebApp.API.WebApp
{
    /// <summary>
    /// 用户登陆接口
    /// </summary>
    [Route("api/WebApp/[controller]/[action]")]
    [EnableCors("allow_all")]
    public class RegisterLoginController : ApiBaseController
    {
        protected readonly IUserStudentService _student;
        protected readonly IUserLoginLogService _log;

        /// <summary>
        /// 用户注册登陆接口
        /// </summary>
        /// <param name="configStr"></param>
        /// <param name="mapper"></param>
        /// <param name="student"></param>
        /// <param name="user"></param>
        /// <param name="log"></param>
        public RegisterLoginController(
            //IOptions<DataSettingsModel> configDbStr,
            IOptions<CmsAppSettingModel> configStr,
            IMapper mapper,
             IUserStudentService student,
             IUserService user,
             IUserLoginLogService log
            ) : base(configStr, mapper)
        {
            _student = student;
            _log = log;

        }

        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, NoSign]
        public async Task<ResultObject> RegisterUser(UserStudentMapper model)
        {
            try
            {
                var sqlModel = await _student.GetEntityAsync(a => a.LoginName == model.LoginName);
                if (sqlModel != null)
                {
                    return Error(ErrorCode.UserExist, "账号已存在");
                }
                var entity = _mapper.Map<UserStudentEntity>(model);
                entity.Id = Guid.NewGuid();
                entity.NickName = "yk_" + RandomCode.CreateAuthStr(9, false);
                entity.Salt = RandomCode.CreateAuthStr(6, false);
                entity.LoginPwd = (model.LoginPwd + entity.Salt).ToMD5();
                //固定游客标签ID
                entity.LabelId = Guid.Parse("6ec15324-1865-432c-b300-30168e61aef5");
                entity.CreateTime = DateTime.Now;
                entity.IsDelete = 0;
                entity.IsVIP = 0;
                await _student.InsertAsync(entity);

                return Success("注册成功,即将跳转登陆界面");
            }
            catch (Exception ex)
            {
                LogHelper.Error("登陆失败 =>>" + ex.ToString());
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 用户登陆
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        [HttpPost, NoSign]
        public async Task<ResultObject> UserLogin(LoginUserPwd login)
        {
            try
            {
                var user = await _student.GetEntityAsync(a => a.LoginName == login.account);
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
                    HeadImg = user.HeadImg
                };

                _redis.SetStringKey("userLogin_" + info.Token, info, TimeSpan.FromDays(7));
                await SaveUserLoginLog(info.Gid, info.LoginUser);

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
        [HttpGet, NoSign]
        public async Task<ResultObject> AutomaticLogin(string token)
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
                    await SaveUserLoginLog(info.Gid, info.LoginUser);
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
        protected async Task SaveUserLoginLog(string userId, string userName)
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

            await _log.InsertAsync(model);
        }

    }
}
