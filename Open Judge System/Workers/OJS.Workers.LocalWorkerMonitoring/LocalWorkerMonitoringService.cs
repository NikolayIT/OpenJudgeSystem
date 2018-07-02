namespace OJS.Workers.LocalWorkerMonitoring
{
    using System;
    using System.ServiceProcess;
    using System.Timers;

    using log4net;

    using OJS.Common.Helpers;
    using OJS.Workers.Common;
    using OJS.Workers.Common.Helpers;

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
            if (!ServicesHelper.ServiceIsInstalled(Constants.LocalWorkerServiceName))
            {
                const string localWorkerExePath =
                    @"..\..\..\..\Workers\OJS.Workers.LocalWorker\bin\Debug\OJS.Workers.LocalWorker.exe";

                logger.Warn($"the {Constants.LocalWorkerServiceName} is not found");
                LocalWorkerServicesHelper.TryInstallAndStartService(
                    Constants.LocalWorkerServiceName,
                    localWorkerExePath,
                    logger);

                return;
            }

            var localWorkerStatus = ServicesHelper.GetServiceStatus(Constants.LocalWorkerServiceName);

            switch (localWorkerStatus)
            {
                case ServiceControllerStatus.Running:
                    break;
                case ServiceControllerStatus.StartPending:
                    logger.Info($"{Constants.LocalWorkerServiceName} is starting...");
                    break;
                case ServiceControllerStatus.StopPending:
                    logger.Warn($"{Constants.LocalWorkerServiceName} is stopping...");
                    break;
                case ServiceControllerStatus.Stopped:
                    logger.Warn($"{Constants.LocalWorkerServiceName} is stopped.");
                    LocalWorkerServicesHelper.TryStartService(Constants.LocalWorkerServiceName, logger);
                    break;
                case ServiceControllerStatus.PausePending:
                    logger.Warn($"{Constants.LocalWorkerServiceName} is pausing...");
                    break;
                case ServiceControllerStatus.Paused:
                    logger.Warn($"{Constants.LocalWorkerServiceName} is paused.");
                    break;
                case ServiceControllerStatus.ContinuePending:
                    logger.Warn($"{Constants.LocalWorkerServiceName} is resuming...");
                    break;
                default:
                    logger.Error(new ArgumentOutOfRangeException(nameof(localWorkerStatus), "Invalid service state"));
                    break;
            }
        }
    }
}