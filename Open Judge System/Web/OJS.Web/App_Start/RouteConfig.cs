namespace OJS.Web
{
    using System.Web.Mvc;
    using System.Web.Routing;

    using OJS.Web.Controllers;

    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // TODO: Unit test this route
            routes.MapRoute("robots.txt", "robots.txt", new { controller = "Home", action = "RobotsTxt" }, new[] { "OJS.Web.Controllers" });

            RegisterRedirectsToOldSystemUrls(routes);

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "OJS.Web.Controllers" });
        }

        public static void RegisterRedirectsToOldSystemUrls(RouteCollection routes)
        {
            for (var i = 0; i < RedirectsController.OldSystemRedirects.Count; i++)
            {
                var redirect = RedirectsController.OldSystemRedirects[i];
                routes.MapRoute(
                    name: string.Format("RedirectOldSystemUrl_{0}", i),
                    url: redirect.Key,
                    defaults: new { controller = "Redirects", action = "Index", id = i },
                    namespaces: new[] { "OJS.Web.Controllers" });
            }
        }
    }
}
