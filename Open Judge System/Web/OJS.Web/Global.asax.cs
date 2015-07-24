namespace OJS.Web
{
    using System.Data.Entity;
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;

    using OJS.Data;
    using OJS.Data.Migrations;
    using OJS.Data.Providers.Registries;

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Database.SetInitializer(new DropCreateDatabaseIfModelChanges<OjsDbContext>());
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<OjsDbContext, DefaultMigrationConfiguration>());
            EfBulkInsertGlimpseProviderRegistry.Execute();

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            ViewEngineConfig.RegisterViewEngines(ViewEngines.Engines);

            ControllerBuilder.Current.SetControllerFactory(new NinjectControllerFactory());
        }
    }
}
