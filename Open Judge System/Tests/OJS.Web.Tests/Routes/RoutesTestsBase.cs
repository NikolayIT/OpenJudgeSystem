namespace OJS.Web.Tests.Routes
{
    using System.Web.Mvc;
    using System.Web.Routing;

    using OJS.Tests.Common.WebStubs;

    public class RoutesTestsBase : BaseWebTests
    {
        public RouteData GetRouteData(string url)
        {
            var context = new StubHttpContextForRouting(requestUrl: url);
            var routes = new RouteCollection();
            RouteConfig.RegisterRoutes(routes);

            var routeData = routes.GetRouteData(context);
            return routeData;
        }

        public RouteData GetAreaRouteData(string url, AreaRegistration areaConfig)
        {
            var routes = new RouteCollection();

            var areaContext = new AreaRegistrationContext(areaConfig.AreaName, routes);
            areaConfig.RegisterArea(areaContext);

            var context = new StubHttpContextForRouting(requestUrl: url);

            var routeData = routes.GetRouteData(context);
            return routeData;
        }
    }
}
