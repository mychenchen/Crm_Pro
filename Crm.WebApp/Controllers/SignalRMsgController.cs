using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crm.WebApp.Infrastructure;
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

    }
}