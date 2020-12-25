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
    /// 系统用户管理
    /// </summary>
    [EnableCors("allow_all")]
    [Route("api/[controller]/[action]")]
    public class UserController : ApiBaseController
    {
        protected readonly IUserService _user;
        public UserController(
            IOptions<CmsAppSettingModel> configStr,
            IMapper mapper,
            IUserService user
            ) : base(configStr, mapper)
        {

            _user = user;
        }

        /// <summary>
        /// 查询列表
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="page">页码</param>
        /// <param name="limit">每页条数</param>
        /// <returns></returns>
        [HttpGet]
        public ResultObject GetData(string name, int page, int limit)
        {
            try
            {
                var count = 0;
                var data = _user.GetMapperPageList(name, page, limit, ref count);
                var list = _mapper.Map<List<UserMapper>>(data);

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
                var data = _user.GetEntity(a => a.Id == gid);
                var info = _mapper.Map<UserMapper>(data);

                return Success(info);
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
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ResultObject SaveModel(UserMapper model)
        {
            string optEvent = "添加";
            string errStr = "成功";
            try
            {
                var userInfo = GetLoginUserDetail();
                if (userInfo.LoginUser != "admin" && model.LoginName == "admin")
                {
                    return Error("最高权限账号,不可修改");
                }
                var entity = _mapper.Map<User>(model);
                if (_user.IsExist(a => a.Id != entity.Id && a.LoginName == entity.LoginName))
                {
                    return Error("登陆账号已存在,无法重复添加");
                }
                entity.UpdateTime = DateTime.Now;
                entity.IsDelete = 0;
                if (entity.Id == Guid.Empty)
                {
                    entity.Salt = RandomCode.CreateAuthStr(6, false);
                    _user.Insert(entity);
                }
                else
                {
                    optEvent = "修改";
                    var info = _user.GetEntity(a => a.Id == model.Id);
                    if (info == null)
                    {
                        return Error("信息不存在,修改失败");
                    }
                    entity.RoleId = info.RoleId;
                    entity.Salt = info.Salt;
                    //密码修改
                    if (info.LoginPwd != entity.LoginPwd)
                    {
                        entity.LoginPwd = (entity.LoginPwd + info.Salt).ToMD5();
                    }
                    _user.Update(entity);
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
                SaveUserOperation("UserController", optEvent, $"系统用户({model.LoginName},{model.NickName},结果:{errStr})");
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
                var info = _user.GetEntity(a => a.Id == gid);
                info.IsDelete = 1;
                _user.Update(info);
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
                SaveUserOperation("UserController", "删除", $"系统用户({gid},结果:{errStr})");
            }
        }

    }
}
