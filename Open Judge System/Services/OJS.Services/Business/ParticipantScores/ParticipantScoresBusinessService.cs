namespace OJS.Services.Business.ParticipantScores
{
    using System.Linq;

    using OJS.Common.Helpers;
    using OJS.Data.Models;
    using OJS.Services.Data.ParticipantScores;
    using OJS.Services.Data.Submissions;

    public class ParticipantScoresBusinessService : IParticipantScoresBusinessService
    {
        private readonly IParticipantScoresDataService participantScoresData;
        private readonly ISubmissionsDataService submissionsData;

        public ParticipantScoresBusinessService(
            IParticipantScoresDataService participantScoresData,
            ISubmissionsDataService submissionsData)
        {
            this.participantScoresData = participantScoresData;
            this.submissionsData = submissionsData;
        }

        public void RecalculateForParticipantByProblem(int participantId, int problemId)
        {
            var submission = this.submissionsData.GetBestForParticipantByProblem(participantId, problemId);

            if (submission != null)
            {
                this.participantScoresData.ResetBySubmission(submission);
            }
            else
            {
                this.participantScoresData.DeleteForParticipantByProblem(participantId, problemId);
            }
        }

        public void NormalizePointsThatExceedAllowedLimitByContest(int contestId)
        {
            using (var scope = TransactionsHelper.CreateTransactionScope())
            {
                this.InternalNormalizeSubmissionPoints(contestId);
                this.InternalNormalizeParticipantScorePoints(contestId);

                scope.Complete();
            }
        }

        public (int updatedSubmissionsCount, int updatedParticipantScoresCount) NormalizeAllPointsThatExceedAllowedLimit()
        {
            int updatedSubmissionsCount,
                updatedParticipantScoresCount;

            using (var scope = TransactionsHelper.CreateTransactionScope())
            {
                updatedSubmissionsCount = this.InternalNormalizeSubmissionPoints();
                updatedParticipantScoresCount = this.InternalNormalizeParticipantScorePoints();

                scope.Complete();
            }

            return (updatedSubmissionsCount, updatedParticipantScoresCount);
        }

        public int InternalNormalizeSubmissionPoints(int? contestId = null)
        {
            var updatedSubmissionsCount = 0;
            IQueryable<Submission> submissionsQuery;

            if (contestId.HasValue)
            {
                submissionsQuery = this.submissionsData
                    .GetAllHavingPointsExceedingLimitByContest(contestId.Value);
            }
            else
            {
                submissionsQuery = this.submissionsData.GetAllHavingPointsExceedingLimit();
            }

            submissionsQuery
                .Select(s => new
                {
                    Submission = s,
                    ProblemMaxPoints = s.Problem.MaximumPoints
                })
                .ToList()
                .ForEach(x =>
                {
                    x.Submission.Points = x.ProblemMaxPoints;

                    this.submissionsData.Update(x.Submission);

                    updatedSubmissionsCount++;
                });

            return updatedSubmissionsCount;
        }

        public int InternalNormalizeParticipantScorePoints(int? contestId = null)
        {
            var updatedParticipantScoresCount = 0;
            IQueryable<ParticipantScore> participantScoresQuery;

            if (contestId.HasValue)
            {
                participantScoresQuery = this.participantScoresData
                    .GetAllHavingPointsExceedingLimitByContest(contestId.Value);
            }
            else
            {
                participantScoresQuery = this.participantScoresData.GetAllHavingPointsExceedingLimit();
            }

            participantScoresQuery
                .Select(ps => new
                {
                    ParticipantScore = ps,
                    ProblemMaxPoints = ps.Problem.MaximumPoints
                })
                .ToList()
                .ForEach(x =>
                {
                    this.participantScoresData.UpdateBySubmissionAndPoints(
                        x.ParticipantScore,
                        x.ParticipantScore.SubmissionId,
                        x.ProblemMaxPoints);

                    updatedParticipantScoresCount++;
                });

            return updatedParticipantScoresCount;
        }
    }
}