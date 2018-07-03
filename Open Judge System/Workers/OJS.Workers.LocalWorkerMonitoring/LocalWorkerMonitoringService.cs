namespace OJS.Workers.LocalWorkerMonitoring
{
    using System;
    using System.ServiceProcess;
    using System.Timers;

    using log4net;

    using OJS.Common.Helpers;
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

        private async void OnTimerElapsed(object sender, ElapsedEventArgs args)
        {
            const string serviceName = Constants.LocalWorkerServiceName;
            var localWorkerStatus = ServicesHelper.GetServiceStatus(serviceName);

            try
            {
                switch (localWorkerStatus)
                {
                    case null:
                        logger.Warn($"{serviceName} is not found");
                        const string localWorkerExePath =
                            @"..\..\..\..\Workers\OJS.Workers.LocalWorker\bin\Debug\OJS.Workers.LocalWorker.exe";
                        InstallService(serviceName, localWorkerExePath);
                        StartService(serviceName);
                        break;
                    case ServiceControllerStatus.Running:
                        this.totalChecksCount = 0;
                        this.failsToStartCount = 0;
                        this.numberOfTotalChecksBeforeSendingEmail = NumberOfTotalChecksBeforeSendingEmail;
                        this.numberOfFailedStartsBeforeSendingEmail = NumberOfFailedStartsBeforeSendingEmail;
                        break;
                    case ServiceControllerStatus.StartPending:
                        logger.Info($"{serviceName} is starting...");
                        break;
                    case ServiceControllerStatus.StopPending:
                        logger.Warn($"{serviceName} is stopping...");
                        break;
                    case ServiceControllerStatus.Stopped:
                        logger.Warn($"{serviceName} is stopped.");
                        StartService(serviceName);
                        break;
                    case ServiceControllerStatus.PausePending:
                        logger.Warn($"{serviceName} is pausing...");
                        break;
                    case ServiceControllerStatus.Paused:
                        logger.Warn($"{serviceName} is paused.");
                        break;
                    case ServiceControllerStatus.ContinuePending:
                        logger.Warn($"{serviceName} is resuming...");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(localWorkerStatus),"Invalid service status");
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

            var messageTitle = $"{serviceName} cannot keep running";
            var messageBody = $"{messageTitle}. It has started and stopped more than {this.totalChecksCount} times.";

            if (this.failsToStartCount > this.numberOfFailedStartsBeforeSendingEmail)
            {
                messageTitle = $"{serviceName} failed to start";
                messageBody = $"{messageTitle} more than {this.failsToStartCount} times.";
            }

            messageBody += $" The current status of the service is {localWorkerStatus}";

            try
            {
                await this.emailSender.SendEmailAsync(
                    this.devEmail,
                    messageTitle,
                    messageBody);

                this.numberOfFailedStartsBeforeSendingEmail *= 2;
                this.numberOfTotalChecksBeforeSendingEmail *= 2;
            }
            catch (Exception ex)
            {
                logger.Error("An exception was thrown while sending email", ex);
            }
        }

        private static void StartService(string serviceName)
        {
            logger.Info($"Attempting to start the {serviceName}...");

            ServicesHelper.StartService(serviceName);

            logger.Info($"{serviceName} started successfully.");
        }

        private static void InstallService(string serviceName, string serviceExePath)
        {
            logger.Info($"Attempting to install the {serviceName}...");

            ServicesHelper.InstallService(serviceName, serviceExePath);

            logger.Info($"{serviceName} installed successfully.");
        }
    }
}