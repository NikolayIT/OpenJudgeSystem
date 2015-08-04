namespace OJS.Web.Common.Attributes
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using Microsoft.AspNet.Identity;

    using OJS.Data;
    using OJS.Data.Models;

    public class LogAttribute : IActionFilter
    {
        private IOjsData data;

        public LogAttribute(IOjsData data)
        {
            this.data = data;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            string userId = null;
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                userId = filterContext.HttpContext.User.Identity.GetUserId();
            }

            var request = filterContext.RequestContext.HttpContext.Request;
            this.data.UsageLogs.Add(new UsageLog()
            {
                IpAddress = request.UserHostAddress,
                Url = request.RawUrl,
                UserId = userId,
                RequestType = request.RequestType,
                PostParams = request.Form.ToString(),
            });
        }
    }
}
