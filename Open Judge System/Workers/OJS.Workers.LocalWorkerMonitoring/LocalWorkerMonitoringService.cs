namespace OJS.Workers.LocalWorkerMonitoring
{
    using System;
    using System.ServiceProcess;
    using System.Timers;

    using log4net;

    using OJS.Common.Helpers;
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
            if (!ServicesHelper.ServiceIsInstalled(Constants.LocalWorkerServiceName))
            {
                logger.Warn($"the {Constants.LocalWorkerServiceName} is not found");
                InstallAndStartLocalWorker();
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
                    StartLocalWorker();
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

        private static void InstallAndStartLocalWorker()
        {
            const string localWorkerExePath =
                @"..\..\..\..\Workers\OJS.Workers.LocalWorker\bin\Debug\OJS.Workers.LocalWorker.exe";

            try
            {
                logger.Info($"Attempting to install and start the {Constants.LocalWorkerServiceName}");

                ServicesHelper.InstallAndStart(Constants.LocalWorkerServiceName, localWorkerExePath);

                logger.Info($"{Constants.LocalWorkerServiceName} installed and started successfully.");
            }
            catch (Exception ex)
            {
                logger.Error(
                    $"An exception was thrown while attempting to install and start the {Constants.LocalWorkerServiceName}", ex);
            }
        }

        private static void StartLocalWorker()
        {
            try
            {
                logger.Info($"Attempting to start the {Constants.LocalWorkerServiceName}");

                ServicesHelper.StartService(Constants.LocalWorkerServiceName);

                logger.Info($"{Constants.LocalWorkerServiceName} started successfully.");
            }
            catch (Exception ex)
            {
                logger.Error($"An exception was thrown while attempting to start the {Constants.LocalWorkerServiceName}", ex);
            }
        }
    }
}