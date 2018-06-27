namespace OJS.Workers.LocalWorkerMonitoring
{
    using System;
    using System.ServiceProcess;
    using System.Timers;

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

            // Set up a timer to trigger every minute.
            var timer = new Timer { Interval = 60000 };
            timer.Elapsed += this.OnTimer;
            timer.Start();

            logger.Info($"{nameof(LocalWorkerMonitoringService)} started.");
        }

        protected override void OnStop()
        {
            logger.Info($"{nameof(LocalWorkerMonitoringService)} stopped.");
        }

        private void OnTimer(object sender, ElapsedEventArgs args)
        {
            var localWorkerServiceStatus = this.localWorkerServiceController.Status;

            if (localWorkerServiceStatus == ServiceControllerStatus.StopPending)
            {
                logger.Warn($"{Constants.LocalWorkerServiceName} is stopping...");
            }
            else if (localWorkerServiceStatus == ServiceControllerStatus.Stopped)
            {
                logger.Warn($"{Constants.LocalWorkerServiceName} has stopped.");

                try
                {
                    this.localWorkerServiceController.Start();
                    logger.Info($"{Constants.LocalWorkerServiceName} has started successfully.");
                }
                catch (Exception ex)
                {
                    logger.Error($"Restarting the {Constants.LocalWorkerServiceName} has thrown an exception: {ex.Message}");
                }
            }
        }
    }
}