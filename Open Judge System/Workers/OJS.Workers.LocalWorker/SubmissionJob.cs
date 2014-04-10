namespace OJS.Workers.LocalWorker
{
    using System;
    using System.Linq;
    using System.Threading;

    using log4net;

    using OJS.Common;
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Workers.ExecutionStrategies;

    using ExecutionContext = OJS.Workers.ExecutionStrategies.ExecutionContext;

    public class SubmissionJob : IJob
    {
        private readonly ILog logger;

        private readonly SynchronizedHashtable processingSubmissionIds;

        private bool stopping;

        public SubmissionJob(string name, SynchronizedHashtable processingSubmissionIds)
        {
            this.Name = name;

            this.logger = LogManager.GetLogger(name);
            this.logger.Info("SubmissionJob initializing...");

            this.stopping = false;
            this.processingSubmissionIds = processingSubmissionIds;

            this.logger.Info("SubmissionJob initialized.");
        }

        public string Name { get; set; }

        public void Start()
        {
            this.logger.Info("SubmissionJob starting...");
            while (!this.stopping)
            {
                IOjsData data = new OjsData();
                Submission submission;
                try
                {
                    submission = data.Submissions.GetSubmissionForProcessing();
                }
                catch (Exception exception)
                {
                    this.logger.FatalFormat("Unable to get submission for processing. Exception: {0}", exception);
                    throw;
                }

                if (submission == null)
                {
                    // No submission available. Wait 1 second and try again.
                    Thread.Sleep(1000);
                    continue;
                }

                if (!this.processingSubmissionIds.Add(submission.Id))
                {
                    // Other thread is processing the same submission. Wait the other thread to set Processing to true and then try again.
                    Thread.Sleep(100);
                    continue;
                }

                try
                {
                    submission.Processing = true;
                    data.SaveChanges();
                }
                catch (Exception exception)
                {
                    this.logger.Error("Unable to set dbSubmission.Processing to true! Exception: {0}", exception);
                    this.processingSubmissionIds.Remove(submission.Id);
                    throw;
                }

                submission.ProcessingComment = null;
                try
                {
                    data.TestRuns.DeleteBySubmissionId(submission.Id);
                    this.ProcessSubmission(submission);
                    data.SaveChanges();
                }
                catch (Exception exception)
                {
                    this.logger.ErrorFormat("ProcessSubmission on submission №{0} has thrown an exception: {1}", submission.Id, exception);
                    submission.ProcessingComment = string.Format("Exception in ProcessSubmission: {0}", exception.Message);
                }

                try
                {
                    this.CalculatePointsForSubmission(submission);
                }
                catch (Exception exception)
                {
                    this.logger.ErrorFormat("CalculatePointsForSubmission on submission №{0} has thrown an exception: {1}", submission.Id, exception);
                    submission.ProcessingComment = string.Format("Exception in CalculatePointsForSubmission: {0}", exception.Message);
                }

                submission.Processed = true;
                submission.Processing = false;

                try
                {
                    data.SaveChanges();
                }
                catch (Exception exception)
                {
                    this.logger.ErrorFormat("Unable to save changes to the submission №{0}! Exception: {1}", submission.Id, exception);
                }

                // Next line removes the submission from the list. Fixes problem with retesting submissions.
                this.processingSubmissionIds.Remove(submission.Id);
            }

            this.logger.Info("SubmissionJob stopped.");
        }

        public void Stop()
        {
            this.stopping = true;
        }

        private static string GetCompilerPath(CompilerType type)
        {
            switch (type)
            {
                case CompilerType.None:
                    return null;
                case CompilerType.CSharp:
                    return Settings.CSharpCompilerPath;
                case CompilerType.MsBuild:
                    return Settings.MsBuildExecutablePath;
                case CompilerType.CPlusPlusGcc:
                    return Settings.CPlusPlusGccCompilerPath;
                case CompilerType.Java:
                    throw new NotImplementedException("Compiler not supported.");
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        private void ProcessSubmission(Submission submission)
        {
            // TODO: Check for N+1 queries problem
            this.logger.InfoFormat("Work on submission №{0} started.", submission.Id);

            IExecutionStrategy executionStrategy = this.CreateExecutionStrategy(submission.SubmissionType.ExecutionStrategyType);
            var context = new ExecutionContext
            {
                SubmissionId = submission.Id,
                AdditionalCompilerArguments = submission.SubmissionType.AdditionalCompilerArguments,
                CheckerAssemblyName = submission.Problem.Checker.DllFile,
                CheckerParameter = submission.Problem.Checker.Parameter,
                CheckerTypeName = submission.Problem.Checker.ClassName,
                FileContent = submission.Content,
                AllowedFileExtensions = submission.SubmissionType.AllowedFileExtensions,
                CompilerType = submission.SubmissionType.CompilerType,
                MemoryLimit = submission.Problem.MemoryLimit,
                TimeLimit = submission.Problem.TimeLimit,
            };

            context.Tests = submission.Problem.Tests.ToList().Select(x => new TestContext
            {
                Id = x.Id,
                Input = x.InputDataAsString,
                Output = x.OutputDataAsString,
                IsTrialTest = x.IsTrialTest
            });

            ExecutionResult executionResult;
            try
            {
                executionResult = executionStrategy.Execute(context);
            }
            catch (Exception exception)
            {
                this.logger.ErrorFormat("executionStrategy.Execute on submission №{0} has thrown an exception: {1}", submission.Id, exception);
                submission.ProcessingComment = string.Format("Exception in executionStrategy.Execute: {0}", exception.Message);
                return;
            }

            submission.IsCompiledSuccessfully = executionResult.IsCompiledSuccessfully;
            submission.CompilerComment = executionResult.CompilerComment;

            if (!executionResult.IsCompiledSuccessfully)
            {
                return;
            }

            foreach (var testResult in executionResult.TestResults)
            {
                var testRun = new TestRun
                {
                    CheckerComment = testResult.CheckerComment,
                    ExecutionComment = testResult.ExecutionComment,
                    MemoryUsed = testResult.MemoryUsed,
                    ResultType = testResult.ResultType,
                    TestId = testResult.Id,
                    TimeUsed = testResult.TimeUsed,
                };
                submission.TestRuns.Add(testRun);
            }

            this.logger.InfoFormat("Work on submission №{0} ended.", submission.Id);
        }

        private void CalculatePointsForSubmission(Submission submission)
        {
            // Internal joke: submission.Points = new Random().Next(0, submission.Problem.MaximumPoints + 1) + Weather.Instance.Today("Sofia").IsCloudy ? 10 : 0;            
            if (submission.Problem.Tests.Count == 0)
            {
                submission.Points = 0;
            }
            else
            {
                submission.Points = (submission.CorrectTestRunsWithoutTrialTestsCount * submission.Problem.MaximumPoints) / submission.TestsWithoutTrialTestsCount;
            }
        }

        private IExecutionStrategy CreateExecutionStrategy(ExecutionStrategyType type)
        {
            IExecutionStrategy executionStrategy;
            switch (type)
            {
                case ExecutionStrategyType.CompileExecuteAndCheck:
                    executionStrategy = new CompileExecuteAndCheckExecutionStrategy(GetCompilerPath);
                    break;
                case ExecutionStrategyType.NodeJsPreprocessExecuteAndCheck:
                    executionStrategy = new NodeJsPreprocessExecuteAndCheckExecutionStrategy(Settings.NodeJsExecutablePath);
                    break;
                case ExecutionStrategyType.DoNothing:
                    executionStrategy = new DoNothingExecutionStrategy();
                    break;
                case ExecutionStrategyType.RemoteExecution:
                    executionStrategy = new RemoteExecutionStrategy();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return executionStrategy;
        }
    }
}
