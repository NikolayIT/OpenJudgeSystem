namespace OJS.Web.Common.Attributes
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;
    using System.Web.Caching;
    using System.Web.Mvc;

    using OJS.Web.Common.Extensions;

    public class RestrictRequestsAttribute : ActionFilterAttribute
    {
        private const int DefaultRequestsPerInterval = 3;
        private const int DefaultRestrictInterval = 300;

        // The default Error Message that will be displayed in case of excessive Requests
        private string errorMessage = "Моля, изчакайте 10 секунди преди да пробвате пак.";

        // This will store the URL to Redirect errors to
        // NOTE: Not implemented.
        //// private string redirectURL;

        /// <summary>
        /// Gets or sets the number of request before the protection is turned on
        /// </summary>
        public int RequestsPerInterval { get; set; } = DefaultRequestsPerInterval;

        /// <summary>
        /// Gets or sets restrict interval in seconds
        /// </summary>
        public int RestrictInterval { get; set; } = DefaultRestrictInterval;

        public string ErrorMessage
        {
            get
            {
                var resourceProperty = this.ResourceType?.GetProperty(this.ResourceName, BindingFlags.Static | BindingFlags.Public);
                if (resourceProperty != null)
                {
                    return (string)resourceProperty.GetValue(resourceProperty.DeclaringType, null);
                }

                return this.errorMessage;
            }

            set
            {
                this.errorMessage = value;
            }
        }

        public Type ResourceType { get; set; }

        public string ResourceName { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.User.IsAdmin())
            {
                // Store our HttpContext (for easier reference and code brevity)
                var request = filterContext.HttpContext.Request;

                // Grab the IP Address from the originating Request (very simple implementation for example purposes)
                // and append the User Agent
                var originationInfo =
                    $"{request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? request.UserHostAddress}{request.UserAgent}";

                // Now we just need the target URL Information
                var targetInfo = request.RawUrl + request.QueryString;

                // Generate a hash for your strings (this appends each of the bytes of the value into a single hashed string)
                var hashValue = string.Join(
                    string.Empty,
                    MD5.Create()
                        .ComputeHash(Encoding.ASCII.GetBytes(originationInfo + targetInfo))
                        .Select(s => s.ToString("x2")));

                // Store our HttpContext.Cache (for easier reference and code brevity)
                var cache = filterContext.HttpContext.Cache;

                // Checks if the hashed value is contained in the Cache (indicating a repeat request)
                if (cache[hashValue] != null)
                {
                    // Converts the cache into a int to check the number of request to this point
                    var cacheValue = int.Parse(cache[hashValue].ToString());

                    if (cacheValue >= this.RequestsPerInterval)
                    {
                        // Adds the Error Message to the Model and Redirect
                        filterContext.Controller.ViewData.ModelState.AddModelError(string.Empty, this.ErrorMessage);
                    }
                    else
                    {
                        // Increments the number of requests
                        cacheValue++;
                        cache.Remove(hashValue);
                        cache.Add(hashValue, cacheValue, null, DateTime.Now.AddSeconds(this.RestrictInterval), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
                    }
                }
                else
                {
                    // Adds an 1 int (representing the first request) to the cache using the hashValue to a key
                    // (This sets the expiration that will determine if the Request is valid or not)
                    cache.Add(hashValue, 1, null, DateTime.Now.AddSeconds(this.RestrictInterval), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
