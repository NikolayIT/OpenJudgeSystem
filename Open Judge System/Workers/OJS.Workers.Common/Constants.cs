namespace OJS.Workers.Common
{
    public static class Constants
    {
        public const string LocalWorkerServiceName = "OJS Local Worker Service";
        public const string LocalWorkerMonitoringServiceName = "OJS Local Worker Monitoring Service";

        public const string LocalWorkerServiceLogName = "LocalWorkerService";
        public const string LocalWorkerMonitoringServiceLogName = "LocalWorkerMonitoringService";

        public const int DefaultJobLoopWaitTimeInMilliseconds = 1000;
        public const int DefaultTimeBeforeAbortingThreadsInMilliseconds = 10000;
    }
}