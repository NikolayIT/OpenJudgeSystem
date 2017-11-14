namespace OJS.Services.Data.ParticipantScores
{
    using System.Linq;
    using System.Transactions;
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
            

        public ParticipantScore GetParticipantScore(int participantId, int problemId, bool isOfficial) =>
            this.participantScores.All()
                .FirstOrDefault(x => x.ParticipantId == participantId &&
                    x.ProblemId == problemId &&
                    x.IsOfficial == isOfficial);

        public void SaveParticipantScore(Submission submission, bool resetScore = false)
        {
            using (var transaction = new TransactionScope())
            {
                var participant = this.participantsData
                    .GetByIdQuery(submission.ParticipantId.Value)
                    .Select(p => new
                    {
                        p.IsOfficial,
                        p.User.UserName
                    })
                    .FirstOrDefault();

                if (submission.ParticipantId == null || submission.ProblemId == null)
                {
                    return;
                }

                var existingScore = this.GetParticipantScore(
                    submission.ParticipantId.Value,
                    submission.ProblemId.Value,
                    participant.IsOfficial);

                if (existingScore == null)
                {
                    this.participantScores.Add(new ParticipantScore
                    {
                        ParticipantId = submission.ParticipantId.Value,
                        ProblemId = submission.ProblemId.Value,
                        SubmissionId = submission.Id,
                        ParticipantName = participant.UserName,
                        Points = submission.Points,
                        IsOfficial = participant.IsOfficial
                    });
                }
                else if (resetScore || submission.Points >= existingScore.Points || submission.Id == existingScore.SubmissionId)
                {
                    existingScore.SubmissionId = submission.Id;
                    existingScore.Points = submission.Points;
                }

                this.participantScores.SaveChanges();
                transaction.Complete();
            }
        }

        public void DeleteParticipantScores(int problemId) =>
            this.participantScores.All()
                .Where(x => x.ProblemId == problemId)
                .Delete();

    }
}