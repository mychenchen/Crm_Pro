
using Microsoft.AspNetCore.Mvc.Filters;

namespace CMS.Project
{
    /// <summary>
    ///
    /// </summary>
    public class AuthorizeLogin : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            //filterContext.HttpContext.Response.Write("<br />" + "执行OnActionExecuting：" + Message + "<br />");
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            //filterContext.HttpContext.Response.Write("<br />" + "执行OnActionExecuted：" + Message + "<br />");
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            base.OnResultExecuting(filterContext);
            //filterContext.HttpContext.Response.Write("<br />" + "执行OnResultExecuting：" + Message + "<br />");
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            base.OnResultExecuted(filterContext);
            //filterContext.HttpContext.Response.Write("<br />" + "执行OnResultExecuted：" + Message + "<br />");
        }
    }
}