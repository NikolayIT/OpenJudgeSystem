namespace OJS.Web.Common.Attributes
{
    using System.Web.Mvc;

    using Microsoft.AspNet.Identity;

    using OJS.Data;
    using OJS.Data.Models;

    public class LoggerFilterAttribute : IActionFilter
    {
        private readonly IOjsData data;

        public LoggerFilterAttribute(IOjsData data)
        {
            this.data = data;
        }

        public virtual void OnActionExecuting(ActionExecutingContext filterContext)
        {
        }

        public virtual void OnActionExecuted(ActionExecutedContext filterContext)
        {
            this.LogAction(filterContext);
        }

        protected virtual void LogAction(ActionExecutedContext filterContext)
        {
            string userId = null;
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                userId = filterContext.HttpContext.User.Identity.GetUserId();
            }

            var request = filterContext.RequestContext.HttpContext.Request;
            this.data.AccessLogs.Add(new AccessLog
            {
                IpAddress = request.UserHostAddress,
                Url = request.RawUrl,
                UserId = userId,
                RequestType = request.RequestType,
                PostParams = request.Unvalidated.Form.ToString(),
            });

            this.data.SaveChanges();
        }
    }
}
