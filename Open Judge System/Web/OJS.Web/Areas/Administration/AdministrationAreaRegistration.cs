namespace OJS.Web.Areas.Administration
{
    using System.Web.Mvc;

    using OJS.Common;

    public class AdministrationAreaAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Administration";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Administration_Files_Connector",
                "Administration/Files/connector",
                new { action = "FileConnector", controller = "Files" });

            context.MapRoute(
                "Administration_Files_Thumbnails",
                "Administration/Files/Thumbnails/{tmb}",
                new { action = "Thumbs", controller = "Files", tmb = UrlParameter.Optional });

            context.MapRoute(
                "Administration_default",
                "Administration/{controller}/{action}/{id}",
                new { action = GlobalConstants.Index, id = UrlParameter.Optional });
        }
    }
}