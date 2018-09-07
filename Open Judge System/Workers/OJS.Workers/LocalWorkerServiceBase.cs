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

    public class LocalWorkerServiceBase<TSubmission> : ServiceBase
    {
        private readonly ICollection<Thread> threads;
        private readonly ICollection<IJob> jobs;

        protected LocalWorkerServiceBase()
        {
            var loggerAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();

            this.Logger = LogManager.GetLogger(loggerAssembly, Constants.LocalWorkerServiceLogName);

            this.threads = new List<Thread>();
            this.jobs = new List<IJob>();
        }

        protected ILog Logger { get; }

        protected IDependencyContainer DependencyContainer { get; private set; }

        protected override void OnStart(string[] args)
        {
            this.Logger.Info("LocalWorkerService starting...");

            this.DependencyContainer = this.GetDependencyContainer();

            this.SpawnJobsAndThreads(this.jobs, this.threads, new ConcurrentQueue<TSubmission>());

            this.BeforeStartingThreads();

            this.StartThreads(this.threads);

            this.Logger.Info("LocalWorkerService started.");
        }

        protected override void OnStop()
        {
            this.Logger.Info("LocalWorkerService stopping...");

            this.StopJobs(this.jobs);

            Thread.Sleep(this.TimeBeforeAbortingThreadsInMilliseconds);

            this.StopThreads(this.threads);

            this.Logger.Info("LocalWorkerService stopped.");
        }

        protected virtual void BeforeStartingThreads()
        {
            this.CreateExecutionStrategiesWorkingDirectory();
        }

        protected virtual IDependencyContainer GetDependencyContainer() =>
            throw new InvalidOperationException(
                $"{nameof(this.GetDependencyContainer)} method required but not implemented in derived service");

        protected virtual int TimeBeforeAbortingThreadsInMilliseconds =>
            Constants.DefaultTimeBeforeAbortingThreadsInMilliseconds;

        private void SpawnJobsAndThreads(
            ICollection<IJob> jobsToSpawn,
            ICollection<Thread> threadsToSpawn,
            ConcurrentQueue<TSubmission> submissionsForProcessing)
        {
            var sharedLockObject = new object();

            for (var i = 1; i <= Settings.ThreadsCount; i++)
            {
                var job = new SubmissionJob<TSubmission>(
                    $"Job #{i}",
                    this.DependencyContainer,
                    submissionsForProcessing,
                    Settings.GanacheCliDefaultPortNumber + i,
                    sharedLockObject);

                var thread = new Thread(job.Start) { Name = $"Thread #{i}" };

                jobsToSpawn.Add(job);
                threadsToSpawn.Add(thread);
            }
        }

        private void StartThreads(IEnumerable<Thread> threadsToStarts)
        {
            foreach (var thread in threadsToStarts)
            {
                this.Logger.InfoFormat($"Starting {thread.Name}...");
                thread.Start();
                this.Logger.InfoFormat($"{thread.Name} started.");
                Thread.Sleep(234);
            }
        }

        private void StopJobs(IEnumerable<IJob> jobsToStop)
        {
            foreach (var job in jobsToStop)
            {
                job.Stop();
                this.Logger.InfoFormat($"{job.Name} stopped.");
            }
        }

        private void StopThreads(IEnumerable<Thread> threadsToStop)
        {
            foreach (var thread in threadsToStop)
            {
                thread.Abort();
                this.Logger.InfoFormat($"{thread.Name} aborted.");
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