namespace OJS.Workers.LocalWorker
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;

    using log4net;

    using OJS.Data.Models;
    using OJS.Services.Data.Participants;
    using OJS.Services.Data.ParticipantScores;
    using OJS.Services.Data.Submissions;
    using OJS.Services.Data.SubmissionsForProcessing;
    using OJS.Services.Data.TestRuns;
    using OJS.Workers.ExecutionStrategies;
    using OJS.Workers.LocalWorker.Helpers;

    using SimpleInjector.Lifestyles;

    using ExecutionContext = OJS.Workers.ExecutionStrategies.ExecutionContext;

    public class SubmissionJob : IJob
    {
        private readonly object sharedLockObject;
        private readonly int portNumber;

        private readonly ILog logger;

        private readonly ConcurrentQueue<int> submissionsForProcessing;

        private bool stopping;

        public SubmissionJob(
            string name,
            ConcurrentQueue<int> submissionsForProcessing,
            object sharedLockObject,
            int portNumber)
        {
            this.Name = name;

            this.logger = LogManager.GetLogger(name);
            this.logger.Info("SubmissionJob initializing...");

            this.stopping = false;

            this.submissionsForProcessing = submissionsForProcessing;
            this.sharedLockObject = sharedLockObject;
            this.portNumber = portNumber;

            this.logger.Info("SubmissionJob initialized.");
        }

        public string Name { get; set; }

        public void Start()
        {
            this.logger.Info("SubmissionJob starting...");
            var container = Bootstrap.Container;
            while (!this.stopping)
            {
                using (ThreadScopedLifestyle.BeginScope(container))
                {
                    var testRunsData = container.GetInstance<ITestRunsDataService>();
                    var submissionsData = container.GetInstance<ISubmissionsDataService>();
                    var participantsData = container.GetInstance<IParticipantsDataService>();
                    var participantScoresData = container.GetInstance<IParticipantScoresDataService>();
                    var submissionsForProccessingData = container.GetInstance<ISubmissionsForProcessingDataService>();

                    Submission submission = null;
                    SubmissionForProcessing submissionForProcessing = null;
                    bool retrievedSubmissionSuccessfully;
                    try
                    {
                        lock (this.submissionsForProcessing)
                        {
                            if (this.submissionsForProcessing.IsEmpty)
                            {
                                var submissions = submissionsForProccessingData
                                    .GetAllUnprocessed()
                                    .OrderBy(x => x.Id)
                                    .Select(x => x.SubmissionId)
                                    .ToList();

                                submissions.ForEach(this.submissionsForProcessing.Enqueue);
                            }

                            retrievedSubmissionSuccessfully = this.submissionsForProcessing
                                .TryDequeue(out var submissionId);

                            if (retrievedSubmissionSuccessfully)
                            {
                                this.logger
                                    .InfoFormat($"Submission №{submissionId} retrieved from data store successfully");

                                submission = submissionsData.GetById(submissionId);

                                submissionForProcessing = submissionsForProccessingData.GetBySubmission(submissionId);

                                if (submission != null && submissionForProcessing != null)
                                {
                                    submissionForProcessing.Processed = false;
                                    submissionForProcessing.Processing = true;

                                    submissionsForProccessingData.Update(submissionForProcessing);
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        this.logger.FatalFormat("Unable to get submission for processing. Exception: {0}", exception);
                        throw;
                    }

                    if (retrievedSubmissionSuccessfully && submission != null && submissionForProcessing != null)
                    {
                        this.BeginProcessingSubmission(
                            submission,
                            submissionForProcessing,
                            testRunsData,
                            submissionsData,
                            participantsData,
                            participantScoresData);
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

        private void BeginProcessingSubmission(
            Submission submission,
            SubmissionForProcessing submissionForProcessing,
            ITestRunsDataService testRunsData,
            ISubmissionsDataService submissionsData,
            IParticipantsDataService participantsData,
            IParticipantScoresDataService participantScoresData)
        {
            submission.ProcessingComment = null;

            try
            {
                testRunsData.DeleteBySubmission(submission.Id);

                this.ProcessSubmission(submission);

                submissionsData.Update(submission);
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
            submissionForProcessing.Processed = true;
            submissionForProcessing.Processing = false;

            try
            {
                this.SaveParticipantScore(submission, participantsData, participantScoresData);
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
                submissionsData.Update(submission);
            }
            catch (Exception exception)
            {
                this.logger.ErrorFormat("Unable to save changes to the submission №{0}! Exception: {1}", submission.Id, exception);
            }

            this.logger.InfoFormat("Submission №{0} successfully processed", submission.Id);
        }

        private void ProcessSubmission(Submission submission)
        {
            // TODO: Check for N+1 queries problem
            this.logger.InfoFormat("Work on submission №{0} started.", submission.Id);

            var executionStrategy = SubmissionJobHelper.CreateExecutionStrategy(
                submission.SubmissionType.ExecutionStrategyType,
                this.portNumber);

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
                Tests = submission.Problem.Tests
                    .AsQueryable()
                    .Select(t => new TestContext
                    {
                        Id = t.Id,
                        Input = t.InputDataAsString,
                        Output = t.OutputDataAsString,
                        IsTrialTest = t.IsTrialTest,
                        OrderBy = t.OrderBy
                    })
                    .ToList()
            };

            ExecutionResult executionResult;
            try
            {
                executionResult = executionStrategy.SafeExecute(context);
            }
            catch (Exception exception)
            {
                this.logger.Error($"executionStrategy.Execute on submission №{submission.Id} has thrown an exception:", exception);
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

        private void SaveParticipantScore(
            Submission submission,
            IParticipantsDataService participantsData,
            IParticipantScoresDataService participantScoresData)
        {
            if (submission.ParticipantId == null || submission.ProblemId == null)
            {
                return;
            }

            var participant = participantsData
                .GetByIdQuery(submission.ParticipantId.Value)
                .Select(p => new
                {
                    p.IsOfficial,
                    p.User.UserName
                })
                .FirstOrDefault();

            if (participant == null)
            {
                return;
            }

            ParticipantScore existingScore;

            lock (this.sharedLockObject)
            {
                existingScore = participantScoresData.GetByParticipantIdProblemIdAndIsOfficial(
                    submission.ParticipantId.Value,
                    submission.ProblemId.Value,
                    participant.IsOfficial);

                if (existingScore == null)
                {
                    participantScoresData.AddBySubmissionByUsernameAndIsOfficial(
                        submission,
                        participant.UserName,
                        participant.IsOfficial);

                    return;
                }
            }

            if (submission.Points > existingScore.Points ||
                submission.Id == existingScore.SubmissionId)
            {
                participantScoresData.UpdateBySubmissionAndPoints(
                    existingScore,
                    submission.Id,
                    submission.Points);
            }
        }
    }
}