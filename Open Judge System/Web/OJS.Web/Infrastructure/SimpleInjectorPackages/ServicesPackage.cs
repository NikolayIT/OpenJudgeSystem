namespace OJS.Web.Infrastructure.SimpleInjectorPackages
{
    using System.Linq;

    using MissingFeatures;

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
            var serviceAssemblies = new[]
            {
                typeof(ISubmissionsForProcessingDataService).Assembly,
                typeof(IHangfireBackgroundJobService).Assembly
            };

            var registrations = serviceAssemblies
                .SelectMany(a => a.GetExportedTypes())
                .Where(type =>
                    typeof(IService).IsAssignableFrom(type) &&
                    !type.IsAbstract &&
                    !type.IsGenericTypeDefinition)
                .Select(type => new
                {
                    ServiceTypes = type
                        .GetInterfaces()
                        .Where(i =>
                            i.IsPublic &&
                            !i.GenericTypeArguments.Any() &&
                            i != typeof(IService)),
                    Implementation = type
                });

            foreach (var registration in registrations)
            {
                registration.ServiceTypes.ForEach(
                    service => container.Register(service, registration.Implementation, Lifestyle.Scoped));
            }

            container.Register<ISimilarityFinder, SimilarityFinder>(Lifestyle.Scoped);
            container.Register<IPlagiarismDetectorFactory, PlagiarismDetectorFactory>(Lifestyle.Scoped);
        }
    }
}