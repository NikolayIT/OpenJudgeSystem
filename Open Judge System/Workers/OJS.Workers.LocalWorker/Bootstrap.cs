namespace OJS.Workers.LocalWorker
{
    using System.Data.Entity;
    using System.Linq;

    using MissingFeatures;

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
            container.Register<OjsDbContext>(Lifestyle.Scoped);

            container.Register<DbContext>(container.GetInstance<OjsDbContext>, Lifestyle.Scoped);

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

        private static void RegisterServices(Container container) =>
            typeof(ISubmissionsForProcessingDataService).Assembly
                .GetExportedTypes()
                .Where(type =>
                    typeof(IService).IsAssignableFrom(type) &&
                    !typeof(IArchivesService).IsAssignableFrom(type) &&
                    !type.IsAbstract &&
                    !type.IsGenericTypeDefinition)
                .Select(type => new
                {
                    ServiceType = type
                        .GetInterfaces()
                        .First(i =>
                            i.IsPublic &&
                            !i.GenericTypeArguments.Any() &&
                            i != typeof(IService)),
                    Implementation = type
                })
                .ForEach(registration =>
                    container.Register(
                        registration.ServiceType,
                        registration.Implementation,
                        Lifestyle.Scoped));
    }
}