using AutoMapper;
using Crm.WebApp.AuthorizeHelper;
using Crm.WebApp.Models;
using Currency.Quartz;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace Crm.WebApp.API
{
    /// <summary>
    /// 测试接口
    /// </summary>
    [Route("api/[controller]/[action]")]
    [EnableCors("allow_all")]
    public class CeShiDemoController : ApiBaseController
    {
        /// <summary>
        /// 用户登陆接口
        /// </summary>
        /// <param name="configStr"></param>
        /// <param name="mapper"></param>
        public CeShiDemoController(
            IOptions<CmsAppSettingModel> configStr,
            IMapper mapper
            ) : base(configStr, mapper)
        {

        }

        /// <summary>
        ///  测试demo
        /// </summary>
        /// <returns></returns>
        [HttpGet, NoSign]
        public ResultObject Demo()
        {
            try
            {
                return Success("ok");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        /// <summary>
        ///  Quartz任务,测试用的demo
        /// </summary>
        /// <returns></returns>
        [HttpGet, NoSign]
        public ResultObject QuartzDemo()
        {
            try
            {
                var key = Guid.NewGuid().ToString("N");
                _redis.SetStringKey<int>("online_" + key, 1,TimeSpan.FromMinutes(5));
                QuartzService.StartJob<UsrOnLineJob>(key, 10,
                    new Dictionary<string, string>
                    {
                        {"userId",key }
                    });

                return Success("ok");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

    }
}
