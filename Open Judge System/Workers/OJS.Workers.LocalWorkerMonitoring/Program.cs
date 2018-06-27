namespace OJS.Workers.LocalWorkerMonitoring
{
    using System.ServiceProcess;

    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main() => ServiceBase.Run(new LocalWorkerMonitoringService());
    }
}