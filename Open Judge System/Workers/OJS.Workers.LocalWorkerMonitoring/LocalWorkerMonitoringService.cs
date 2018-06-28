namespace OJS.Workers.LocalWorkerMonitoring
{
    using System;
    using System.ServiceProcess;
    using System.Timers;

    using log4net;

    using OJS.Workers.Common;

    internal class LocalWorkerMonitoringService : ServiceBase
    {
        private const int IntervalInMilliseconds = 60000;

        private static ILog logger;

        public LocalWorkerMonitoringService() =>
            logger = LogManager.GetLogger(Constants.LocalWorkerMonitoringServiceLogName);

        protected override void OnStart(string[] args)
        {
            var timer = new Timer { Interval = IntervalInMilliseconds };
            timer.Elapsed += this.OnTimerElapsed;
            timer.Start();

            logger.Info($"{nameof(LocalWorkerMonitoringService)} started.");
        }

        protected override void OnStop()
        {
            logger.Info($"{nameof(LocalWorkerMonitoringService)} stopped.");
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs args)
        {
            using (var localWorkerServiceController = new ServiceController(Constants.LocalWorkerServiceName))
            {
                switch (localWorkerServiceController.Status)
                {
                    case ServiceControllerStatus.StopPending:
                        logger.Warn($"{Constants.LocalWorkerServiceName} is stopping...");
                        logger.Info($"Waiting for the {Constants.LocalWorkerServiceName} to stop...");
                        localWorkerServiceController.WaitForStatus(ServiceControllerStatus.Stopped);
                        TryStartService(localWorkerServiceController);
                        break;
                    case ServiceControllerStatus.Stopped:
                        logger.Warn($"{Constants.LocalWorkerServiceName} has stopped.");
                        TryStartService(localWorkerServiceController);
                        break;
                    case ServiceControllerStatus.ContinuePending:
                        logger.Warn($"{Constants.LocalWorkerServiceName} is resuming...");
                        break;
                    case ServiceControllerStatus.Paused:
                        logger.Warn($"{Constants.LocalWorkerServiceName} has paused.");
                        break;
                    case ServiceControllerStatus.PausePending:
                        logger.Warn($"{Constants.LocalWorkerServiceName} is pausing...");
                        break;
                    case ServiceControllerStatus.StartPending:
                        logger.Info($"{Constants.LocalWorkerServiceName} is starting...");
                        break;
                    case ServiceControllerStatus.Running:
                        break;
                    default:
                        logger.Error(new ArgumentOutOfRangeException(string.Empty, nameof(ServiceControllerStatus)));
                        break;
                }
            }
        }

        private static void TryStartService(ServiceController serviceController)
        {
            try
            {
                serviceController.Start();
                logger.Info($"Attempting to start the {Constants.LocalWorkerServiceName}...");
                serviceController.WaitForStatus(ServiceControllerStatus.Running);
                logger.Info($"{Constants.LocalWorkerServiceName} has started successfully.");
            }
            catch (Exception ex)
            {
                logger.Error($"Starting the {Constants.LocalWorkerServiceName} has thrown an exception: {ex.Message}");
            }
        }
    }
}