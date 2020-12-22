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

namespace Crm.WebApp.API.WebApp
{
    /// <summary>
    /// 用户个人设置
    /// </summary>
    [Route("api/WebApp/[controller]/[action]")]
    [EnableCors("allow_all")]
    public class UserStettingController : ApiBaseController
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
        public UserStettingController(
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
        public ResultObject RegisterUser(UserStudentMapper model)
        {
            try
            {
                var sqlModel = _student.GetEntity(a => a.LoginName == model.LoginName);
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
                _student.Insert(entity);

                return Success("注册成功,即将跳转登陆界面");
            }
            catch (Exception ex)
            {
                LogHelper.Error("登陆失败 =>>" + ex.ToString());
                return Error(ex.Message);
            }
        }


    }
}
