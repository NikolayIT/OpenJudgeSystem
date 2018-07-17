namespace OJS.Web
{
    using System.Web.Mvc;
    using System.Web.Routing;

    using OJS.Common;

    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapMvcAttributeRoutes();

            AreaRegistration.RegisterAllAreas();

            // TODO: Unit test this route
            routes.MapRoute("robots.txt", "robots.txt", new { controller = "Home", action = "RobotsTxt" }, new[] { "OJS.Web.Controllers" });

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = GlobalConstants.Index, id = UrlParameter.Optional },
                namespaces: new[] { "OJS.Web.Controllers" });
        }
    }
}
