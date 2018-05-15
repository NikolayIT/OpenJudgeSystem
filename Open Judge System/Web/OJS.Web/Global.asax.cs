namespace OJS.Web
{
    using System;
    using System.Data.Entity;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;

    using HangfireConfiguration;

    using OJS.Data;
    using OJS.Data.Archives;
    using OJS.Data.Archives.Migrations;
    using OJS.Data.Migrations;
    using OJS.Data.Providers.Registries;
    using OJS.Web.Infrastructure.Filters;

    public class Global : HttpApplication
    {
        protected void Application_Start()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<OjsDbContext, DefaultMigrationConfiguration>());
            Database.SetInitializer(
                new MigrateDatabaseToLatestVersion<ArchivesDbContext,
                    ArchivesDefaultMigrationConfiguration>());
            EfBulkInsertGlimpseProviderRegistry.Execute();

            FilterConfig.RegisterGlobalFilters(
                GlobalFilters.Filters,
                new object[] { new ActionFilterDispatcher(SimpleInjectorConfig.Container.GetAllInstances) });
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            ViewEngineConfig.RegisterViewEngines(ViewEngines.Engines);
            HangfireBootstrapper.Instance.Start();
        }

        protected void Application_End(object sender, EventArgs e)
        {
            HangfireBootstrapper.Instance.Stop();
        }
    }
}