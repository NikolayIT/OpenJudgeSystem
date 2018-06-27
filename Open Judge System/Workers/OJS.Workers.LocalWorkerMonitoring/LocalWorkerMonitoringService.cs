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
            logger = LogManager.GetLogger(Constants.LocalWorkerMonitoringServiceLogName);

            this.localWorkerServiceController = new ServiceController(Constants.LocalWorkerServiceName);
        }

        protected override void OnStart(string[] args)
        {
            logger.Info($"{nameof(LocalWorkerMonitoringService)} starting...");
            logger.Info($"{Constants.LocalWorkerServiceName} status is: {this.localWorkerServiceController.Status}");

            logger.Info($"{nameof(LocalWorkerMonitoringService)} started.");
        }

        protected override void OnStop()
        {
            logger.Info($"{nameof(LocalWorkerMonitoringService)} stopping...");

            logger.Info($"{nameof(LocalWorkerMonitoringService)} stopped.");
        }
    }
}