namespace OJS.Workers.LocalWorker
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
    using OJS.Workers.Compilers;

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
            container.Register<ArchivesDbContext>(Lifestyle.Scoped);

            container.Register<DbContext>(container.GetInstance<OjsDbContext>, Lifestyle.Scoped);

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
            RegisterCompilers(container);
        }

        private static void RegisterCompilers(Container container)
        {
            container.Register(
                () => new CSharpCompiler(
                    Settings.CSharpCompilerProcessExitTimeOutMultiplier),
                Lifestyle.Scoped);

            container.Register(
                () => new CSharpDotNetCoreCompiler(
                    Settings.CSharpDotNetCoreCompilerProcessExitTimeOutMultiplier,
                    Settings.CSharpDotNetCoreCompilerPath,
                    Settings.DotNetCoreSharedAssembliesPath),
                Lifestyle.Scoped);

            container.Register(
                () => new CPlusPlusCompiler(
                    Settings.CPlusPlusCompilerProcessExitTimeOutMultiplier),
                Lifestyle.Scoped);

            container.Register(
                () => new MsBuildCompiler(
                    Settings.MsBuildCompilerProcessExitTimeOutMultiplier),
                Lifestyle.Scoped);

            container.Register(
                () => new JavaCompiler(
                    Settings.JavaCompilerProcessExitTimeOutMultiplier),
                Lifestyle.Scoped);

            container.Register(
                () => new JavaZipCompiler(
                    Settings.JavaZipCompilerProcessExitTimeOutMultiplier),
                Lifestyle.Scoped);

            container.Register(
                () => new JavaInPlaceFolderCompiler(
                    Settings.JavaInPlaceCompilerProcessExitTimeOutMultiplier),
                Lifestyle.Scoped);

            container.Register(
                () => new MsBuildLibraryCompiler(
                    Settings.MsBuildLibraryCompilerProcessExitTimeOutMultiplier),
                Lifestyle.Scoped);

            container.Register(
                () => new CPlusPlusZipCompiler(
                    Settings.CPlusPlusZipCompilerProcessExitTimeOutMultiplier),
                Lifestyle.Scoped);

            container.Register(
                () => new DotNetCompiler(
                    Settings.DotNetCompilerProcessExitTimeOutMultiplier),
                Lifestyle.Scoped);

            container.Register(
                () => new SolidityCompiler(
                    Settings.SolidityCompilerProcessExitTimeOutMultiplier),
                Lifestyle.Scoped);
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