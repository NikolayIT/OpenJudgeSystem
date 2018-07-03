namespace OJS.Workers.LocalWorker
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.ServiceProcess;
    using System.Threading;

    using log4net;

    using OJS.Common;
    using OJS.Common.Helpers;
    using OJS.Services.Business.SubmissionsForProcessing;
    using OJS.Workers.Common;
    using OJS.Workers.Common.AppSettings;

    internal class LocalWorkerService : ServiceBase
    {
        private static ILog logger;
        private readonly ICollection<Thread> threads;
        private readonly ICollection<IJob> jobs;
        private readonly ISubmissionsForProcessingBusinessService submissionsForProcessingBusiness;

        public LocalWorkerService(
            ISubmissionsForProcessingBusinessService submissionsForProcessingBusiness)
        {
            logger = LogManager.GetLogger(Constants.LocalWorkerServiceLogName);
            logger.Info("LocalWorkerService initializing...");

            this.submissionsForProcessingBusiness = submissionsForProcessingBusiness;
            this.threads = new List<Thread>();
            this.jobs = new List<IJob>();

            this.SpawnJobsAndThreads(this.jobs, this.threads, new ConcurrentQueue<int>());

            logger.Info("LocalWorkerService initialized.");
        }

        protected override void OnStart(string[] args)
        {
            logger.Info("LocalWorkerService starting...");

            this.CreateExecutionStrategiesWorkingDirectory();

            this.submissionsForProcessingBusiness.ResetAllProcessingSubmissions(logger);

            this.StartThreads(this.threads);

            this.TryStartMonitoringService();

            logger.Info("LocalWorkerService started.");
        }

        protected override void OnStop()
        {
            logger.Info("LocalWorkerService stopping...");

            this.StopJobs(this.jobs);

            Thread.Sleep(10000);

            this.StopThreads(this.threads);

            logger.Info("LocalWorkerService stopped.");
        }

        private void SpawnJobsAndThreads(
            ICollection<IJob> jobsToSpawn,
            ICollection<Thread> threadsToSpawn,
            ConcurrentQueue<int> submissionsForProcessing)
        {
            var sharedLockObject = new object();

            for (var i = 1; i <= Settings.ThreadsCount; i++)
            {
                var job = new SubmissionJob(
                    $"Job №{i}",
                    submissionsForProcessing,
                    sharedLockObject);

                var thread = new Thread(job.Start) { Name = $"Thread №{i}" };

                jobsToSpawn.Add(job);
                threadsToSpawn.Add(thread);
            }
        }

        private void StartThreads(IEnumerable<Thread> threadsToStarts)
        {
            foreach (var thread in threadsToStarts)
            {
                logger.InfoFormat($"Starting {thread.Name}...");
                thread.Start();
                logger.InfoFormat($"{thread.Name} started.");
                Thread.Sleep(234);
            }
        }

        private void StopJobs(IEnumerable<IJob> jobsToStop)
        {
            foreach (var job in jobsToStop)
            {
                job.Stop();
                logger.InfoFormat($"{job.Name} stopped.");
            }
        }

        private void StopThreads(IEnumerable<Thread> threadsToStop)
        {
            foreach (var thread in threadsToStop)
            {
                thread.Abort();
                logger.InfoFormat($"{thread.Name} aborted.");
            }
        }

        /// <summary>
        /// Creates folder in the Temp directory if not already created,
        /// in which all strategies create their own working directories
        /// making easier the deletion of left-over files by the background job
        /// </summary>
        private void CreateExecutionStrategiesWorkingDirectory()
        {
            var path = GlobalConstants.ExecutionStrategiesWorkingDirectoryPath;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private void TryStartMonitoringService()
        {
            const string monitoringServiceName = Constants.LocalWorkerMonitoringServiceName;

            try
            {
                var serviceStatus = ServicesHelper.GetServiceStatus(monitoringServiceName);
                if (serviceStatus == ServiceControllerStatus.Running)
                {
                    logger.Info($"{monitoringServiceName} is running.");
                    return;
                }

                if (serviceStatus == null)
                {
                    const string monitoringServiceExePath =
                        @"..\..\..\..\OJS.Workers.LocalWorkerMonitoring\bin\Debug\OJS.Workers.LocalWorkerMonitoring.exe";

                    logger.Info($"Attempting to install the {monitoringServiceName}...");

                    ServicesHelper.InstallService(monitoringServiceName, monitoringServiceExePath);

                    logger.Info($"{monitoringServiceName} installed successfully.");
                }

                logger.Info($"Attempting to start the {monitoringServiceName}...");

                ServicesHelper.StartService(monitoringServiceName);

                logger.Info($"{monitoringServiceName} started successfully.");
            }
            catch (Exception ex)
            {
                logger.Error($"An exception was thrown while attempting to start the {monitoringServiceName}", ex);
            }
        }
    }
}