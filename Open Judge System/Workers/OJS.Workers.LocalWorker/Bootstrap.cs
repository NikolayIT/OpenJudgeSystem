namespace OJS.Workers.LocalWorker
{
    using System.Linq;
    using OJS.Data;
    using OJS.Data.Repositories.Base;
    using OJS.Data.Repositories.Contracts;
    using OJS.Services.Common;
    using OJS.Services.Data.SubmissionsForProcessing;

    using SimpleInjector;
    using SimpleInjector.Lifestyles;

    internal class Bootstrap
    {
        public static Container Container;

        public static void Start(Container container)
        {
            container.Options.DefaultScopedLifestyle = new ThreadScopedLifestyle();

            RegisterTypes(container);

            container.Verify();

            Container = container;
        }

        private static void RegisterTypes(Container container)
        {
            container.Register<LocalWorkerService>(Lifestyle.Scoped);

            container.Register<IOjsDbContext, OjsDbContext>(Lifestyle.Scoped);

            container.Register(
                typeof(IEfGenericRepository<>),
                typeof(EfGenericRepository<>),
                Lifestyle.Scoped);

            container.Register(
                typeof(IEfDeletableEntityRepository<>),
                typeof(EfDeletableEntityRepository<>),
                Lifestyle.Scoped);

            RegisterServices(container);
        }

        private static void RegisterServices(Container container)
        {
            var servicesAssembly = typeof(ISubmissionsForProcessingDataService).Assembly;

            var registrations = servicesAssembly
                .GetExportedTypes()
                .Where(type =>
                    typeof(IService).IsAssignableFrom(type) &&
                    !type.IsAbstract &&
                    !type.IsGenericTypeDefinition)
                .Select(type => new
                {
                    ServiceType = type
                        .GetInterfaces()
                        .First(i => i.IsPublic &&
                            !i.GenericTypeArguments.Any() &&
                            i != typeof(IService)),
                    Implementation = type
                });

            foreach (var registration in registrations)
            {
                container.Register(registration.ServiceType, registration.Implementation, Lifestyle.Scoped);
            }
        }
    }
}