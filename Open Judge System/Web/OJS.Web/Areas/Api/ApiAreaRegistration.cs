namespace OJS.Web.Areas.Api
{
    using System.Web.Mvc;

    using OJS.Common;

    public class ApiAreaRegistration : AreaRegistration
    {
        public override string AreaName => "Api";

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Api_default",
                "Api/{controller}/{action}/{id}",
                new { action = GlobalConstants.Index, id = UrlParameter.Optional });
        }
    }
}