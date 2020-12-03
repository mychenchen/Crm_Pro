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
                var data = _user.GetModel(gid);
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
                var entity = new User();
                if (model.Id == Guid.Empty)
                {
                    entity.Salt = RandomCode.CreateAuthStr(6, false);
                    entity.CreateTime = DateTime.Now;
                    entity.UpdateTime = DateTime.Now;
                    entity.IsDelete = 0;
                }
                else
                {
                    entity = _user.GetModel(model.Id);
                    if (entity == null)
                    {
                        return Error("信息不存在,修改失败");
                    }
                    entity.UpdateTime = DateTime.Now;
                    optEvent = "修改";
                }
                entity.LabelId = model.LabelId;
                entity.LoginName = model.LoginName;
                entity.NickName = model.NickName;
                entity.Sex = model.Sex;
                entity.HeadImg = model.HeadImg;
                entity.MyIntroduce = model.MyIntroduce;
                //密码修改
                if (model.LoginPwd != entity.LoginPwd)
                {
                    entity.LoginPwd = (model.LoginPwd + entity.Salt).ToMD5();
                }
                if (_user.VerifyLoginName(entity.Id, entity.LoginName))
                {
                    return Error("登陆账号已存在,无法重复添加");
                }

                _user.AddUpdateModel(entity);
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
        public ResultObject DeleteModel(string gid)
        {
            string errStr = "成功";
            try
            {
                _user.Delete(Guid.Parse(gid));
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

        ///// <summary>
        ///// 发送Mq消息
        ///// </summary>
        ///// <param name="quName"></param>
        ///// <param name="messageStr"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public ResultObject SendMq(string quName, string messageStr)
        //{
        //    try
        //    {
        //        HotNews ht = new HotNews
        //        {
        //            Id = Guid.NewGuid(),
        //            ShowTime = DateTime.Now,
        //            CoverUrl = "#",
        //            CreateTime = DateTime.Now,
        //            IsDelete = 0,
        //            NewsContent = messageStr,
        //            Title = "测试内容"
        //        };


        //        _mq.SendByName(quName, ht.ToJson());

        //        return Success("发送成功");
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.Error(ex.ToString());
        //        return Error(ex.Message);
        //    }
        //}
    }
}
