[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(OJS.Web.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethod(typeof(OJS.Web.NinjectWebCommon), "Stop")]

namespace OJS.Web
{
    using System;
    using System.Web;
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;
    using Ninject;
    using Ninject.Modules;
    using Ninject.Web.Common;

    using OJS.Data;
    using OJS.Services.Business.SubmissionsForProcessing;
    using OJS.Services.Common.BackgroundJobs;
    using OJS.Services.Data.SubmissionsForProcessing;
    using OJS.Workers.Tools.AntiCheat;
    using OJS.Workers.Tools.AntiCheat.Contracts;
    using OJS.Workers.Tools.Similarity;

    public static class NinjectWebCommon
    {
        private static readonly Bootstrapper Bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            Bootstrapper.Initialize(CreateKernel);
        }

        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            Bootstrapper.ShutDown();
        }

        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var modules = new INinjectModule[]
            {
                new LoggingModule()
            };

            var kernel = new StandardKernel(modules);
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<IOjsData>().To<OjsData>().InRequestScope();
            kernel.Bind<IOjsDbContext>().To<OjsDbContext>().InRequestScope();
            kernel.Bind<ISimilarityFinder>().To<SimilarityFinder>().InRequestScope();
            kernel.Bind<IPlagiarismDetectorFactory>().To<PlagiarismDetectorFactory>().InRequestScope();
            kernel.Bind<IHangfireBackgroundJobService>().To<HangfireBackgroundJobService>().InRequestScope();

            kernel
                .Bind<ISubmissionsForProcessingDataService>()
                .To<SubmissionsForProcessingDataService>()
                .InRequestScope();

            kernel
                .Bind<ISubmissionsForProcessingBusinessService>()
                .To<SubmissionsForProcessingBusinessService>()
                .InRequestScope();
        }
    }
}