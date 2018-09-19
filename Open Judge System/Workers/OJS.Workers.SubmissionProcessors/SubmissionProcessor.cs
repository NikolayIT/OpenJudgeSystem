namespace OJS.Workers.Jobs
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    using log4net;

    using OJS.Workers.Common;
    using OJS.Workers.ExecutionStrategies;
    using OJS.Workers.Jobs.Helpers;
    using OJS.Workers.Jobs.Models;

    using ExecutionContext = ExecutionStrategies.ExecutionContext;

    public class SubmissionJob<TSubmission> : IJob
    {
        private readonly object sharedLockObject;
        private readonly ILog logger;
        private readonly IDependencyContainer dependencyContainer;
        private readonly ConcurrentQueue<TSubmission> submissionsForProcessing;
        private readonly int portNumber;

        private IJobStrategy<TSubmission> jobStrategy;
        private bool stopping;

        public SubmissionJob(
            string name,
            IDependencyContainer dependencyContainer,
            ConcurrentQueue<TSubmission> submissionsForProcessing,
            int portnumber,
            object sharedLockObject)
        {
            this.Name = name;

            this.logger = LogManager.GetLogger(name);
            this.logger.Info("SubmissionJob initializing...");

            this.stopping = false;

            this.dependencyContainer = dependencyContainer;
            this.submissionsForProcessing = submissionsForProcessing;
            this.portNumber = portnumber;
            this.sharedLockObject = sharedLockObject;


            this.logger.Info("SubmissionJob initialized.");
        }

        public string Name { get; set; }

        public void Start()
        {
            this.logger.Info("SubmissionJob starting...");

            while (!this.stopping)
            {
                using (this.dependencyContainer.BeginDefaultScope())
                {
                    this.jobStrategy = this.dependencyContainer.GetInstance<IJobStrategy<TSubmission>>();

                    this.jobStrategy.Initialize(this.logger, this.submissionsForProcessing, this.sharedLockObject);

                    var submission = this.GetSubmissionForProcessing();

                    if (submission != null)
                    {
                        this.ProcessSubmission(submission);
                    }
                    else
                    {
                        Thread.Sleep(this.jobStrategy.JobLoopWaitTimeInMilliseconds);
                    }
                }
            }

            this.logger.Info("SubmissionJob stopped.");
        }

        public void Stop()
        {
            this.stopping = true;
        }

        private SubmissionModel GetSubmissionForProcessing()
        {
            try
            {
                return this.jobStrategy.RetrieveSubmission();
            }
            catch (Exception exception)
            {
                this.logger.Fatal("Unable to get submission for processing.", exception);
                throw;
            }
        }

        private void ProcessSubmission(SubmissionModel submission)
        {
            try
            {
                this.logger.Info($"Work on submission #{submission.Id} started.");

                var executionStrategy = this.CreateExecutinStrategy(submission);

                this.BeforeExecute(submission);

                var executionResult = this.ExecuteSubmission(executionStrategy, submission);

                this.logger.Info($"Work on submission #{submission.Id} ended.");

                this.ProcessExecutionResult(executionResult, submission);

                this.logger.Info($"Submission #{submission.Id} successfully processed.");
            }
            catch
            {
                this.jobStrategy.OnError(submission);
            }
        }

        private IExecutionStrategy CreateExecutinStrategy(SubmissionModel submission)
        {
            try
            {
                return SubmissionJobHelper.CreateExecutionStrategy(
                    submission.ExecutionStrategyType,
                    this.portNumber);
            }
            catch (Exception ex)
            {
                this.logger.Error(
                    $"{nameof(SubmissionJobHelper.CreateExecutionStrategy)} has thrown an Exception: ", ex);

                submission.ProcessingComment = $"Exception in creating execution strategy: {ex.Message}";
                throw;
            }
        }

        private void BeforeExecute(SubmissionModel submission)
        {
            try
            {
                this.jobStrategy.BeforeExecute();
            }
            catch (Exception ex)
            {
                this.logger.Error(
                    $"{nameof(this.jobStrategy.BeforeExecute)} on submission #{submission.Id} has thrown an exception:",
                    ex);

                submission.ProcessingComment = $"Exception before executing the submission: {ex.Message}";
                throw;
            }
        }

        private ExecutionResult ExecuteSubmission(IExecutionStrategy executionStrategy, SubmissionModel submission)
        {
            try
            {
                var context = new ExecutionContext
                {
                    SubmissionId = submission.Id,
                    AdditionalCompilerArguments = submission.AdditionalCompilerArguments,
                    CheckerAssemblyName = submission.CheckerAssemblyName,
                    CheckerParameter = submission.CheckerParameter,
                    CheckerTypeName = submission.CheckerTypeName,
                    FileContent = submission.FileContent,
                    AllowedFileExtensions = submission.AllowedFileExtensions,
                    CompilerType = submission.CompilerType,
                    MemoryLimit = submission.MemoryLimit,
                    TimeLimit = submission.TimeLimit,
                    TaskSkeleton = submission.TaskSkeleton,
                    Tests = submission.Tests
                };

                var executionResult = executionStrategy.SafeExecute(context);

                return executionResult;
            }
            catch (Exception ex)
            {
                this.logger.Error(
                    $"{nameof(executionStrategy.SafeExecute)} on submission #{submission.Id} has thrown an exception:",
                    ex);

                submission.ProcessingComment = $"Exception in executing the submission: {ex.Message}";
                throw;
            }
        }

        private void ProcessExecutionResult(ExecutionResult executionResult, SubmissionModel submission)
        {
            try
            {
                this.jobStrategy.ProcessExecutionResult(executionResult);
            }
            catch (Exception ex)
            {
                this.logger.Error(
                    $"{nameof(this.ProcessExecutionResult)} on submission #{submission.Id} has thrown an exception:",
                    ex);

                submission.ProcessingComment = $"Exception in processing submission: {ex.Message}";
                throw;
            }
        }
    }
}