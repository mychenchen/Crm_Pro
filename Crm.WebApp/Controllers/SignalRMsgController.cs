using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crm.WebApp.AuthorizeHelper;
using Crm.WebApp.Infrastructure;
using Currency.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Crm.WebApp.Controllers
{

    public class SignalRMsgController : Controller
    {
        //private readonly IHubContext<ChatHub> _chatHub;

        //public SignalRMsgController(IHubContext<ChatHub> chatHub)
        //{
        //    _chatHub = chatHub;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet,NoSign]
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