namespace OJS.Workers.Jobs
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    using log4net;

    using OJS.Workers.ExecutionStrategies;
    using OJS.Workers.Jobs.Helpers;
    using OJS.Workers.Jobs.Models;

    using SimpleInjector;
    using SimpleInjector.Lifestyles;

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

        public void Start(Container container)
        {
            this.logger.Info("SubmissionJob starting...");
            
            while (!this.stopping)
            {
                using (ThreadScopedLifestyle.BeginScope(container))
                {
                    var jobStrategy = container.GetInstance<IJobStrategy<T>>();

                    jobStrategy.Initialize(this.logger, this.submissionsForProcessing, this.sharedLockObject);

                    SubmissionDto submission;

                    try
                    {
                        submission = jobStrategy.RetrieveSubmission();
                    }
                    catch (Exception exception)
                    {
                        this.logger.FatalFormat("Unable to get submission for processing. Exception: {0}", exception);
                        throw;
                    }

                    if (submission != null)
                    {
                        this.logger.InfoFormat("Work on submission №{0} started.", submission.Id);

                        var executionStrategy = SubmissionJobHelper.CreateExecutionStrategy(
                            submission.ExecutionStrategyType);

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

                        ExecutionResult executionResult;

                        try
                        {
                            jobStrategy.BeforeExecute();

                            executionResult = executionStrategy.SafeExecute(context);
                        }
                        catch (Exception ex)
                        {
                            this.logger.Error($"executionStrategy.Execute on submission №{submission.Id} has thrown an exception:", ex);
                            submission.ProcessingComment = $"Exception in executionStrategy.Execute: {ex.Message}";
                            jobStrategy.OnError(submission);
                            continue;
                        }

                        this.logger.InfoFormat("Work on submission №{0} ended.", submission.Id);

                        try
                        {
                            jobStrategy.ProcessEcexutionResult(executionResult);
                        }
                        catch (Exception ex)
                        {
                            this.logger.ErrorFormat("ProcessExecutionResult on submission №{0} has thrown an exception: {1}", submission.Id, ex);
                            submission.ProcessingComment = $"Exception in ProcessSubmission: {ex.Message}";
                            jobStrategy.OnError(submission);
                            continue;
                        }

                        this.logger.InfoFormat("Submission №{0} successfully processed", submission.Id);
                    }
                    else
                    {
                        Thread.Sleep(1000);
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