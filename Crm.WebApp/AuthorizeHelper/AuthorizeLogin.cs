
using Crm.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace Crm.WebApp.AuthorizeHelper
{
    /// <summary>
    /// 所有API请求拦截器
    /// </summary>
    public class AuthorizeLogin : ActionFilterAttribute
    {
        /// <summary>
        /// 在执行请求开始之前进行调用
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // 判断是否加上了不需要拦截
            var noNeedCheck = false;
            if (context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            {
                noNeedCheck = controllerActionDescriptor.MethodInfo.GetCustomAttributes(inherit: true)
                  .Any(a => a.GetType().Equals(typeof(NoSignAttribute)));
            }
            if (noNeedCheck) return;

            //拦截全局里是否带了token
            var token = context.HttpContext.Request.Headers["ToKenStr"];
            if (string.IsNullOrEmpty(token))
            {
                context.Result = new JsonResult(new ResultObject()
                {
                    code = (int)ErrorCode.NoAuthorize,
                    message = "token失效",
                    resultTime = "",
                    data = ""
                });
            }


            //base.OnActionExecuting(filterContext);


            //filterContext.HttpContext.Response.Write("<br />" + "执行OnActionExecuting：" + Message + "<br />");
        }

        /// <summary>
        /// 在执行请求开始之后进行调用
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            //filterContext.HttpContext.Response.Write("<br />" + "执行OnActionExecuted：" + Message + "<br />");
        }

        /// <summary>
        /// 在执行请求结束前进行调用
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            base.OnResultExecuting(filterContext);
            //filterContext.HttpContext.Response.Write("<br />" + "执行OnResultExecuting：" + Message + "<br />");
        }

        /// <summary>
        /// 在执行请求结束后进行调用
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            base.OnResultExecuted(filterContext);
            //filterContext.HttpContext.Response.Write("<br />" + "执行OnResultExecuted：" + Message + "<br />");
        }
    }
}