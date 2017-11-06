namespace OJS.Web.Infrastructure.SimpleInjectorPackages
{
    using System.Linq;

    using SimpleInjector;
    using SimpleInjector.Packaging;

    using OJS.Services.Common;
    using OJS.Services.Common.BackgroundJobs;
    using OJS.Services.Data.SubmissionsForProcessing;
    using OJS.Workers.Tools.AntiCheat;
    using OJS.Workers.Tools.AntiCheat.Contracts;
    using OJS.Workers.Tools.Similarity;

    public class ServicesPackage : IPackage
    {
        public void RegisterServices(Container container)
        {
            var servicesAssembly = typeof(ISubmissionsForProcessingDataService).Assembly;

            var registrations = servicesAssembly
                .GetExportedTypes()
                .Where(type => typeof(IService).IsAssignableFrom(type) &&
                    !type.IsAbstract &&
                    !type.IsGenericTypeDefinition)
                .Select(type => new
                {
                    Service = type.GetInterfaces()
                        .Single(i => i.IsPublic &&
                            !i.GenericTypeArguments.Any() &&
                            i != typeof(IService)),
                    Implementation = type
                });

            foreach (var registration in registrations)
            {
                container.Register(registration.Service, registration.Implementation);
            }

            container.Register<IHangfireBackgroundJobService, HangfireBackgroundJobService>();

            container.Register<ISimilarityFinder, SimilarityFinder>();
            container.Register<IPlagiarismDetectorFactory, PlagiarismDetectorFactory>();
        }
    }
}