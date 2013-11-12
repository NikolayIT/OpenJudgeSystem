namespace OJS.Workers.LocalWorker
{
    using System.ServiceProcess;

    using log4net;

    internal class LocalWorkerService : ServiceBase
    {
        private static ILog logger;

        public LocalWorkerService()
        {
            logger = LogManager.GetLogger("LocalWorkerService");
            logger.Info("LocalWorkerService initializing...");
            logger.Info("LocalWorkerService initialized.");
        }

        protected override void OnStart(string[] args)
        {
            logger.Info("LocalWorkerService starting...");
            logger.Info("LocalWorkerService started.");
        }

        protected override void OnStop()
        {
            logger.Info("LocalWorkerService stopping...");
            logger.Info("LocalWorkerService stopped.");
        }
    }
}
