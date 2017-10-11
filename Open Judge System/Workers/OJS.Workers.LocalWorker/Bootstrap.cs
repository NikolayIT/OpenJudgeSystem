namespace OJS.Workers.LocalWorker
{
    using OJS.Data;
    using OJS.Services.Business.SubmissionsForProcessing;
    using OJS.Services.Data.SubmissionsForProcessing;

    using SimpleInjector;
    using SimpleInjector.Lifestyles;

    internal class Bootstrap
    {
        public static void Start(Container container)
        {
            container.Options.DefaultScopedLifestyle = new ThreadScopedLifestyle();

            container.Register<IOjsDbContext, OjsDbContext>(Lifestyle.Scoped);

            container
                .Register<
                    ISubmissionsForProcessingDataService,
                    SubmissionsForProcessingDataService>();

            container
                .Register<
                    ISubmissionsForProcessingBusinessService,
                    SubmissionsForProcessingBusinessService>();

            container.Verify();
        }
    }
}