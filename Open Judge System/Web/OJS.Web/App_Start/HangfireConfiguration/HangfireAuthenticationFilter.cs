namespace OJS.Web.HangfireConfiguration
{
    using Hangfire.Dashboard;

    using Microsoft.Owin;

    using OJS.Web.Common.Extensions;

    public class HangFireAuthenticationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var owinContext = new OwinContext(context.GetOwinEnvironment());

            return owinContext.Authentication.User.IsAdmin();
        }
    }
}
