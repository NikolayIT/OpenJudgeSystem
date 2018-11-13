namespace OJS.LocalWorker
{
    using System;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Data.Participants;
    using OJS.Services.Data.ParticipantScores;
    using OJS.Services.Data.Submissions;
    using OJS.Services.Data.SubmissionsForProcessing;
    using OJS.Services.Data.TestRuns;
    using OJS.Workers.Common;
    using OJS.Workers.Common.Models;
    using OJS.Workers.ExecutionStrategies.Models;
    using OJS.Workers.SubmissionProcessors;
    using OJS.Workers.SubmissionProcessors.Models;

    public class OjsSubmissionProcessingStrategy : SubmissionProcessingStrategy<int>
    {
        private readonly ISubmissionsDataService submissionsData;
        private readonly ITestRunsDataService testRunsData;
        private readonly IParticipantsDataService participantsData;
        private readonly IParticipantScoresDataService participantScoresData;
        private readonly ISubmissionsForProcessingDataService submissionsForProcessingData;

        private Submission submission;
        private SubmissionForProcessing submissionForProcessing;

        public OjsSubmissionProcessingStrategy(
            ISubmissionsDataService submissionsData,
            ITestRunsDataService testRunsData,
            IParticipantsDataService participantsData,
            IParticipantScoresDataService participantScoresData,
            ISubmissionsForProcessingDataService submissionsForProcessingData)
        {
            this.submissionsData = submissionsData;
            this.testRunsData = testRunsData;
            this.participantsData = participantsData;
            this.participantScoresData = participantScoresData;
            this.submissionsForProcessingData = submissionsForProcessingData;
        }

        public override IOjsSubmission RetrieveSubmission()
        {
            lock (this.SubmissionsForProcessing)
            {
                if (this.SubmissionsForProcessing.IsEmpty)
                {
                    this.submissionsForProcessingData
                        .GetAllUnprocessed()
                        .OrderBy(x => x.Id)
                        .Select(x => x.SubmissionId)
                        .ToList()
                        .ForEach(this.SubmissionsForProcessing.Enqueue);
                }

                var isSubmissionRetrieved = this.SubmissionsForProcessing.TryDequeue(out var submissionId);

                if (!isSubmissionRetrieved)
                {
                    return null;
                }

                this.Logger.InfoFormat($"Submission #{submissionId} retrieved from data store successfully");

                this.submission = this.submissionsData.GetById(submissionId);

                this.submissionForProcessing = this.submissionsForProcessingData.GetBySubmission(submissionId);

                if (this.submission == null || this.submissionForProcessing == null)
                {
                    this.Logger.Error($"Cannot retrieve submission #{submissionId} from database");
                    return null;
                }

                this.SetSubmissionToProcessing();
            }

            return this.GetSubmissionModel();
        }

        public override void BeforeExecute()
        {
            this.submission.ProcessingComment = null;
            this.testRunsData.DeleteBySubmission(this.submission.Id);
        }

        public override void OnError(IOjsSubmission submissionModel)
        {
            this.submission.ProcessingComment = submissionModel.ProcessingComment;

            this.UpdateResults();
        }

        protected override void ProcessTestsExecutionResult(IExecutionResult<TestResult> executionResult)
        {
            this.submission.IsCompiledSuccessfully = executionResult.IsCompiledSuccessfully;
            this.submission.CompilerComment = executionResult.CompilerComment;

            if (!executionResult.IsCompiledSuccessfully)
            {
                this.UpdateResults();
                return;
            }

            foreach (var testResult in executionResult.Results)
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
                    TimeUsed = testResult.TimeUsed
                };

                this.submission.TestRuns.Add(testRun);
            }

            this.submissionsData.Update(this.submission);

            this.UpdateResults();
        }

        private void UpdateResults()
        {
            this.CalculatePointsForSubmission();

            this.SaveParticipantScore();

            this.CacheTestRuns();

            this.SetSubmissionToProcessed();
        }

        private void CalculatePointsForSubmission()
        {
            try
            {
                // Internal joke: submission.Points = new Random().Next(0, submission.Problem.MaximumPoints + 1) + Weather.Instance.Today("Sofia").IsCloudy ? 10 : 0;
                if (this.submission.Problem.Tests.Count == 0 || this.submission.TestsWithoutTrialTestsCount == 0)
                {
                    this.submission.Points = 0;
                }
                else
                {
                    var coefficient = (double)this.submission.CorrectTestRunsWithoutTrialTestsCount /
                        this.submission.TestsWithoutTrialTestsCount;

                    this.submission.Points = (int)(coefficient * this.submission.Problem.MaximumPoints);
                }
            }
            catch (Exception ex)
            {
                this.Logger.ErrorFormat(
                    "CalculatePointsForSubmission on submission #{0} has thrown an exception: {1}",
                    this.submission.Id,
                    ex);

                this.submission.ProcessingComment = $"Exception in CalculatePointsForSubmission: {ex.Message}";
            }
        }

        private void SaveParticipantScore()
        {
            try
            {
                if (this.submission.ParticipantId == null || this.submission.ProblemId == null)
                {
                    return;
                }

                var participant = this.participantsData
                    .GetByIdQuery(this.submission.ParticipantId.Value)
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

                lock (this.SharedLockObject)
                {
                    existingScore = this.participantScoresData.GetByParticipantIdProblemIdAndIsOfficial(
                        this.submission.ParticipantId.Value,
                        this.submission.ProblemId.Value,
                        participant.IsOfficial);

                    if (existingScore == null)
                    {
                        this.participantScoresData.AddBySubmissionByUsernameAndIsOfficial(
                            this.submission,
                            participant.UserName,
                            participant.IsOfficial);

                        return;
                    }
                }

                if (this.submission.Points > existingScore.Points ||
                    this.submission.Id == existingScore.SubmissionId)
                {
                    this.participantScoresData.UpdateBySubmissionAndPoints(
                        existingScore,
                        this.submission.Id,
                        this.submission.Points);
                }
            }
            catch (Exception ex)
            {
                this.Logger.ErrorFormat(
                    "SaveParticipantScore on submission #{0} has thrown an exception: {1}",
                    this.submission.Id,
                    ex);

                this.submission.ProcessingComment = $"Exception in SaveParticipantScore: {ex.Message}";
            }
        }

        private void SetSubmissionToProcessed()
        {
            try
            {
                this.submission.Processed = true;
                this.submissionForProcessing.Processed = true;
                this.submissionForProcessing.Processing = false;

                this.submissionsData.Update(this.submission);
                this.submissionsForProcessingData.Update(this.submissionForProcessing);
            }
            catch (Exception ex)
            {
                this.Logger.ErrorFormat(
                    "Unable to save changes to the submission #{0}! Exception: {1}",
                    this.submission.Id,
                    ex);
            }
        }

        private void SetSubmissionToProcessing()
        {
            try
            {
                this.submissionForProcessing.Processed = false;
                this.submissionForProcessing.Processing = true;

                this.submissionsForProcessingData.Update(this.submissionForProcessing);
            }
            catch (Exception ex)
            {
                this.Logger.ErrorFormat(
                    "Unable to set submission #{0} to processing! Exception: {1}",
                    this.submission.Id,
                    ex);
            }
        }

        private void CacheTestRuns()
        {
            try
            {
                this.submission.CacheTestRuns();
            }
            catch (Exception ex)
            {
                this.Logger.ErrorFormat(
                    "CacheTestRuns on submission #{0} has thrown an exception: {1}",
                    this.submission.Id,
                    ex);

                this.submission.ProcessingComment = $"Exception in CacheTestRuns: {ex.Message}";
            }
        }

        private IOjsSubmission GetSubmissionModel() => new OjsSubmission<TestsInputModel>()
        {
            Id = this.submission.Id,
            AdditionalCompilerArguments = this.submission.SubmissionType.AdditionalCompilerArguments,
            AllowedFileExtensions = this.submission.SubmissionType.AllowedFileExtensions,
            FileContent = this.submission.Content,
            CompilerType = this.submission.SubmissionType.CompilerType,
            TimeLimit = this.submission.Problem.TimeLimit,
            MemoryLimit = this.submission.Problem.MemoryLimit,
            ExecutionStrategyType = this.submission.SubmissionType.ExecutionStrategyType,
            ExecutionType = ExecutionType.TestsExecution,
            Input = new TestsInputModel
            {
                TaskSkeleton = this.submission.Problem.SolutionSkeleton,
                CheckerParameter = this.submission.Problem.Checker.Parameter,
                CheckerAssemblyName = this.submission.Problem.Checker.DllFile,
                CheckerTypeName = this.submission.Problem.Checker.ClassName,
                Tests = this.submission.Problem.Tests
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
            }
        };
    }
}