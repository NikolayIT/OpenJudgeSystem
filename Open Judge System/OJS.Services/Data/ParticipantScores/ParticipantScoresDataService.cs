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
            this.participantScores
                .All()
                .FirstOrDefault(ps => ps.ParticipantId == participantId &&
                    ps.ProblemId == problemId &&
                    ps.IsOfficial == isOfficial);

        public bool SaveParticipantScore(Submission submission, bool resetScore = false)
        {
            using (var transaction = new TransactionScope())
            {
                if (submission.ParticipantId == null || submission.ProblemId == null)
                {
                    return false;
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
                    return false;
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
                else if (resetScore ||
                    submission.Points >= existingScore.Points ||
                    submission.Id == existingScore.SubmissionId)
                {
                    existingScore.SubmissionId = submission.Id;
                    existingScore.Points = submission.Points;
                }

                this.participantScores.SaveChanges();
                transaction.Complete();
                return true;
            }
        }

        public void DeleteParticipantScores(int problemId) =>
            this.participantScores.All()
                .Where(x => x.ProblemId == problemId)
                .Delete();

        public void DeleteParticipantScore(int participantId, int problemId)
        {
            var isOfficial = this.participantsData.IsOfficial(participantId);

            var existingScore = this.GetParticipantScore(participantId, problemId, isOfficial);

            if (existingScore != null)
            {
                this.participantScores.Delete(existingScore);
                this.participantScores.SaveChanges();
            }
        }
    }
}