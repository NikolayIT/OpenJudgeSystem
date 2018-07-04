namespace OJS.Workers.LocalWorkerMonitoring
{
    using System;
    using System.ServiceProcess;
    using System.Timers;

    using log4net;

    using OJS.Common.Helpers;
    using OJS.Common.Models;
    using OJS.Services.Common.Emails;
    using OJS.Workers.Common;

    internal class LocalWorkerMonitoringService : ServiceBase
    {
        private const int IntervalInMilliseconds = 60000;
        private const int NumberOfFailedStartsBeforeSendingEmail = 5;
        private const int NumberOfTotalChecksBeforeSendingEmail = 10;

        private int numberOfFailedStartsBeforeSendingEmail;
        private int numberOfTotalChecksBeforeSendingEmail;

        private int totalChecksCount;
        private int failsToStartCount;

        private readonly IEmailSenderService emailSender;
        private readonly string devEmail;
        private static ILog logger;

        public LocalWorkerMonitoringService(
            ILog loggerInstance,
            IEmailSenderService emailSender,
            string devEmail)
        {
            this.emailSender = emailSender;
            this.devEmail = devEmail;
            logger = loggerInstance;
            this.numberOfFailedStartsBeforeSendingEmail = NumberOfFailedStartsBeforeSendingEmail;
            this.numberOfTotalChecksBeforeSendingEmail = NumberOfTotalChecksBeforeSendingEmail;
        }

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
            const string serviceName = Constants.LocalWorkerServiceName;

            try
            {
                var localWorkerState = ServicesHelper.GetServiceState(serviceName);
                switch (localWorkerState)
                {
                    case ServiceState.NotFound:
                        logger.Warn($"{serviceName} is not found.");
                        InstallAndStartLocalWorker();
                        break;
                    case ServiceState.Running:
                        this.totalChecksCount = 0;
                        this.failsToStartCount = 0;
                        this.numberOfTotalChecksBeforeSendingEmail = NumberOfTotalChecksBeforeSendingEmail;
                        this.numberOfFailedStartsBeforeSendingEmail = NumberOfFailedStartsBeforeSendingEmail;
                        break;
                    case ServiceState.StartPending:
                        logger.Info($"{serviceName} is starting...");
                        break;
                    case ServiceState.ContinuePending:
                        logger.Warn($"{serviceName} is resuming...");
                        break;
                    case ServiceState.StopPending:
                        logger.Warn($"{serviceName} is stopping...");
                        StartLocalWorker();
                        break;
                    case ServiceState.Stopped:
                        logger.Warn($"{serviceName} is stopped.");
                        StartLocalWorker();
                        break;
                    case ServiceState.PausePending:
                        logger.Warn($"{serviceName} is pausing...");
                        StartLocalWorker();
                        break;
                    case ServiceState.Paused:
                        logger.Warn($"{serviceName} is paused.");
                        StartLocalWorker();
                        break;
                    case ServiceState.Unknown:
                        throw new ArgumentException($"{serviceName} is in unknown state.");
                    default:
                        throw new ArgumentOutOfRangeException(nameof(localWorkerState));
                }
            }
            catch (Exception ex)
            {
                this.failsToStartCount++;
                logger.Error($"An exception was thrown while attempting to start the {serviceName}", ex);
            }

            this.totalChecksCount++;

            if (this.failsToStartCount <= this.numberOfFailedStartsBeforeSendingEmail &&
                this.totalChecksCount <= this.numberOfTotalChecksBeforeSendingEmail)
            {
                return;
            }

            var messageTitle = $"{serviceName} cannot keep Running state";
            var messageBody = $"{serviceName} has started and stopped consecutively more than {this.totalChecksCount} times.";

            if (this.failsToStartCount > this.numberOfFailedStartsBeforeSendingEmail)
            {
                messageTitle = $"{serviceName} failed to start";
                messageBody = $"{serviceName} failed to start more than {this.failsToStartCount} times.";
            }

            try
            {
                this.emailSender.SendEmail(
                    this.devEmail,
                    messageTitle,
                    messageBody);

                this.numberOfFailedStartsBeforeSendingEmail *= 2;
                this.numberOfTotalChecksBeforeSendingEmail *= 2;
            }
            catch (Exception ex)
            {
                logger.Error($"An exception was thrown while sending email to {this.devEmail}", ex);
            }
        }

        private static void StartLocalWorker()
        {
            logger.Info($"Attempting to start the {Constants.LocalWorkerServiceName}...");

            ServicesHelper.StartService(Constants.LocalWorkerServiceName);

            logger.Info($"{Constants.LocalWorkerServiceName} started successfully.");
        }

        private static void InstallAndStartLocalWorker()
        {
            const string localWorkerExePath =
                @"..\..\..\..\Workers\OJS.Workers.LocalWorker\bin\Debug\OJS.Workers.LocalWorker.exe";

            logger.Info($"Attempting to install the {Constants.LocalWorkerServiceName}...");

            ServicesHelper.InstallService(Constants.LocalWorkerServiceName, localWorkerExePath);

            logger.Info($"{Constants.LocalWorkerServiceName} installed successfully.");

            StartLocalWorker();
        }
    }
}