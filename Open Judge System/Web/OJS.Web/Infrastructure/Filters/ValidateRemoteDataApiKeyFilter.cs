namespace OJS.Web.Infrastructure.Filters
{
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    using OJS.Common;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Common;
    using OJS.Web.Infrastructure.Filters.Attributes;
    using OJS.Web.Infrastructure.Filters.Contracts;

    public class ValidateRemoteDataApiKeyFilter : IActionFilter<ValidateRemoteDataApiKeyAttribute>
    {
        private const string ApiKeyQueryStringParamName = "apiKey";

        private readonly UserManager<UserProfile> userManager;

        public ValidateRemoteDataApiKeyFilter(IOjsDbContext context) =>
            this.userManager = new OjsUserManager<UserProfile>(new UserStore<UserProfile>(context.DbContext));

        public void OnActionExecuting(
            ValidateRemoteDataApiKeyAttribute attribute,
            ActionExecutingContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            var apiKey = request.QueryString[ApiKeyQueryStringParamName] ?? request[ApiKeyQueryStringParamName];

            var isValidApiKey = !string.IsNullOrWhiteSpace(apiKey) &&
                this.userManager.Users.Any(u => u.Id == apiKey && u.IsDeleted == false) &&
                this.userManager.IsInRole(apiKey, GlobalConstants.AdministratorRoleName);

            if (!isValidApiKey)
            {
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Invalid API key.");
            }
        }

        public void OnActionExecuted(
            ValidateRemoteDataApiKeyAttribute attribute,
            ActionExecutedContext filterContext)
        {
        }
    }
}