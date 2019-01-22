namespace OJS.LocalWorker
{
    using System.Data.Entity;
    using System.Linq;

    using MissingFeatures;

    using OJS.Data;
    using OJS.Data.Archives;
    using OJS.Data.Archives.Repositories;
    using OJS.Data.Archives.Repositories.Contracts;
    using OJS.Data.Repositories.Base;
    using OJS.Data.Repositories.Contracts;
    using OJS.Services.Common;
    using OJS.Services.Common.BackgroundJobs;
    using OJS.Services.Data.SubmissionsForProcessing;
    using OJS.Workers.SubmissionProcessors;

    using SimpleInjector;
    using SimpleInjector.Lifestyles;

    internal class Bootstrap
    {
        public static SimpleInjectorContainer Container { get; private set; }

        public static void Start(SimpleInjectorContainer container)
        {
            container.Options.DefaultScopedLifestyle = new ThreadScopedLifestyle();

            RegisterTypes(container);

            container.Verify();

            Container = container;
        }

        private static void RegisterTypes(Container container)
        {
            container.Register<OjsDbContext>(Lifestyle.Scoped);
            container.Register<ArchivesDbContext>(Lifestyle.Scoped);
            container.Register<DbContext>(container.GetInstance<OjsDbContext>, Lifestyle.Scoped);

            container.Register(
                typeof(ISubmissionProcessingStrategy<>),
                typeof(OjsSubmissionProcessingStrategy),
                Lifestyle.Scoped);

            container.Register(
                typeof(IEfGenericRepository<>),
                typeof(EfGenericRepository<>),
                Lifestyle.Scoped);

            container.Register(
                typeof(IEfDeletableEntityRepository<>),
                typeof(EfDeletableEntityRepository<>),
                Lifestyle.Scoped);

            container.Register(
                typeof(IArchivesGenericRepository<>),
                typeof(ArchivesGenericReposity<>),
                Lifestyle.Scoped);

            RegisterServices(container);
        }

        private static void RegisterServices(Container container)
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
        }
    }
}