namespace OJS.Workers.LocalWorkerMonitoring
{
    using System.ServiceProcess;

    using log4net;

    using OJS.Workers.Common;

    internal class LocalWorkerMonitoringService : ServiceBase
    {
        private static ILog logger;
        private readonly ServiceController localWorkerServiceController;

        public LocalWorkerMonitoringService()
        {
            logger = LogManager.GetLogger(Constants.LocalWorkerServiceLogName);
            this.localWorkerServiceController = new ServiceController("OJS Local Worker Service");
        }

        protected override void OnStart(string[] args)
        {
            logger.Info("LocalWorkerMonitoringService starting...");
            logger.Info($"LocalWorkerService status is: {this.localWorkerServiceController.Status}");

            logger.Info("LocalWorkerMonitoringService started.");
        }

        protected override void OnStop()
        {
            logger.Info("LocalWorkerMonitoringService stopping...");

            logger.Info("LocalWorkerMonitoringService stopped.");
        }
    }
}