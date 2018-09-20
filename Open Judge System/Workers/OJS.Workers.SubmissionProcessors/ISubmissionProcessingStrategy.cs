namespace OJS.Workers.SubmissionProcessors
{
    using System.Collections.Concurrent;

    using log4net;

    using OJS.Workers.ExecutionStrategies;
    using OJS.Workers.SubmissionProcessors.Models;

    public interface ISubmissionProcessingStrategy<TSubmission>
    {
        int JobLoopWaitTimeInMilliseconds { get; }

        void Initialize(
            ILog logger,
            ConcurrentQueue<TSubmission> submissionsForProcessing,
            object sharedLockObject);

        SubmissionModel RetrieveSubmission();

        void BeforeExecute();

        void ProcessExecutionResult(ExecutionResult executionResult);

        void OnError(SubmissionModel submission);
    }
}