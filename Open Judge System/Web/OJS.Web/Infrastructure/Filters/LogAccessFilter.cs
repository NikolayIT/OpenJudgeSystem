namespace OJS.Web.Infrastructure.Filters
{
    using System.Web.Mvc;

    using Microsoft.AspNet.Identity;

    using OJS.Data.Contracts;
    using OJS.Data.Models;
    using OJS.Web.Infrastructure.Filters.Attributes;
    using OJS.Web.Infrastructure.Filters.Contracts;

    public class LogAccessFilter : IActionFilter<LogAccessAttribute>
    {
        private readonly IRepository<AccessLog> accessLogs;

        public LogAccessFilter(IRepository<AccessLog> accessLogs) => this.accessLogs = accessLogs;

        public virtual void OnActionExecuting(
            LogAccessAttribute attribute,
            ActionExecutingContext filterContext)
        {
        }

        public virtual void OnActionExecuted(
            LogAccessAttribute attribute,
            ActionExecutedContext filterContext)
        {
            this.LogAction(filterContext);
        }

        private void LogAction(ActionExecutedContext filterContext)
        {
            string userId = null;
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                userId = filterContext.HttpContext.User.Identity.GetUserId();
            }

            var request = filterContext.RequestContext.HttpContext.Request;
            this.accessLogs.Add(new AccessLog
            {
                IpAddress = request.UserHostAddress,
                Url = request.RawUrl,
                UserId = userId,
                RequestType = request.RequestType,
                PostParams = request.Unvalidated.Form.ToString(),
            });

            this.accessLogs.SaveChanges();
        }
    }
}