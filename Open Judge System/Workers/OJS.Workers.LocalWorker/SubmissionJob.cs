namespace OJS.Workers.LocalWorker
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;

    using log4net;

    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Workers.ExecutionStrategies;
    using OJS.Workers.ExecutionStrategies.SqlStrategies.MySql;
    using OJS.Workers.ExecutionStrategies.SqlStrategies.SqlServerLocalDb;

    using ExecutionContext = OJS.Workers.ExecutionStrategies.ExecutionContext;

    public class SubmissionJob : IJob
    {
        private readonly ILog logger;

        private readonly ConcurrentQueue<int> submissionsForProcessing;

        private bool stopping;

        public SubmissionJob(string name, ConcurrentQueue<int> submissionsForProcessing)
        {
            this.Name = name;

            this.logger = LogManager.GetLogger(name);
            this.logger.Info("SubmissionJob initializing...");

            this.stopping = false;

            this.submissionsForProcessing = submissionsForProcessing;

            this.logger.Info("SubmissionJob initialized.");
        }

        public string Name { get; set; }

        public void Start()
        {
            this.logger.Info("SubmissionJob starting...");
            while (!this.stopping)
            {
                var data = new OjsData();
                Submission submission = null;
                int submissionId;

                try
                {
                    var retrievedSubmissionSuccessfully = false;
                    lock (this.submissionsForProcessing)
                    {
                        if (this.submissionsForProcessing.IsEmpty)
                        {
                            var submissions = data.Submissions
                                .All()
                                .Where(x => !x.Processed && !x.Processing)
                                .OrderBy(x => x.Id)
                                .Select(x => x.Id)
                                .ToList();

                            submissions.ForEach(this.submissionsForProcessing.Enqueue);
                        }

                        retrievedSubmissionSuccessfully = this.submissionsForProcessing.TryDequeue(out submissionId);

                        if (retrievedSubmissionSuccessfully)
                        {
                            this.logger.InfoFormat("Submission №{0} retrieved from data store successfully", submissionId);
                            submission = data.Submissions.GetById(submissionId);
                            if (!submission.Processing)
                            {
                                submission.Processing = true;
                                data.SaveChanges();
                            }                     
                        }
                    }

                    if (!retrievedSubmissionSuccessfully || submission == null)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }                   
                }
                catch (Exception exception)
                {
                    this.logger.FatalFormat("Unable to get submission for processing. Exception: {0}", exception);
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
                    submission.ProcessingComment = $"Exception in ProcessSubmission: {exception.Message}";
                }

                try
                {
                    this.CalculatePointsForSubmission(submission);
                }
                catch (Exception exception)
                {
                    this.logger.ErrorFormat("CalculatePointsForSubmission on submission №{0} has thrown an exception: {1}", submission.Id, exception);
                    submission.ProcessingComment = $"Exception in CalculatePointsForSubmission: {exception.Message}";
                }

                submission.Processed = true;
                submission.Processing = false;

                try
                {
                    data.ParticipantScores.SaveParticipantScore(submission);
                }
                catch (Exception exception)
                {
                    this.logger.ErrorFormat("SaveParticipantScore on submission №{0} has thrown an exception: {1}", submission.Id, exception);
                    submission.ProcessingComment = $"Exception in SaveParticipantScore: {exception.Message}";
                }

                try
                {
                    submission.CacheTestRuns();
                }
                catch (Exception exception)
                {
                    this.logger.ErrorFormat("CacheTestRuns on submission №{0} has thrown an exception: {1}", submission.Id, exception);
                    submission.ProcessingComment = $"Exception in CacheTestRuns: {exception.Message}";
                }

                try
                {
                    data.SaveChanges();
                }
                catch (Exception exception)
                {
                    this.logger.ErrorFormat("Unable to save changes to the submission №{0}! Exception: {1}", submission.Id, exception);
                }

                this.logger.InfoFormat("Submission №{0} successfully processed", submissionId);
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
                case CompilerType.MsBuildLibrary:
                    return Settings.MsBuildExecutablePath;
                case CompilerType.CPlusPlusGcc:
                case CompilerType.CPlusPlusZip:
                    return Settings.CPlusPlusGccCompilerPath;
                case CompilerType.Java:
                case CompilerType.JavaZip:
                case CompilerType.JavaInPlaceCompiler:
                    return Settings.JavaCompilerPath;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        private void ProcessSubmission(Submission submission)
        {
            // TODO: Check for N+1 queries problem
            this.logger.InfoFormat("Work on submission №{0} started.", submission.Id);

            var executionStrategy = this.CreateExecutionStrategy(submission.SubmissionType.ExecutionStrategyType);
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
                TaskSkeleton = submission.Problem.SolutionSkeleton,
                Tests = submission.Problem.Tests.AsQueryable().Select(x =>
                        new TestContext
                        {
                            Id = x.Id,
                            Input = x.InputDataAsString,
                            Output = x.OutputDataAsString,
                            IsTrialTest = x.IsTrialTest
                        }).ToList(),
            };

            ExecutionResult executionResult;
            try
            {
                executionResult = executionStrategy.Execute(context);
            }
            catch (Exception exception)
            {
                this.logger.ErrorFormat("executionStrategy.Execute on submission №{0} has thrown an exception: {1}", submission.Id, exception);
                submission.ProcessingComment = $"Exception in executionStrategy.Execute: {exception.Message}";
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
                    CheckerComment = testResult.CheckerDetails.Comment,
                    ExpectedOutputFragment = testResult.CheckerDetails.ExpectedOutputFragment,
                    UserOutputFragment = testResult.CheckerDetails.UserOutputFragment,
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
            if (submission.Problem.Tests.Count == 0 || submission.TestsWithoutTrialTestsCount == 0)
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
                case ExecutionStrategyType.CPlusPlusCompileExecuteAndCheckExecutionStrategy:
                    executionStrategy = new CPlusPlusCompileExecuteAndCheckExecutionStrategy(GetCompilerPath);
                    break;
                case ExecutionStrategyType.CPlusPlusZipFileExecutionStrategy:
                    executionStrategy = new CPlusPlusZipFileExecutionStrategy(GetCompilerPath);
                    break;
                case ExecutionStrategyType.CSharpTestRunner:
                    executionStrategy = new CSharpTestRunnerExecutionStrategy(GetCompilerPath);
                    break;
                case ExecutionStrategyType.CSharpUnitTestsExecutionStrategy:
                    executionStrategy = new CSharpUnitTestsExecutionStrategy(Settings.NUnitConsoleRunnerPath, GetCompilerPath);
                    break;
                case ExecutionStrategyType.CSharpProjectTestsExecutionStrategy:
                    executionStrategy = new CSharpProjectTestsExecutionStrategy(Settings.NUnitConsoleRunnerPath, GetCompilerPath);
                    break;
                case ExecutionStrategyType.CSharpPerformanceProjectTestsExecutionStrategy:
                    executionStrategy = new CSharpPerformanceProjectTestsExecutionStrategy(Settings.NUnitConsoleRunnerPath, GetCompilerPath);
                    break;
                case ExecutionStrategyType.CSharpAspProjectTestsExecutionStrategy:
                    executionStrategy = new CSharpAspProjectTestsExecutionStrategy(Settings.NUnitConsoleRunnerPath, GetCompilerPath);
                    break;
                case ExecutionStrategyType.RubyExecutionStrategy:
                    executionStrategy = new RubyExecutionStrategy(Settings.RubyPath);
                    break;
                case ExecutionStrategyType.JavaPreprocessCompileExecuteAndCheck:
                    executionStrategy = new JavaPreprocessCompileExecuteAndCheckExecutionStrategy(
                        Settings.JavaExecutablePath,
                        GetCompilerPath);
                    break;
                case ExecutionStrategyType.JavaZipFileCompileExecuteAndCheck:
                    executionStrategy = new JavaZipFileCompileExecuteAndCheckExecutionStrategy(
                        Settings.JavaExecutablePath,
                        GetCompilerPath);
                    break;
                case ExecutionStrategyType.JavaProjectTestsExecutionStrategy:
                    executionStrategy = new JavaProjectTestsExecutionStrategy(
                        Settings.JavaExecutablePath,
                        GetCompilerPath,
                        Settings.JavaLibsPath);
                    break;
                case ExecutionStrategyType.JavaUnitTestsExecutionStrategy:
                    executionStrategy = new JavaUnitTestsExecutionStrategy(
                        Settings.JavaExecutablePath,
                        GetCompilerPath,
                        Settings.JavaLibsPath);
                    break;
                case ExecutionStrategyType.JavaSpringAndHibernateProjectExecutionStrategy:
                    executionStrategy = new JavaSpringAndHibernateProjectExecutionStrategy(
                        Settings.JavaExecutablePath,
                        GetCompilerPath,
                        Settings.JavaLibsPath,
                        Settings.MavenPath);
                    break;
                case ExecutionStrategyType.NodeJsPreprocessExecuteAndCheck:
                    executionStrategy = new NodeJsPreprocessExecuteAndCheckExecutionStrategy(
                        Settings.NodeJsExecutablePath,
                        Settings.UnderscoreModulePath,
                        Settings.NodeJsBaseTimeUsedInMilliseconds * 2,
                        Settings.NodeJsBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.NodeJsPreprocessExecuteAndRunUnitTestsWithMocha:
                    executionStrategy = new NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy(
                        Settings.NodeJsExecutablePath,
                        Settings.MochaModulePath,
                        Settings.ChaiModulePath,
                        Settings.SinonModulePath,
                        Settings.SinonChaiModulePath,
                        Settings.UnderscoreModulePath,
                        Settings.NodeJsBaseTimeUsedInMilliseconds,
                        Settings.NodeJsBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.NodeJsZipPreprocessExecuteAndRunUnitTestsWithDomAndMocha:
                    executionStrategy = new NodeJsZipPreprocessExecuteAndRunUnitTestsWithDomAndMochaExecutionStrategy(
                        Settings.NodeJsExecutablePath,
                        Settings.MochaModulePath,
                        Settings.ChaiModulePath,
                        Settings.JsDomModulePath,
                        Settings.JQueryModulePath,
                        Settings.HandlebarsModulePath,
                        Settings.SinonModulePath,
                        Settings.SinonChaiModulePath,
                        Settings.UnderscoreModulePath,
                        Settings.BrowserifyModulePath,
                        Settings.BabelifyModulePath,
                        Settings.Es2015ImportPluginPath,
                        Settings.NodeJsBaseTimeUsedInMilliseconds,
                        Settings.NodeJsBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.NodeJsPreprocessExecuteAndRunJsDomUnitTests:
                    executionStrategy = new NodeJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategy(
                        Settings.NodeJsExecutablePath,
                        Settings.MochaModulePath,
                        Settings.ChaiModulePath,
                        Settings.JsDomModulePath,
                        Settings.JQueryModulePath,
                        Settings.HandlebarsModulePath,
                        Settings.SinonModulePath,
                        Settings.SinonChaiModulePath,
                        Settings.UnderscoreModulePath,
                        Settings.NodeJsBaseTimeUsedInMilliseconds,
                        Settings.NodeJsBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategy:
                    executionStrategy = new NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategy(
                        Settings.NodeJsExecutablePath,
                        Settings.MochaModulePath,
                        Settings.ChaiModulePath,
                        Settings.JsDomModulePath,
                        Settings.JQueryModulePath,
                        Settings.HandlebarsModulePath,
                        Settings.SinonModulePath,
                        Settings.SinonChaiModulePath,
                        Settings.UnderscoreModulePath,
                        Settings.NodeJsBaseTimeUsedInMilliseconds,
                        Settings.NodeJsBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.NodeJsExecuteAndRunAsyncJsDomTestsWithReactExecutionStrategy:
                    executionStrategy = new NodeJsExecuteAndRunAsyncJsDomTestsWithReactExecutionStrategy(
                        Settings.NodeJsExecutablePath,
                        Settings.MochaModulePath,
                        Settings.ChaiModulePath,
                        Settings.JsDomModulePath,
                        Settings.JQueryModulePath,
                        Settings.HandlebarsModulePath,
                        Settings.SinonJsDomModulePath,
                        Settings.SinonModulePath,
                        Settings.SinonChaiModulePath,
                        Settings.UnderscoreModulePath,
                        Settings.BabelCoreModulePath,
                        Settings.ReactJsxPluginPath,
                        Settings.ReactModulePath,
                        Settings.ReactDomModulePath,
                        Settings.NodeJsBaseTimeUsedInMilliseconds,
                        Settings.NodeJsBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.NodeJsZipExecuteHtmlAndCssStrategy:
                    executionStrategy = new NodeJsZipExecuteHtmlAndCssStrategy(
                        Settings.NodeJsExecutablePath,
                        Settings.MochaModulePath,
                        Settings.ChaiModulePath,
                        Settings.SinonModulePath,
                        Settings.SinonChaiModulePath,
                        Settings.JsDomModulePath,
                        Settings.JQueryModulePath,
                        Settings.UnderscoreModulePath,
                        Settings.BootstrapModulePath,
                        Settings.BootstrapCssPath,
                        Settings.NodeJsBaseTimeUsedInMilliseconds,
                        Settings.NodeJsBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.PythonExecuteAndCheck:
                    executionStrategy = new PythonExecuteAndCheckExecutionStrategy(Settings.PythonExecutablePath);
                    break;
                case ExecutionStrategyType.PhpCgiExecuteAndCheck:
                    executionStrategy = new PhpCgiExecuteAndCheckExecutionStrategy(Settings.PhpCgiExecutablePath);
                    break;
                case ExecutionStrategyType.PhpCliExecuteAndCheck:
                    executionStrategy = new PhpCliExecuteAndCheckExecutionStrategy(Settings.PhpCliExecutablePath);
                    break;
                case ExecutionStrategyType.SqlServerLocalDbPrepareDatabaseAndRunQueries:
                    executionStrategy = new SqlServerLocalDbPrepareDatabaseAndRunQueriesExecutionStrategy(
                        Settings.SqlServerLocalDbMasterDbConnectionString,
                        Settings.SqlServerLocalDbRestrictedUserId,
                        Settings.SqlServerLocalDbRestrictedUserPassword);
                    break;
                case ExecutionStrategyType.SqlServerLocalDbRunQueriesAndCheckDatabase:
                    executionStrategy = new SqlServerLocalDbRunQueriesAndCheckDatabaseExecutionStrategy(
                        Settings.SqlServerLocalDbMasterDbConnectionString,
                        Settings.SqlServerLocalDbRestrictedUserId,
                        Settings.SqlServerLocalDbRestrictedUserPassword);
                    break;
                case ExecutionStrategyType.SqlServerLocalDbRunSkeletonRunQueriesAndCheckDatabase:
                    executionStrategy = new SqlServerLocalDbRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategy(
                        Settings.SqlServerLocalDbMasterDbConnectionString,
                        Settings.SqlServerLocalDbRestrictedUserId,
                        Settings.SqlServerLocalDbRestrictedUserPassword);
                    break;
                case ExecutionStrategyType.MySqlPrepareDatabaseAndRunQueries:
                    executionStrategy = new MySqlPrepareDatabaseAndRunQueriesExecutionStrategy(
                        Settings.MySqlSysDbConnectionString,
                        Settings.MySqlRestrictedUserId,
                        Settings.MySqlRestrictedUserPassword);
                    break;
                case ExecutionStrategyType.MySqlRunQueriesAndCheckDatabase:
                    executionStrategy = new MySqlRunQueriesAndCheckDatabaseExecutionStrategy(
                        Settings.MySqlSysDbConnectionString,
                        Settings.MySqlRestrictedUserId,
                        Settings.MySqlRestrictedUserPassword);
                    break;
                case ExecutionStrategyType.MySqlRunSkeletonRunQueriesAndCheckDatabase:
                    executionStrategy = new MySqlRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategy(
                        Settings.MySqlSysDbConnectionString,
                        Settings.MySqlRestrictedUserId,
                        Settings.MySqlRestrictedUserPassword);
                    break;
                case ExecutionStrategyType.DoNothing:
                    executionStrategy = new DoNothingExecutionStrategy();
                    break;
                case ExecutionStrategyType.RemoteExecution:
                    executionStrategy = new RemoteExecutionStrategy();
                    break;
                case ExecutionStrategyType.CheckOnly:
                    executionStrategy = new CheckOnlyExecutionStrategy();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return executionStrategy;
        }
    }
}
