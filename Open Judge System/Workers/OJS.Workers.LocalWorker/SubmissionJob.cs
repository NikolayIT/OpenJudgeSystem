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
        private bool stopping;

        private readonly ILog logger;
        private readonly SynchronizedHashtable processingSubmissionIds;

        public SubmissionJob(string name, SynchronizedHashtable processingSubmissionIds)
        {
            this.Name = name;

            logger = LogManager.GetLogger(name);
            logger.Info("SubmissionJob initializing...");

            this.stopping = false;
            this.processingSubmissionIds = processingSubmissionIds;

            logger.Info("SubmissionJob initialized.");
        }

        public string Name { get; set; }

        public void Start()
        {
            logger.Info("SubmissionJob starting...");
            while (!this.stopping)
            {
                IOjsData data = new OjsData();
                Submission dbSubmission;
                try
                {
                    dbSubmission = data.Submissions.GetSubmissionForProcessing();
                }
                catch (Exception exception)
                {
                    logger.FatalFormat("Unable to get submission for processing. Exception: {0}", exception);
                    throw;
                }

                if (dbSubmission == null)
                {
                    // No submission available. Wait 1 second and try again.
                    Thread.Sleep(1000);
                    continue;
                }

                if (!processingSubmissionIds.Add(dbSubmission.Id))
                {
                    // Other thread is processing the same submission. Wait the other thread to set Processing to true and then try again.
                    Thread.Sleep(100);
                    continue;
                }

                try
                {
                    dbSubmission.Processing = true;
                    data.SaveChanges();
                }
                catch (Exception exception)
                {
                    logger.Error("Unable to set dbSubmission.Processing to true! Exception: {0}", exception);
                    processingSubmissionIds.Remove(dbSubmission.Id);
                    throw;
                }

                data.TestRuns.DeleteBySubmissionId(dbSubmission.Id);
                data.SaveChanges();

                this.ProcessSubmission(dbSubmission);
                dbSubmission.Processed = true;
                dbSubmission.Processing = false;
                try
                {
                    data.SaveChanges();
                }
                catch (Exception exception)
                {
                    logger.Error("Unable to save changes to the submission! Exception: {0}", exception);
                }

                processingSubmissionIds.Remove(dbSubmission.Id);
            }

            logger.Info("SubmissionJob stopped.");
        }

        private void ProcessSubmission(Submission dbSubmission)
        {
            // TODO: Check for N+1 queries problem
            logger.InfoFormat("Work on submission №{0} started.", dbSubmission.Id);

            IExecutionStrategy executionStrategy = this.CreateExecutionStrategy(dbSubmission.SubmissionType.ExecutionStrategyType);
            var context = new ExecutionContext
            {
                AdditionalCompilerArguments = dbSubmission.SubmissionType.AdditionalCompilerArguments,
                CheckerAssemblyName = dbSubmission.Problem.Checker.DllFile,
                CheckerParameter = dbSubmission.Problem.Checker.Parameter,
                CheckerTypeName = dbSubmission.Problem.Checker.ClassName,
                Code = dbSubmission.ContentAsString,
                CompilerType = dbSubmission.SubmissionType.CompilerType,
                MemoryLimit = dbSubmission.Problem.MemoryLimit,
                TimeLimit = dbSubmission.Problem.TimeLimit,
            };

            context.Tests = dbSubmission.Problem.Tests.ToList().Select(x => new TestContext
            {
                Id = x.Id,
                Input = x.InputDataAsString,
                Output = x.OutputDataAsString,
            });

            ExecutionResult executionResult;
            try
            {
                executionResult = executionStrategy.Execute(context);
            }
            catch (Exception exception)
            {
                logger.ErrorFormat("executionStrategy.Execute on submission №{0} has thrown an exception: {1}", dbSubmission.Id, exception);
                dbSubmission.ProcessingComment = string.Format("Exception in executionStrategy.Execute: {0}", exception.Message);
                return;
            }

            dbSubmission.IsCompiledSuccessfully = executionResult.IsCompiledSuccessfully;
            dbSubmission.CompilerComment = executionResult.CompilerComment;

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
                dbSubmission.TestRuns.Add(testRun);
            }

            // Internal joke: dbSubmission.Points = new Random().Next(0, dbSubmission.Problem.MaximumPoints + 1) + Weather.Instance.Today("Sofia").IsCloudy ? 10 : 0;
            if (dbSubmission.Problem.Tests.Count == 0)
            {
                dbSubmission.Points = 0;
            }
            else
            {
                dbSubmission.Points = (dbSubmission.CorrectTestRunsCount * dbSubmission.Problem.MaximumPoints) / dbSubmission.Problem.Tests.Count;
            }

            logger.InfoFormat("Work on submission №{0} ended.", dbSubmission.Id);
        }
        
        public void Stop()
        {
            this.stopping = true;
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
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return executionStrategy;
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
                    throw new NotImplementedException("Compiler not supported.");
                case CompilerType.CPlusPlusGcc:
                    return Settings.CPlusPlusGccCompilerPath;
                case CompilerType.Java:
                    throw new NotImplementedException("Compiler not supported.");
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }
    }
}
