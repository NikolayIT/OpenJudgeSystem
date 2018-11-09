namespace OJS.Services.Data.ParticipantScores
{
    using System.Collections.Generic;
    using System.Linq;

    using EntityFramework.Extensions;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;
    using OJS.Services.Data.Participants;

    public class ParticipantScoresDataService : IParticipantScoresDataService
    {
        private readonly IEfGenericRepository<ParticipantScore> participantScores;
        private readonly IParticipantsDataService participantsData;

        public ParticipantScoresDataService(
            IEfGenericRepository<ParticipantScore> participantScores,
            IParticipantsDataService participantsData)
        {
            this.participantScores = participantScores;
            this.participantsData = participantsData;
        }

        public ParticipantScore GetByParticipantIdAndProblemId(int participantId, int problemId) =>
            this.participantScores
                .All()
                .FirstOrDefault(ps =>
                    ps.ParticipantId == participantId &&
                    ps.ProblemId == problemId);

        public ParticipantScore GetByParticipantIdProblemIdAndIsOfficial(int participantId, int problemId, bool isOfficial) =>
            this.participantScores
                .All()
                .FirstOrDefault(ps =>
                    ps.ParticipantId == participantId &&
                    ps.ProblemId == problemId &&
                    ps.IsOfficial == isOfficial);

        public IQueryable<ParticipantScore> GetAll() =>
            this.participantScores.All();

        public IQueryable<ParticipantScore> GetAllByProblem(int problemId) =>
            this.GetAll()
                .Where(ps => ps.ProblemId == problemId);

        public IQueryable<ParticipantScore> GetAllHavingPointsExceedingLimit() =>
            this.GetAll()
                .Where(ps => ps.Points > ps.Problem.MaximumPoints);

        public void ResetBySubmission(Submission submission)
        {
            if (submission.ParticipantId == null || submission.ProblemId == null)
            {
                return;
            }

            var participant = this.participantsData
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

            var existingScore = this.GetByParticipantIdProblemIdAndIsOfficial(
                submission.ParticipantId.Value,
                submission.ProblemId.Value,
                participant.IsOfficial);

            if (existingScore == null)
            {
                this.AddBySubmissionByUsernameAndIsOfficial(submission, participant.UserName, participant.IsOfficial);
            }
            else
            {
                this.UpdateBySubmissionAndPoints(existingScore, submission.Id, submission.Points);
            }
        }

        public void DeleteAllByProblem(int problemId) =>
            this.participantScores
                .All()
                .Where(x => x.ProblemId == problemId)
                .Delete();

        public void DeleteForParticipantByProblem(int participantId, int problemId)
        {
            var isOfficial = this.participantsData.IsOfficialById(participantId);

            var existingScore = this.GetByParticipantIdProblemIdAndIsOfficial(participantId, problemId, isOfficial);

            if (existingScore != null)
            {
                this.participantScores.Delete(existingScore);
                this.participantScores.SaveChanges();
            }
        }

        public void Delete(IEnumerable<ParticipantScore> participantScoresEnumerable)
        {
            foreach (var participantScore in participantScoresEnumerable)
            {
                this.participantScores.Delete(participantScore);
            }

            this.participantScores.SaveChanges();
        }

        public void AddBySubmissionByUsernameAndIsOfficial(Submission submission, string username, bool isOfficial)
        {
            this.participantScores.Add(new ParticipantScore
            {
                ParticipantId = submission.ParticipantId.Value,
                ProblemId = submission.ProblemId.Value,
                SubmissionId = submission.Id,
                ParticipantName = username,
                Points = submission.Points,
                IsOfficial = isOfficial
            });

            this.participantScores.SaveChanges();
        }

        public void UpdateBySubmissionAndPoints(
            ParticipantScore participantScore,
            int? submissionId,
            int submissionPoints)
        {
            participantScore.SubmissionId = submissionId;
            participantScore.Points = submissionPoints;

            this.participantScores.Update(participantScore);
            this.participantScores.SaveChanges();
        }

        public void RemoveSubmissionIdsBySubmissionIds(IEnumerable<int> submissionIds) =>
            this.participantScores
                .Update(
                    ps => submissionIds.Cast<int?>().Contains(ps.SubmissionId),
                    ps => new ParticipantScore
                    {
                        SubmissionId = null
                    });
    }
}