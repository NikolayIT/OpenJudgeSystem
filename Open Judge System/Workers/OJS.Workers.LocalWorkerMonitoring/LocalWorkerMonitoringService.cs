namespace OJS.Workers.LocalWorkerMonitoring
{
    using System;
    using System.ServiceProcess;
    using System.Timers;

    using log4net;

    using OJS.Workers.Common;
    using OJS.Workers.Common.ServiceInstaller.Models;

    using ServiceInstaller = Common.ServiceInstaller.ServiceInstaller;

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
            var waitTimeout = TimeSpan.FromMilliseconds(IntervalInMilliseconds);
            const string localWorkerServiceName = Constants.LocalWorkerServiceName;
            var localWorkerState = ServiceInstaller.GetServiceStatus(localWorkerServiceName);
            switch (localWorkerState)
            {
                case ServiceState.Running:
                    break;
                case ServiceState.Unknown:
                    logger.Error($"Unable to retrive the state of the {localWorkerServiceName}.");
                    break;
                case ServiceState.NotFound:
                    logger.Warn($"the {localWorkerServiceName} is not found");
                    try
                    {
                        logger.Info($"Attempting to install and start the {localWorkerServiceName}");
                        ServiceInstaller.InstallAndStart(
                            localWorkerServiceName,
                            localWorkerServiceName,
                            @"C:\OpenJudgeSystem\Open Judge System\Workers\OJS.Workers.LocalWorker\bin\Debug\OJS.Workers.LocalWorker.exe");
                        logger.Info($"{localWorkerServiceName} installed and started successfully.");
                    }
                    catch (Exception ex)
                    {
                        logger.Error(
                            $"An exception was thrown while attempting to install and start the {localWorkerServiceName}", ex);
                    }
                    break;
                case ServiceState.Stopped:
                    logger.Warn($"the {localWorkerServiceName} is stopped.");
                    using (var serviceController = new ServiceController(localWorkerServiceName))
                    {
                        try
                        {
                            logger.Info($"Attempting to start the {localWorkerServiceName}");
                            serviceController.Start();
                            serviceController.WaitForStatus(ServiceControllerStatus.Running, waitTimeout);
                            logger.Info($"{localWorkerServiceName} installed and started successfully.");
                        }
                        catch (Exception ex)
                        {
                            logger.Error(
                                $"An exception was thrown while attempting to start the {localWorkerServiceName}", ex);
                        }
                    }
                    
                    break;
                case ServiceState.StartPending:
                    break;
                case ServiceState.StopPending:
                case ServiceState.ContinuePending:
                case ServiceState.PausePending:
                case ServiceState.Paused:
                    goto case ServiceState.Stopped;
                default:
                    logger.Error(new ArgumentOutOfRangeException(nameof(localWorkerState), "Invalid service state"));
                    break;
            }
        }
    }
}