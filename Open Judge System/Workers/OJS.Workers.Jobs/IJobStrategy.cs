namespace OJS.Workers.Jobs
{
    using System.Collections.Concurrent;

    using log4net;

    using OJS.Workers.ExecutionStrategies;
    using OJS.Workers.Jobs.Models;

    public interface IJobStrategy<T>
    {
        void Initialize(ILog logger, ConcurrentQueue<T> queue, object sharedLockObject = null);

        SubmissionModel RetrieveSubmission();

        void BeforeExecute();

        void ProcessEcexutionResult(ExecutionResult executionResult);

        void OnError(SubmissionModel submission);
    }
}