[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace OJS.Workers.LocalWorker
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.ServiceProcess;

    using Ninject;

    using OJS.Data;
    using OJS.Services.Business.SubmissionsForProcessing;
    using OJS.Services.Data.SubmissionsForProcessing;

    internal static class Program
    {
        /// <summary>
        /// The main entry point for the service.
        /// </summary>
        private static void Main()
        {
            try
            {
                // Explicitly set App.config file location to prevent confusion
                // ReSharper disable once AssignNullToNotNullAttribute
                Environment.CurrentDirectory = Path.GetDirectoryName(typeof(Program).Assembly.Location);
                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", "OJS.Workers.LocalWorker.exe.config");

                using (IKernel kernel = new StandardKernel())
                {
                    kernel.Bind<IOjsDbContext>().To<OjsDbContext>();

                    kernel
                        .Bind<ISubmissionsForProcessingDataService>()
                        .To<SubmissionsForProcessingDataService>();

                    kernel
                        .Bind<ISubmissionsForProcessingBusinessService>()
                        .To<SubmissionsForProcessingBusinessService>();

                    var localWorkerService = kernel.Get<LocalWorkerService>();

                    // Run the service
                    var servicesToRun = new ServiceBase[] { localWorkerService };
                    ServiceBase.Run(servicesToRun);
                }
            }
            catch (Exception exception)
            {
                const string Source = "OJS.Workers.LocalWorker";
                if (!EventLog.SourceExists(Source))
                {
                    EventLog.CreateEventSource(Source, "Application");
                }

                EventLog.WriteEntry(Source, exception.ToString(), EventLogEntryType.Error);
            }
        }
    }
}
