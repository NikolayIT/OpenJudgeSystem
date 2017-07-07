namespace OJS.Data.Repositories
{
    using System.Linq;

    using EntityFramework.Extensions;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Base;
    using OJS.Data.Repositories.Contracts;

    public class ParticipantScoresRepository : GenericRepository<ParticipantScore>, IParticipantScoresRepository
    {
        public ParticipantScoresRepository(IOjsDbContext context)
            : base(context)
        {
        }

        public void SaveParticipantScore(Submission submission, bool resetScore = false)
        {
            var participant = this.Context
                .Participants
                .Where(x => x.Id == submission.ParticipantId)
                .Select(x => new
                {
                    x.IsOfficial,
                    UserName = x.User.UserName
                })
                .FirstOrDefault();

            var existingScore = this.GetParticipantScore(
                submission.ParticipantId,
                submission.ProblemId,
                participant.IsOfficial);

            if (existingScore == null)
            {
                this.DbSet.Add(new ParticipantScore
                {
                    ParticipantId = submission.ParticipantId.Value,
                    ProblemId = submission.ProblemId.Value,
                    SubmissionId = submission.Id,
                    ParticipantName = participant.UserName,
                    Points = submission.Points,
                    IsOfficial = participant.IsOfficial
                });
            }
            else if (resetScore || submission.Points >= existingScore.Points || submission.Id == existingScore.Id)
            {
                existingScore.SubmissionId = submission.Id;
                existingScore.Points = submission.Points;
            }
        }

        public void RecalculateParticipantScore(int participantId, int problemId)
        {
            var submission = this.Context.Submissions
                .Where(x => x.ParticipantId == participantId
                            && x.ProblemId == problemId
                            && !x.IsDeleted
                            && x.Processed)
                .OrderByDescending(x => x.Points)
                .ThenByDescending(x => x.Id)
                .FirstOrDefault();

            if (submission != null)
            {
                this.SaveParticipantScore(submission, true);
            }
            else
            {
                this.DeleteParticipantScore(participantId, problemId);
            }
        }

        public void DeleteParticipantScore(Submission submission)
        {
            if (submission.ParticipantId.HasValue && submission.ProblemId.HasValue)
            {
                this.DeleteParticipantScore(submission.ParticipantId.Value, submission.ProblemId.Value);
            }
        }

        public void DeleteParticipantScores(int problemId)
            => this.Context.ParticipantScores
                .Where(x => x.ProblemId == problemId)
                .Delete();

        private ParticipantScore GetParticipantScore(int? participantId, int? problemId, bool isOfficial)
            => this.Context.ParticipantScores
                .Where(x => x.ParticipantId == participantId
                    && x.ProblemId == problemId
                    && x.IsOfficial == isOfficial)
                .FirstOrDefault();

        private void DeleteParticipantScore(int participantId, int problemId)
        {
            var isOfficial = this.Context
                    .Participants
                    .Where(x => x.Id == participantId)
                    .Select(x => x.IsOfficial)
                    .FirstOrDefault();

            var existingScore = this.GetParticipantScore(participantId, problemId, isOfficial);

            if (existingScore != null)
            {
                this.Context.ParticipantScores.Remove(existingScore);
            }
        }
    }
}
