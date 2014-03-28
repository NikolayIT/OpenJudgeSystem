namespace OJS.Web
{
    using System.Data.Entity;
    using System.Net;
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;

    using OJS.Data;
    using OJS.Data.Migrations;

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // TODO: Remove when an SSL certificate is added to the judge system
            ServicePointManager.ServerCertificateValidationCallback +=
                (s, cert, chain, sslPolicyErrors) => true;

            // Database.SetInitializer(new DropCreateDatabaseIfModelChanges<OjsDbContext>());
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<OjsDbContext, DefaultMigrationConfiguration>());
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ControllerBuilder.Current.SetControllerFactory(new NinjectControllerFactory());
        }
    }
}
