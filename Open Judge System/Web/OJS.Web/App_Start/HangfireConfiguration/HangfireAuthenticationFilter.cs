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
            var isUserAdmin = owinContext.Authentication.User.IsAdmin();
            return isUserAdmin;
        }
    }
}
