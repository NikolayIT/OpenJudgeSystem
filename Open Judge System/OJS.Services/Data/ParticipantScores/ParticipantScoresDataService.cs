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
                .FirstOrDefault(ps => ps.ParticipantId == participantId &&
                    ps.ProblemId == problemId);

        public ParticipantScore GetByParticipantIdProblemIdAndIsOfficial(int participantId, int problemId, bool isOfficial) =>
            this.participantScores
                .All()
                .FirstOrDefault(ps => ps.ParticipantId == participantId &&
                    ps.ProblemId == problemId &&
                    ps.IsOfficial == isOfficial);

        public IQueryable<ParticipantScore> GetAllQuery() =>
            this.participantScores.All();

        public void SaveBySubmission(Submission submission, bool resetScore = false)
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

            if (participant != null)
            {
                using (var transaction = this.participantScores.BeginTransaction())
                {
                    var existingScore = this.GetByParticipantIdProblemIdAndIsOfficial(
                        submission.ParticipantId.Value,
                        submission.ProblemId.Value,
                        participant.IsOfficial);

                    if (existingScore == null)
                    {
                        this.AddNew(submission, participant.UserName, participant.IsOfficial);
                    }
                    else if (resetScore ||
                        submission.Points >= existingScore.Points ||
                        submission.Id == existingScore.SubmissionId)
                    {
                        this.Update(existingScore, submission.Id, submission.Points);
                    }

                    transaction.Commit();
                }
            }
        }

        public void DeleteAllByProblem(int problemId) =>
            this.participantScores.All()
                .Where(x => x.ProblemId == problemId)
                .Delete();

        public void DeleteForParticipantByProblem(int participantId, int problemId)
        {
            var isOfficial = this.participantsData.IsOfficial(participantId);

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

        private void AddNew(Submission submission, string participantName, bool isOfficial)
        {
            this.participantScores.Add(new ParticipantScore
            {
                ParticipantId = submission.ParticipantId.Value,
                ProblemId = submission.ProblemId.Value,
                SubmissionId = submission.Id,
                ParticipantName = participantName,
                Points = submission.Points,
                IsOfficial = isOfficial
            });

            this.participantScores.SaveChanges();
        }

        private void Update(ParticipantScore participantScore, int submissionId, int submissionPoints)
        {
            participantScore.SubmissionId = submissionId;
            participantScore.Points = submissionPoints;

            this.participantScores.Update(participantScore);
            this.participantScores.SaveChanges();
        }
    }
}