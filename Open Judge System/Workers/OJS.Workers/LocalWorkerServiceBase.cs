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

    using OJS.Workers.Common;
    using OJS.Workers.SubmissionProcessors;

    public class LocalWorkerServiceBase<TSubmission> : ServiceBase
    {
        private readonly ICollection<Thread> threads;
        private readonly ICollection<ISubmissionProcessor> submissionProcessors;

        protected LocalWorkerServiceBase()
        {
            var loggerAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();

            this.Logger = LogManager.GetLogger(loggerAssembly, Constants.LocalWorkerServiceLogName);

            this.threads = new List<Thread>();
            this.submissionProcessors = new List<ISubmissionProcessor>();
        }

        protected ILog Logger { get; }

        protected IDependencyContainer DependencyContainer { get; private set; }

        protected override void OnStart(string[] args)
        {
            this.Logger.Info("LocalWorkerService starting...");

            this.DependencyContainer = this.GetDependencyContainer();

            this.SpawnSubmissionProcessorsAndThreads(
                this.submissionProcessors,
                this.threads,
                new ConcurrentQueue<TSubmission>());

            this.BeforeStartingThreads();

            this.StartThreads(this.threads);

            this.Logger.Info("LocalWorkerService started.");
        }

        protected override void OnStop()
        {
            this.Logger.Info("LocalWorkerService stopping...");

            this.StopSubmissionProcessors(this.submissionProcessors);

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

        private void SpawnSubmissionProcessorsAndThreads(
            ICollection<ISubmissionProcessor> processors,
            ICollection<Thread> threadsToSpawn,
            ConcurrentQueue<TSubmission> submissionsForProcessing)
        {
            var sharedLockObject = new object();

            for (var i = 1; i <= Settings.ThreadsCount; i++)
            {
                var submissionProcessor = new SubmissionProcessor<TSubmission>(
                    $"{nameof(SubmissionProcessor<TSubmission>)} #{i}",
                    this.DependencyContainer,
                    submissionsForProcessing,
                    Settings.GanacheCliDefaultPortNumber + i,
                    sharedLockObject);

                var thread = new Thread(submissionProcessor.Start)
                {
                    Name = $"{nameof(Thread)} #{i}"
                };

                processors.Add(submissionProcessor);
                threadsToSpawn.Add(thread);
            }
        }

        private void StartThreads(IEnumerable<Thread> threadsToStart)
        {
            foreach (var thread in threadsToStart)
            {
                this.Logger.InfoFormat($"Starting {thread.Name}...");
                thread.Start();
                this.Logger.InfoFormat($"{thread.Name} started.");
                Thread.Sleep(234);
            }
        }

        private void StopSubmissionProcessors(IEnumerable<ISubmissionProcessor> processors)
        {
            foreach (var processor in processors)
            {
                processor.Stop();
                this.Logger.InfoFormat($"{processor.Name} stopped.");
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
            var path = Constants.ExecutionStrategiesWorkingDirectoryPath;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}