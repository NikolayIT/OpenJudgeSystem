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
            this.Logger.Info($"{Constants.LocalWorkerServiceName} starting...");

            this.DependencyContainer = this.GetDependencyContainer();

            this.SpawnSubmissionProcessorsAndThreads();

            this.BeforeStartingThreads();

            this.StartThreads();

            this.Logger.Info($"{Constants.LocalWorkerServiceName} started.");
        }

        protected override void OnStop()
        {
            this.Logger.Info($"{Constants.LocalWorkerServiceName} stopping...");

            this.StopSubmissionProcessors();

            Thread.Sleep(this.TimeBeforeAbortingThreadsInMilliseconds);

            this.AbortThreads();

            this.Logger.Info($"{Constants.LocalWorkerServiceName} stopped.");
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

        private void SpawnSubmissionProcessorsAndThreads()
        {
            var submissionsForProcessing = new ConcurrentQueue<TSubmission>();
            var sharedLockObject = new object();

            for (var i = 1; i <= Settings.ThreadsCount; i++)
            {
                var submissionProcessor = new SubmissionProcessor<TSubmission>(
                    name: $"SP #{i}",
                    dependencyContainer: this.DependencyContainer,
                    submissionsForProcessing: submissionsForProcessing,
                    portNumber: Settings.GanacheCliDefaultPortNumber + i,
                    sharedLockObject: sharedLockObject);

                var thread = new Thread(submissionProcessor.Start)
                {
                    Name = $"{nameof(Thread)} #{i}"
                };

                this.submissionProcessors.Add(submissionProcessor);
                this.threads.Add(thread);
            }
        }

        private void StartThreads()
        {
            foreach (var thread in this.threads)
            {
                this.Logger.InfoFormat($"Starting {thread.Name}...");
                thread.Start();
                this.Logger.InfoFormat($"{thread.Name} started.");
                Thread.Sleep(234);
            }
        }

        private void StopSubmissionProcessors()
        {
            foreach (var submissionProcessor in this.submissionProcessors)
            {
                submissionProcessor.Stop();
                this.Logger.InfoFormat($"{submissionProcessor.Name} stopped.");
            }
        }

        private void AbortThreads()
        {
            foreach (var thread in this.threads)
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