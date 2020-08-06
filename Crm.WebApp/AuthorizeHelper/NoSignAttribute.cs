using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Crm.WebApp.AuthorizeHelper
{
    /// <summary>
    /// 不需要登陆的地方加个这个空的拦截器
    /// </summary>
    public class NoSignAttribute : ActionFilterAttribute { }
}
