namespace OJS.LocalWorkerMonitoring
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
            try
            {
                EncryptionHelper.EncryptAppConfigSections(Constants.AppSettingsConfigSectionName);
            }
            catch (Exception ex)
            {
                logger.Warn("Cannot encrypt App.config", ex);
            }

            var timer = new Timer { Interval = IntervalInMilliseconds };
            timer.Elapsed += this.OnTimerElapsed;
            timer.Start();

            logger.Info($"{nameof(LocalWorkerMonitoringService)} started.");
        }

        protected override void OnStop()
        {
            try
            {
                EncryptionHelper.DecryptAppConfigSections(Constants.AppSettingsConfigSectionName);
            }
            catch (Exception ex)
            {
                logger.Warn("Cannot decrypt App.config", ex);
            }

            logger.Info($"{nameof(LocalWorkerMonitoringService)} stopped.");
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs args)
        {
            const string localWorkerName = Constants.LocalWorkerServiceName;

            try
            {
                var localWorkerState = ServicesHelper.GetServiceState(localWorkerName);
                switch (localWorkerState)
                {
                    case ServiceState.NotFound:
                        logger.Warn($"{localWorkerName} is not found.");
                        InstallAndStartLocalWorker();
                        break;
                    case ServiceState.Running:
                        this.totalChecksCount = 0;
                        this.failsToStartCount = 0;
                        this.numberOfTotalChecksBeforeSendingEmail = NumberOfTotalChecksBeforeSendingEmail;
                        this.numberOfFailedStartsBeforeSendingEmail = NumberOfFailedStartsBeforeSendingEmail;
                        break;
                    case ServiceState.StartPending:
                        logger.Info($"{localWorkerName} is starting...");
                        break;
                    case ServiceState.ContinuePending:
                        logger.Warn($"{localWorkerName} is resuming...");
                        break;
                    case ServiceState.StopPending:
                        logger.Warn($"{localWorkerName} is stopping...");
                        StartLocalWorker();
                        break;
                    case ServiceState.Stopped:
                        logger.Warn($"{localWorkerName} is stopped.");
                        StartLocalWorker();
                        break;
                    case ServiceState.PausePending:
                        logger.Warn($"{localWorkerName} is pausing...");
                        StartLocalWorker();
                        break;
                    case ServiceState.Paused:
                        logger.Warn($"{localWorkerName} is paused.");
                        StartLocalWorker();
                        break;
                    case ServiceState.Unknown:
                        throw new ArgumentException($"{localWorkerName} is in unknown state.");
                    default:
                        throw new ArgumentOutOfRangeException(nameof(localWorkerState));
                }
            }
            catch (Exception ex)
            {
                this.failsToStartCount++;
                logger.Error($"An exception was thrown while attempting to start the {localWorkerName}", ex);
            }

            this.totalChecksCount++;

            if (this.failsToStartCount <= this.numberOfFailedStartsBeforeSendingEmail &&
                this.totalChecksCount <= this.numberOfTotalChecksBeforeSendingEmail)
            {
                return;
            }

            var messageTitle = $"{localWorkerName} cannot keep Running state";
            var messageBody =
                $"{localWorkerName} has started and stopped consecutively more than {this.totalChecksCount} times.";

            if (this.failsToStartCount > this.numberOfFailedStartsBeforeSendingEmail)
            {
                messageTitle = $"{localWorkerName} failed to start";
                messageBody = $"{localWorkerName} failed to start more than {this.failsToStartCount} times.";
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
            logger.Info($"Attempting to install the {Constants.LocalWorkerServiceName}...");

            ServicesHelper.InstallService(Constants.LocalWorkerServiceName, Settings.LocalWorkerServiceExecutablePath);

            logger.Info($"{Constants.LocalWorkerServiceName} installed successfully.");

            StartLocalWorker();
        }
    }
}