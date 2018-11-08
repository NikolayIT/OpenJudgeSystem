namespace OJS.Services.Business.ParticipantScores
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;

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
                this.InternalNormalizeSubmissionPoints(s =>
                    s.Problem.ProblemGroup.ContestId == contestId);

                this.InternalNormalizeParticipantScorePoints(ps =>
                    ps.Problem.ProblemGroup.ContestId == contestId);

                scope.Complete();
            }
        }

        public void NormalizeAllPointsThatExceedAllowedLimit()
        {
            using (var scope = TransactionsHelper.CreateTransactionScope())
            {
                this.InternalNormalizeSubmissionPoints();
                this.InternalNormalizeParticipantScorePoints();

                scope.Complete();
            }
        }

        public void InternalNormalizeSubmissionPoints(
            Expression<Func<Submission, bool>> additionalFilter = null)
        {
            var submissions = this.submissionsData
                .GetAll()
                .Include(s => s.Problem)
                .Where(s => s.Points > s.Problem.MaximumPoints)
                .Where(additionalFilter ?? (_ => true))
                .ToList();

            foreach (var submission in submissions)
            {
                submission.Points = submission.Problem.MaximumPoints;

                this.submissionsData.Update(submission);
            }
        }

        public void InternalNormalizeParticipantScorePoints(
            Expression<Func<ParticipantScore, bool>> additionalFilter = null)
        {
            var participantScores = this.participantScoresData
                .GetAll()
                .Include(ps => ps.Problem)
                .Where(ps => ps.Points > ps.Problem.MaximumPoints)
                .Where(additionalFilter ?? (_ => true))
                .ToList();

            foreach (var participantScore in participantScores)
            {
                this.participantScoresData.UpdateBySubmissionAndPoints(
                    participantScore,
                    participantScore.SubmissionId,
                    participantScore.Problem.MaximumPoints);
            }
        }
    }
}