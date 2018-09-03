[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace OJS.Workers.LocalWorker
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.ServiceProcess;

    using OJS.Common;

    using SimpleInjector;
    using SimpleInjector.Lifestyles;

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

                var container = new Container();
                Bootstrap.Start(container);

                ObjectFactory.InitializeServiceProvider(Bootstrap.Container);

                using (ThreadScopedLifestyle.BeginScope(container))
                {
                    var localWorkerService = container.GetInstance<LocalWorkerService>();

                    ServiceBase.Run(localWorkerService);
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