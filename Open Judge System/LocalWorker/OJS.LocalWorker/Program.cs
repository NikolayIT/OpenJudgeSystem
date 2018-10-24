[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace OJS.LocalWorker
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.ServiceProcess;

    using OJS.Workers.Common;

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
                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", "OJS.LocalWorker.exe.config");

                var container = new SimpleInjectorContainer();
                Bootstrap.Start(container);

                var localWorkerService = new LocalWorkerService();

                ServiceBase.Run(localWorkerService);
            }
            catch (Exception exception)
            {
                const string Source = "OJS.LocalWorker";
                if (!EventLog.SourceExists(Source))
                {
                    EventLog.CreateEventSource(Source, "Application");
                }

                EventLog.WriteEntry(Source, exception.ToString(), EventLogEntryType.Error);
            }
        }
    }
}