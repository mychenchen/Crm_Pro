using AutoMapper;
using Crm.Repository.MapperEntity;
using Crm.Repository.TbEntity;
using Crm.Service.CustomerService;
using Crm.Service.ProductManageService;
using Crm.Service.SystemService;
using Crm.WebApp.Models;
using Currency.Common;
using Currency.Common.LogManange;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Crm.WebApp.API.WebApp
{
    /// <summary>
    /// 用户个人设置
    /// </summary>
    [Route("api/WebApp/[controller]/[action]")]
    [EnableCors("allow_all")]
    public class UserSettingController : ApiBaseController
    {
        protected readonly IUserStudentService _student;
        protected readonly IUserStudyTaskService _study;

        /// <summary>
        /// 用户个人设置
        /// </summary>
        /// <param name="configStr"></param>
        /// <param name="mapper"></param>
        /// <param name="student"></param>
        /// <param name="user"></param>
        /// <param name="study"></param>
        public UserSettingController(
            IOptions<CmsAppSettingModel> configStr,
            IMapper mapper,
             IUserStudentService student,
             IUserService user,
             IUserStudyTaskService study
            ) : base(configStr, mapper)
        {
            _student = student;
            _study = study;
        }

        /// <summary>
        /// 个人信息设置
        /// </summary>
        /// <param name="model">用户修改信息</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultObject> SaveUserInfo(UserStudentMapper model)
        {
            try
            {
                var detail = await _student.GetEntityAsync(a => a.Id == model.Id);
                if (detail == null)
                {
                    return Error(ErrorCode.UserNotExist);
                }
                var info = _mapper.Map<UserStudentEntity>(model);
                info.IsVIP = detail.IsVIP;
                info.IsDelete = 0;
                info.LabelId = detail.LabelId;
                info.Salt = detail.Salt;
                info.Telephone = detail.Telephone;
                info.LoginName = detail.LoginName;
                info.LoginPwd = detail.LoginPwd;
                info.CreateTime = detail.CreateTime;

                _student.Update(info);
                return Success("保存成功");
            }
            catch (Exception ex)
            {
                return Error(ex.ToString());
            }
        }

        /// <summary>
        /// 修改登陆密码
        /// </summary>
        /// <param name="uId">用户ID</param>
        /// <param name="newPwd">新密码</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ResultObject> UpdatePwd(Guid uId, string newPwd)
        {
            try
            {
                var detail = await _student.GetEntityAsync(a => a.Id == uId);
                if (detail == null)
                {
                    return Error("未找到当前用户");
                }
                detail.LoginPwd = (newPwd + detail.Salt).ToMD5();

                _student.CommQuery($"UPDATE tb_UserStudent SET LoginPwd = '{detail.LoginPwd}' WHERE ID = '{uId}';");
                return Success("修改成功");
            }
            catch (Exception ex)
            {
                return Error(ex.ToString());
            }
        }

        /// <summary>
        /// 修改绑定手机
        /// </summary>
        /// <param name="uId">用户ID</param>
        /// <param name="telCode">验证码</param>
        /// <param name="telephone">手机号</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ResultObject> UpdateTelephone(Guid uId, string telCode, string telephone)
        {
            try
            {
                var code = _redis.GetStringKey($"code_{telephone}");
                if (code != telCode)
                {
                    return Error(ErrorCode.ValicodeError);
                }
                _redis.KeyDelete($"code_{telephone}_Num");
                _redis.KeyDelete($"code_{telephone}");
                var detail = await _student.GetEntityAsync(a => a.Id == uId);
                if (detail == null)
                {
                    return Error(ErrorCode.UserNotExist);
                }

                _student.CommQuery($"UPDATE tb_UserStudent SET Telephone = '{telephone}' WHERE ID = '{uId}';");
                return Success("修改成功");
            }
            catch (Exception ex)
            {
                return Error(ex.ToString());
            }
        }

    }
}
