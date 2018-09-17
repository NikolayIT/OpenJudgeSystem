namespace OJS.LocalWorkerMonitoring
{
    using System;
    using System.IO;
    using System.ServiceProcess;

    using SimpleInjector;
    using SimpleInjector.Lifestyles;

    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            Environment.CurrentDirectory = Path.GetDirectoryName(typeof(Program).Assembly.Location);

            var container = new Container();
            Bootstrap.Start(container);

            using (AsyncScopedLifestyle.BeginScope(container))
            {
                var monitoringService = container.GetInstance<LocalWorkerMonitoringService>();

                monitoringService.CanStop = false;

                ServiceBase.Run(monitoringService);
            }
        } 
    }
}