namespace OJS.Workers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.ServiceProcess;
    using System.Threading;

    using log4net;

    using OJS.Common;
    using OJS.Workers.Common;
    using OJS.Workers.Jobs;

    public class LocalWorkerServiceBase<T> : ServiceBase
    {
        private readonly ILog logger;
        private readonly ICollection<Thread> threads;
        private readonly ICollection<IJob> jobs;

        private IDependencyContainer dependencyContainer;

        protected LocalWorkerServiceBase()
        {
            this.logger = LogManager.GetLogger(
                Assembly.GetEntryAssembly(),
                Constants.LocalWorkerServiceLogName);

            this.logger.Info("LocalWorkerService initializing...");

            this.threads = new List<Thread>();
            this.jobs = new List<IJob>();

            this.logger.Info("LocalWorkerService initialized.");
        }

        protected override void OnStart(string[] args)
        {
            this.logger.Info("LocalWorkerService starting...");

            this.dependencyContainer = this.GetDependencyContainer();

            this.logger.Info("LocalWorkerService starting...");

            this.SpawnJobsAndThreads(this.jobs, this.threads, new ConcurrentQueue<T>());

            this.CreateExecutionStrategiesWorkingDirectory();

            this.BeforeStartingThreads(this.logger);

            this.StartThreads(this.threads);

            this.logger.Info("LocalWorkerService started.");
        }

        protected override void OnStop()
        {
            this.logger.Info("LocalWorkerService stopping...");

            this.StopJobs(this.jobs);

            this.StopThreads(this.threads);

            this.logger.Info("LocalWorkerService stopped.");
        }

        protected virtual void BeforeStartingThreads(ILog loggerToUse)
        {
        }

        protected virtual IDependencyContainer GetDependencyContainer() =>
            throw new InvalidOperationException(
                $"{nameof(this.GetDependencyContainer)} method required but not implemented in derived service");

        private void SpawnJobsAndThreads(
            ICollection<IJob> jobsToSpawn,
            ICollection<Thread> threadsToSpawn,
            ConcurrentQueue<T> submissionsForProcessing)
        {
            var sharedLockObject = new object();

            for (var i = 1; i <= Settings.ThreadsCount; i++)
            {
                var job = new SubmissionJob<T>(
                    $"Job №{i}",
                    submissionsForProcessing,
                    sharedLockObject);

                var thread = new Thread(() => job.Start(this.dependencyContainer)) { Name = $"Thread №{i}" };

                jobsToSpawn.Add(job);
                threadsToSpawn.Add(thread);
            }
        }

        private void StartThreads(IEnumerable<Thread> threadsToStarts)
        {
            foreach (var thread in threadsToStarts)
            {
                this.logger.InfoFormat($"Starting {thread.Name}...");
                thread.Start();
                this.logger.InfoFormat($"{thread.Name} started.");
                Thread.Sleep(234);
            }
        }

        private void StopJobs(IEnumerable<IJob> jobsToStop)
        {
            foreach (var job in jobsToStop)
            {
                job.Stop();
                this.logger.InfoFormat($"{job.Name} stopped.");
            }
        }

        private void StopThreads(IEnumerable<Thread> threadsToStop)
        {
            foreach (var thread in threadsToStop)
            {
                thread.Abort();
                this.logger.InfoFormat($"{thread.Name} aborted.");
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
    }
}