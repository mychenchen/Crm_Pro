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
    /// 用户学习
    /// </summary>
    [Route("api/WebApp/[controller]/[action]")]
    [EnableCors("allow_all")]
    public class UserStudyController : ApiBaseController
    {
        protected readonly IUserStudentService _student;
        protected readonly IProductService _pro;
        protected readonly IUserStudyTaskService _study;

        /// <summary>
        /// 用户学习
        /// </summary>
        /// <param name="configStr"></param>
        /// <param name="mapper"></param>
        /// <param name="student"></param>
        /// <param name="user"></param>
        /// <param name="pro"></param>
        /// <param name="study"></param>
        public UserStudyController(
            IOptions<CmsAppSettingModel> configStr,
            IMapper mapper,
             IUserStudentService student,
             IUserService user,
             IProductService pro,
             IUserStudyTaskService study
            ) : base(configStr, mapper)
        {
            _student = student;
            _pro = pro;
            _study = study;
        }

        /// <summary>
        /// 加入学习任务
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ResultObject> AddStudyTask(Guid proId)
        {
            try
            {
                //当前登陆账号
                var loginUser = GetLoginUserDetail();
                if (loginUser == null)
                {
                    return Error("未登陆,请先登陆再进行操作");
                }
                //所有视频
                var list = await _pro.GetAllVideo(proId);
                //添加学习任务
                List<UserStudyTaskEntity> studyList = new List<UserStudyTaskEntity>();
                list.ForEach(a =>
                {
                    studyList.Add(new UserStudyTaskEntity
                    {
                        Id = Guid.NewGuid(),
                        StudentId = loginUser.Gid,
                        IsStudy = 0,
                        CourseId = proId,
                        CreateTime = DateTime.Now,
                        IsDelete = 0,
                        TimeLong = 0,
                        VideoId = a.Id,
                        VideoName = a.VideoName

                    });
                });

                await _study.InsertArrAsync(studyList);
                return Success("加入成功");
            }
            catch (Exception ex)
            {
                LogHelper.Error("登陆失败 =>>" + ex.ToString());
                return Error(ex.Message);
            }
        }

    }
}
