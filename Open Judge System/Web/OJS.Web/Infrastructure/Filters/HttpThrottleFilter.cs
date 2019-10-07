namespace Suls.Web.Common.Filters
{
    using System.Linq;

    using MvcThrottle;

    public class HttpThrottleFilter : ThrottlingFilter
    {
        public HttpThrottleFilter(
            int limitPerSecond,
            int limitPerMinutre,
            int limitPerHour,
            int limitPerDay,
            string[] whiteListRange) =>
            new ThrottlingFilter()
            {
                Policy = new ThrottlePolicy(limitPerSecond, limitPerMinutre, limitPerHour, limitPerDay)
                {
                    IpThrottling = true,
                    IpWhitelist = whiteListRange.ToList(),
                },
                Repository = new CacheRepository(),
            };
    }
}
