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

    public class SubmissionJob<T> : IJob
    {
        private readonly object sharedLockObject;

        private readonly ILog logger;

        private readonly ConcurrentQueue<T> submissionsForProcessing;

        private bool stopping;

        public SubmissionJob(
            string name,
            ConcurrentQueue<T> submissionsForProcessing,
            object sharedLockObject)
        {
            this.Name = name;

            this.logger = LogManager.GetLogger(name);
            this.logger.Info("SubmissionJob initializing...");

            this.stopping = false;

            this.submissionsForProcessing = submissionsForProcessing;
            this.sharedLockObject = sharedLockObject;

            this.logger.Info("SubmissionJob initialized.");
        }

        public string Name { get; set; }

        public void Start(IDependencyContainer dependencyContainer)
        {
            this.logger.Info("SubmissionJob starting...");
            
            while (!this.stopping)
            {
                using (dependencyContainer.BeginDefaultScope())
                {
                    var jobStrategy = dependencyContainer.GetInstance<IJobStrategy<T>>();

                    jobStrategy.Initialize(this.logger, this.submissionsForProcessing, this.sharedLockObject);

                    SubmissionModel submission;

                    try
                    {
                        submission = jobStrategy.RetrieveSubmission();
                    }
                    catch (Exception exception)
                    {
                        this.logger.Fatal("Unable to get submission for processing.", exception);
                        throw;
                    }

                    if (submission != null)
                    {
                        IExecutionStrategy executionStrategy;
                        ExecutionResult executionResult;

                        this.logger.Info($"Work on submission №{submission.Id} started.");

                        try
                        {
                            executionStrategy = SubmissionJobHelper.CreateExecutionStrategy(
                                submission.ExecutionStrategyType);
                        }
                        catch (Exception ex)
                        {
                            this.logger.Error(
                                $"{nameof(SubmissionJobHelper.CreateExecutionStrategy)} has thrown an Exception: ", ex);

                            submission.ProcessingComment = $"Exception in creating execution strategy: {ex.Message}";
                            jobStrategy.OnError(submission);
                            continue;
                        }

                        try
                        {
                            jobStrategy.BeforeExecute();

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

                            executionResult = executionStrategy.SafeExecute(context);
                        }
                        catch (Exception ex)
                        {
                            this.logger.Error(
                                $"{nameof(executionStrategy.SafeExecute)} on submission №{submission.Id} has thrown an exception:",
                                ex);

                            submission.ProcessingComment = $"Exception in executing the submission: {ex.Message}";
                            jobStrategy.OnError(submission);
                            continue;
                        }

                        this.logger.Info($"Work on submission №{submission.Id} ended.");

                        try
                        {
                            jobStrategy.ProcessEcexutionResult(executionResult);
                        }
                        catch (Exception ex)
                        {
                            this.logger.Error(
                                $"{nameof(jobStrategy.ProcessEcexutionResult)} on submission №{submission.Id} has thrown an exception:",
                                ex);

                            submission.ProcessingComment = $"Exception in processing submission: {ex.Message}";
                            jobStrategy.OnError(submission);
                            continue;
                        }

                        this.logger.Info($"Submission №{submission.Id} successfully processed.");
                    }
                    else
                    {
                        Thread.Sleep(jobStrategy.JobLoopWaitTimeInMilliseconds);
                    }
                }
            }

            this.logger.Info("SubmissionJob stopped.");
        }

        public void Stop()
        {
            this.stopping = true;
        }
    }
}