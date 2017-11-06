namespace OJS.Web.Infrastructure.SimpleInjectorPackages
{
    using SimpleInjector;
    using SimpleInjector.Packaging;

    using OJS.Services.Common.BackgroundJobs;
    using OJS.Services.Data.SubmissionsForProcessing;
    using OJS.Workers.Tools.AntiCheat;
    using OJS.Workers.Tools.AntiCheat.Contracts;
    using OJS.Workers.Tools.Similarity;

    public class ServicesPackage : IPackage
    {
        public void RegisterServices(Container container)
        {
            container.Register<
                IHangfireBackgroundJobService,
                HangfireBackgroundJobService>();

            container.Register<
                ISubmissionsForProcessingDataService,
                SubmissionsForProcessingDataService>();

            container.Register<ISimilarityFinder, SimilarityFinder>();

            container.Register<IPlagiarismDetectorFactory, PlagiarismDetectorFactory>();
        }
    }
}