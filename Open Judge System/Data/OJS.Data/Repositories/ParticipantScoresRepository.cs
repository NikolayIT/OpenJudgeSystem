namespace OJS.Data.Repositories
{
    using System.Linq;
    using OJS.Data.Models;
    using OJS.Data.Repositories.Base;
    using OJS.Data.Repositories.Contracts;

    public class ParticipantScoresRepository : GenericRepository<ParticipantScore>, IParticipantScoresRepository
    {
        public ParticipantScoresRepository(IOjsDbContext context)
            : base(context)
        {
        }

        public void SaveParticipantScore(Submission submission)
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

            var existingScore = this.Context.ParticipantScores
                .Where(x => x.ParticipantId == submission.ParticipantId
                    && x.ProblemId == submission.ProblemId
                    && x.IsOfficial == participant.IsOfficial)
                .FirstOrDefault();

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
            else if (submission == null)
            {
                this.DbSet.Remove(existingScore);
            }
            else if (submission.Points >= existingScore.Points)
            {
                existingScore.SubmissionId = submission.Id;
                existingScore.Points = submission.Points;
            }
        }

        public void RecalculateParticipantScore(int participantId, int problemId)
        {
            var submission = this.Context
                .Submissions
                .Where(x => x.ParticipantId == participantId && x.ProblemId == problemId && !x.IsDeleted)
                .OrderBy(x => x.Points)
                .ThenByDescending(x => x.Id)
                .FirstOrDefault();

            this.SaveParticipantScore(submission);
        }
    }
}
