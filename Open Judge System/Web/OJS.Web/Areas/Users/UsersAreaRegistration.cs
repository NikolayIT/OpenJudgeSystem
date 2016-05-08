namespace OJS.Web.Areas.Users
{
    using System.Web.Mvc;

    using OJS.Common;

    public class UsersAreaRegistration : AreaRegistration
    {
        public override string AreaName => "Users";

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Users_current_user_profile",
                "Users/Profile/{action}",
                new { controller = "Profile", action = GlobalConstants.Index, id = UrlParameter.Optional });

            context.MapRoute(
                "Users_settings",
                "Users/Settings/{action}",
                new { controller = "Settings", action = GlobalConstants.Index, id = UrlParameter.Optional });

            context.MapRoute(
                "Users_profile",
                "Users/{id}",
                new { controller = "Profile", action = GlobalConstants.Index, id = UrlParameter.Optional });

            // context.MapRoute(
            //     "Users_default",
            //     "Users/{controller}/{action}/{id}",
            //     new { action = "Index", id = UrlParameter.Optional });
        }
    }
}