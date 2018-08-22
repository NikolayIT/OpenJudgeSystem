namespace OJS.Workers.Jobs
{
    using System.Collections.Concurrent;

    using log4net;

    using OJS.Workers.ExecutionStrategies;
    using OJS.Workers.Jobs.Models;

    public abstract class BaseJobStrategy<T> : IJobStrategy<T>
    {
        protected ILog Logger { get; private set; }

        protected ConcurrentQueue<T> SubmissionsForProcessing { get; private set; }

        protected object SharedLockObject { get; private set; }

        public virtual void Initialize(ILog logger, ConcurrentQueue<T> queue, object sharedLockObject = null)
        {
            this.Logger = logger;
            this.SubmissionsForProcessing = queue;
            this.SharedLockObject = sharedLockObject;
        }

        public virtual void BeforeExecute()
        {
        }


        public abstract SubmissionDto RetrieveSubmission();

        public abstract void ProcessEcexutionResult(ExecutionResult executionResult);

        public abstract void OnError(SubmissionDto submissionDto);
    }
}