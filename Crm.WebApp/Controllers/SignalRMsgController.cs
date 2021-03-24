using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crm.WebApp.AuthorizeHelper;
using Crm.WebApp.Infrastructure;
using Currency.Common;
using Currency.Common.Redis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Crm.WebApp.Controllers
{

    public class SignalRMsgController : Controller
    {
        protected readonly RedisCommon _redis;

        public SignalRMsgController()
        {
            _redis = RedisHelperNetCore.Default;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet, NoSign]
        public IActionResult Index()
        {
            _redis.SetPublish("txt_demo", "你好");


            //_redis.GetSubscribe("txt_demo", (res, rest) =>
            //{
            //    Console.WriteLine(res);
            //});

            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet, NoSign]
        public string GetDemo()
        {
            List<string> list = new List<string>();
            list.Add("zhangsan");
            list.Add("zhangsan");
            list.Add("zhangsan");
            var ss = list.ToJson();
            return ss;
        }


    }
}