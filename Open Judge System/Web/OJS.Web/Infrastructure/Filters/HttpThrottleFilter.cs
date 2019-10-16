namespace Suls.Web.Common.Filters
{
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;
    using System.Web.Routing;
    
    using MvcThrottle;

    public class HttpThrottleFilter : ThrottlingFilter
    {
        public HttpThrottleFilter(
            int limitPerSecond,
            int limitPerMinutre,
            int limitPerHour,
            int limitPerDay,
            string[] whiteListRange)
        {
            this.Policy = new ThrottlePolicy(limitPerSecond, limitPerMinutre, limitPerHour, limitPerDay)
            {
                IpThrottling = true,
                IpWhitelist = whiteListRange.ToList(),
            };
            this.Repository = new CacheRepository();
        }

        protected override ActionResult QuotaExceededResult(
            RequestContext filterContext,
            string message,
            HttpStatusCode responseCode,
            string requestId)
            => new ViewResult
            {
                ViewName = "ExceededRequestQuota",
            };
    }
}
