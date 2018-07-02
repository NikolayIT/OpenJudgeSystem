namespace OJS.Workers.LocalWorkerMonitoring
{
    using System;
    using System.IO;
    using System.ServiceProcess;
    using System.Timers;

    using log4net;

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
            const string localWorkerServiceName = Constants.LocalWorkerServiceName;

            if (!ServicesHelper.ServiceIsInstalled(localWorkerServiceName))
            {
                logger.Warn($"the {localWorkerServiceName} is not found");
                InstallAndStartLocalWorker();
                return;
            }

            var localWorkerStatus = ServicesHelper.GetServiceStatus(localWorkerServiceName);

            switch (localWorkerStatus)
            {
                case ServiceControllerStatus.Running:
                    break;
                case ServiceControllerStatus.StartPending:
                    logger.Info($"{localWorkerServiceName} is starting...");
                    break;
                case ServiceControllerStatus.StopPending:
                    logger.Warn($"{localWorkerServiceName} is stopping...");
                    break;
                case ServiceControllerStatus.Stopped:
                    logger.Warn($"{localWorkerServiceName} is stopped.");
                    StartLocalWorker();
                    break;
                case ServiceControllerStatus.PausePending:
                    logger.Warn($"{localWorkerServiceName} is pausing...");
                    break;
                case ServiceControllerStatus.Paused:
                    logger.Warn($"{localWorkerServiceName} is paused.");
                    break;
                case ServiceControllerStatus.ContinuePending:
                    logger.Warn($"{localWorkerServiceName} is resumng...");
                    break;
                default:
                    logger.Error(new ArgumentOutOfRangeException(nameof(localWorkerStatus), "Invalid service state"));
                    break;
            }
        }

        private static void InstallAndStartLocalWorker()
        {
            const string localWorkerServiceName = Constants.LocalWorkerServiceName;
            const string localWorkerExePath =
                @"..\..\..\..\Workers\OJS.Workers.LocalWorker\bin\Debug\OJS.Workers.LocalWorker.exe";

            try
            {
                logger.Info($"Attempting to install and start the {localWorkerServiceName}");

                ServicesHelper.InstallAndStart(localWorkerServiceName, localWorkerExePath);

                logger.Info($"{localWorkerServiceName} installed and started successfully.");
            }
            catch (Exception ex)
            {
                logger.Error(
                    $"An exception was thrown while attempting to install and start the {localWorkerServiceName}", ex);
            }
        }

        private static void StartLocalWorker()
        {
            const string localWorkerServiceName = Constants.LocalWorkerServiceName;
            try
            {
                logger.Info($"Attempting to start the {localWorkerServiceName}");

                ServicesHelper.StartService(localWorkerServiceName);

                logger.Info($"{localWorkerServiceName} started successfully.");
            }
            catch (Exception ex)
            {
                logger.Error($"An exception was thrown while attempting to start the {localWorkerServiceName}", ex);
            }
        }
    }
}