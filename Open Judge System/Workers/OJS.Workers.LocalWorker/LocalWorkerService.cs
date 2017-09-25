namespace OJS.Workers.LocalWorker
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.ServiceProcess;
    using System.Threading;

    using log4net;

    using OJS.Common;
    using OJS.Data;

    internal class LocalWorkerService : ServiceBase
    {
        private static ILog logger;
        private readonly IList<Thread> threads;
        private readonly IList<IJob> jobs;

        public LocalWorkerService()
        {
            logger = LogManager.GetLogger("LocalWorkerService");
            logger.Info("LocalWorkerService initializing...");
            this.ResetAllProcessingSubmissions();
            this.CreateExecutionStrategiesDirectoryInTemp();

            this.threads = new List<Thread>();
            this.jobs = new List<IJob>();
            var submissionsForProcessing = new ConcurrentQueue<int>();

            for (var i = 1; i <= Settings.ThreadsCount; i++)
            {
                var job = new SubmissionJob(string.Format("Job №{0}", i), submissionsForProcessing);
                var thread = new Thread(job.Start) { Name = string.Format("Thread №{0}", i) };
                this.jobs.Add(job);
                this.threads.Add(thread);
            }

            logger.Info("LocalWorkerService initialized.");
        }

        protected override void OnStart(string[] args)
        {
            logger.Info("LocalWorkerService starting...");

            foreach (var thread in this.threads)
            {
                logger.InfoFormat("Starting {0}...", thread.Name);
                thread.Start();
                logger.InfoFormat("{0} started.", thread.Name);
                Thread.Sleep(234);
            }

            logger.Info("LocalWorkerService started.");
        }

        protected override void OnStop()
        {
            logger.Info("LocalWorkerService stopping...");

            foreach (var job in this.jobs)
            {
                job.Stop();
                logger.InfoFormat("{0} stopped.", job.Name);
            }

            Thread.Sleep(10000);

            foreach (var thread in this.threads)
            {
                thread.Abort();
                logger.InfoFormat("{0} aborted.", thread.Name);
            }

            logger.Info("LocalWorkerService stopped.");
        }

        /// <summary>
        /// Sets the Processing property to False for all submissions
        /// thus ensuring that the worker will process them eventually instead
        /// of getting stuck in perpetual "Processing..." state
        /// </summary>
        private void ResetAllProcessingSubmissions()
        {
            using (var data = new OjsData())
            {
                var allProcessingSubmissions = data
                    .SubmissionsForProcessing
                    .All()
                    .Where(s => s.Processing && !s.Processed);

                if (allProcessingSubmissions.Any())
                {
                    try
                    {
                        foreach (var unprocessedSubmission in allProcessingSubmissions)
                        {
                            unprocessedSubmission.Processing = false;
                        }

                        data.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        logger.ErrorFormat("Clearing Processing submissions failed with exception {0}", e.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Creates folder in the Temp directory if not already created,
        /// in which all strategies create their own working directories
        /// making easier the deletion of left-over files by the background job
        /// </summary>
        private void CreateExecutionStrategiesDirectoryInTemp()
        {
            var path = GlobalConstants.ExecutionStrategiesWorkingDirectoryPath;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}