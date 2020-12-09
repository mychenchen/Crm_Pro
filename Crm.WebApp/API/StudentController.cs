using AutoMapper;
using Crm.Repository.MapperEntity;
using Crm.Repository.TbEntity;
using Crm.Service.SystemService;
using Crm.WebApp.Models;
using Currency.Common;
using Currency.Common.LogManange;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace Crm.WebApp.API
{
    /// <summary>
    /// 学生用户管理
    /// </summary>
    [EnableCors("allow_all")]
    [Route("api/[controller]/[action]")]
    public class StudentController : ApiBaseController
    {
        protected readonly IUserStudentService _student;
        public StudentController(
            IOptions<CmsAppSettingModel> configStr,
            IMapper mapper,
            IUserStudentService student
            ) : base(configStr, mapper)
        {

            _student = student;
        }

        /// <summary>
        /// 查询列表
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="isVip">是否vip(默认-1)</param>
        /// <param name="page">页码</param>
        /// <param name="limit">每页条数</param>
        /// <returns></returns>
        [HttpGet]
        public ResultObject GetData(string name, int isVip, int page, int limit)
        {
            try
            {
                var count = 0;
                var data = _student.GetMapperPageList(name, isVip, page, limit, ref count);
                var list = _mapper.Map<List<UserStudentMapper>>(data);

                return SuccessPage(page, limit, count, list);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 查询账号详情,根据ID
        /// </summary>
        /// <param name="gid">名称</param>
        /// <returns></returns>
        [HttpGet]
        public ResultObject GetUserDetailByGid(Guid gid)
        {
            try
            {
                var data = _student.GetEntity(a => a.Id == gid);
                var info = _mapper.Map<UserStudentMapper>(data);

                return Success(info);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 修改信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ResultObject UpdateModel(UserStudentMapper model)
        {
            string optEvent = "修改";
            string errStr = "成功";
            var userInfo = GetLoginUserDetail();
            try
            {
                var dbInfo = _student.GetEntity(a => a.Id == model.Id);
                if (dbInfo == null)
                {
                    return Error("信息不存在");
                }
                var entity = _mapper.Map<UserStudentEntity>(model);
                entity.Salt = dbInfo.Salt;
                entity.CreateTime = dbInfo.CreateTime;
                entity.IsDelete = 0;
                if (entity.LoginPwd != dbInfo.LoginPwd)
                {
                    entity.LoginPwd = (entity.LoginPwd + entity.Salt).ToMD5();
                }
                _student.Update(entity);

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
                SaveUserOperation("UserController", optEvent, $"后台用户{userInfo.Name})修改学生({model.NickName})信息,结果:{errStr}");
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
            string errStr = "成功";
            try
            {
                var info = _student.GetEntity(a => a.Id == gid);
                info.IsDelete = 1;
                _student.Update(info);
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
                SaveUserOperation("UserController", "删除", $"用户学生,结果:{errStr}");
            }
        }

    }
}
