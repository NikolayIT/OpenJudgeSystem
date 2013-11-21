namespace OJS.Workers.LocalWorker
{
    using System.Collections.Generic;
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
            
            this.threads = new List<Thread>();
            this.jobs = new List<IJob>();
            var processingSubmissionIds = new SynchronizedHashtable();

            for (int i = 1; i <= Settings.ThreadsCount; i++)
            {
                var job = new SubmissionJob(string.Format("Job №{0}", i), processingSubmissionIds);
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
    }
}
