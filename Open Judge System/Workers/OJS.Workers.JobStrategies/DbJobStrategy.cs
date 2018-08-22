﻿namespace OJS.Workers.JobStrategies
{
    using System;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Data.Participants;
    using OJS.Services.Data.ParticipantScores;
    using OJS.Services.Data.Submissions;
    using OJS.Services.Data.SubmissionsForProcessing;
    using OJS.Services.Data.TestRuns;
    using OJS.Workers.ExecutionStrategies;
    using OJS.Workers.Jobs;
    using OJS.Workers.Jobs.Models;

    public class DbJobStrategy : BaseJobStrategy<int>
    {
        private readonly ISubmissionsDataService submissionsData;
        private readonly ITestRunsDataService testRunsData;
        private readonly IParticipantsDataService participantsData;
        private readonly IParticipantScoresDataService participantScoresData;
        private readonly ISubmissionsForProcessingDataService submissionsForProccessingData;

        private Submission submission;
        private SubmissionForProcessing submissionForProcessing;

        public DbJobStrategy(
            ISubmissionsDataService submissionsData,
            ITestRunsDataService testRunsData,
            IParticipantsDataService participantsData,
            IParticipantScoresDataService participantScoresData,
            ISubmissionsForProcessingDataService submissionsForProccessingData)
        {
            this.submissionsData = submissionsData;
            this.testRunsData = testRunsData;
            this.participantsData = participantsData;
            this.participantScoresData = participantScoresData;
            this.submissionsForProccessingData = submissionsForProccessingData;
        }

        public override void ProcessEcexutionResult(ExecutionResult executionResult)
        {
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
                    TimeUsed = testResult.TimeUsed
                };

                this.submission.TestRuns.Add(testRun);
            }

            this.UpdateResults();
        }

        public override void OnError(SubmissionDto submissionDto)
        {
            this.submission.IsCompiledSuccessfully = submissionDto.IsCompiledSuccessfully;
            this.submission.CompilerComment = submissionDto.CompilerComment;
            this.submission.ProcessingComment = submissionDto.ProcessingComment;

            this.UpdateResults();
        }

        public override SubmissionDto RetrieveSubmission()
        {
            bool retrievedSubmissionSuccessfully;

            lock (this.SubmissionsForProcessing)
            {
                if (this.SubmissionsForProcessing.IsEmpty)
                {
                    var submissions = this.submissionsForProccessingData
                        .GetAllUnprocessed()
                        .OrderBy(x => x.Id)
                        .Select(x => x.SubmissionId)
                        .ToList();

                    submissions.ForEach(this.SubmissionsForProcessing.Enqueue);
                }

                retrievedSubmissionSuccessfully = this.SubmissionsForProcessing
                    .TryDequeue(out var submissionId);

                if (retrievedSubmissionSuccessfully)
                {
                    this.Logger.InfoFormat($"Submission №{submissionId} retrieved from data store successfully");

                    this.submission = this.submissionsData.GetById(submissionId);

                    this.submissionForProcessing = this.submissionsForProccessingData.GetBySubmission(submissionId);

                    if (this.submission != null && this.submissionForProcessing != null)
                    {
                        this.submissionForProcessing.Processed = false;
                        this.submissionForProcessing.Processing = true;

                        this.submissionsForProccessingData.Update(this.submissionForProcessing);
                    }
                }
            }

            if (!retrievedSubmissionSuccessfully || this.submission == null || this.submissionForProcessing == null)
            {
                return null;
            }

            return new SubmissionDto
            {
                Id = this.submission.Id,
                AdditionalCompilerArguments = this.submission.SubmissionType.AdditionalCompilerArguments,
                AllowedFileExtensions = this.submission.SubmissionType.AllowedFileExtensions,
                FileContent = this.submission.Content,
                CompilerType = this.submission.SubmissionType.CompilerType,
                TaskSkeleton = this.submission.SolutionSkeleton,
                TimeLimit = this.submission.Problem.TimeLimit,
                MemoryLimit = this.submission.Problem.MemoryLimit,
                CheckerParameter = this.submission.Problem.Checker.Parameter,
                CheckerAssemblyName = this.submission.Problem.Checker.DllFile,
                CheckerTypeName = this.submission.Problem.Checker.ClassName,
                ExecutionStrategyType = this.submission.SubmissionType.ExecutionStrategyType,
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
            };
        }

        public override void BeforeExecute()
        {
            this.submission.ProcessingComment = null;
            this.testRunsData.DeleteBySubmission(this.submission.Id);
        }

        private void UpdateResults()
        {
            try
            {
                this.CalculatePointsForSubmission();
            }
            catch (Exception ex)
            {
                this.Logger.ErrorFormat("CalculatePointsForSubmission on submission №{0} has thrown an exception: {1}", this.submission.Id, ex);
                this.submission.ProcessingComment = $"Exception in CalculatePointsForSubmission: {ex.Message}";
            }

            try
            {
                this.SetSubmissionToProcessed();
            }
            catch (Exception ex)
            {
                this.Logger.ErrorFormat("Unable to save changes to the submission for processing №{0}! Exception: {1}", this.submissionForProcessing.Id, ex);
            }

            try
            {
                this.SaveParticipantScore();
            }
            catch (Exception exception)
            {
                this.Logger.ErrorFormat("SaveParticipantScore on submission №{0} has thrown an exception: {1}", this.submission.Id, exception);
                this.submission.ProcessingComment = $"Exception in SaveParticipantScore: {exception.Message}";
            }

            try
            {
                this.submission.CacheTestRuns();
            }
            catch (Exception exception)
            {
                this.Logger.ErrorFormat("CacheTestRuns on submission №{0} has thrown an exception: {1}", this.submission.Id, exception);
                this.submission.ProcessingComment = $"Exception in CacheTestRuns: {exception.Message}";
            }

            try
            {
                this.submissionsData.Update(this.submission);
            }
            catch (Exception exception)
            {
                this.Logger.ErrorFormat("Unable to save changes to the submission №{0}! Exception: {1}", this.submission.Id, exception);
            }

            this.Logger.InfoFormat("Submission №{0} successfully processed", this.submission.Id);
        }

        private void CalculatePointsForSubmission()
        {
            // Internal joke: submission.Points = new Random().Next(0, submission.Problem.MaximumPoints + 1) + Weather.Instance.Today("Sofia").IsCloudy ? 10 : 0;
            if (this.submission.Problem.Tests.Count == 0 || this.submission.TestsWithoutTrialTestsCount == 0)
            {
                this.submission.Points = 0;
            }
            else
            {
                this.submission.Points = (this.submission.CorrectTestRunsWithoutTrialTestsCount * this.submission.Problem.MaximumPoints) / this.submission.TestsWithoutTrialTestsCount;
            }
        }

        private void SaveParticipantScore()
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

        private void SetSubmissionToProcessed()
        {
            this.submission.Processed = true;
            this.submissionForProcessing.Processed = true;
            this.submissionForProcessing.Processing = false;
            this.submissionsForProccessingData.Update(this.submissionForProcessing);
        }
    }
}