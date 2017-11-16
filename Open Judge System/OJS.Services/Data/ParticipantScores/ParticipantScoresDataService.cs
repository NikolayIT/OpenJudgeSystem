namespace OJS.Services.Data.ParticipantScores
{
    using System;
    using System.Collections.Generic;
    using System.Data;
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

        public ParticipantScore Get(int participantId, int problemId) =>
            this.participantScores
                .All()
                .FirstOrDefault(ps => ps.ParticipantId == participantId &&
                    ps.ProblemId == problemId);

        public ParticipantScore Get(int participantId, int problemId, bool isOfficial) =>
            this.participantScores
                .All()
                .FirstOrDefault(ps => ps.ParticipantId == participantId &&
                    ps.ProblemId == problemId &&
                    ps.IsOfficial == isOfficial);

        public IQueryable<ParticipantScore> GetAll()
            => this.participantScores.All();

        public void Save(Submission submission, bool resetScore = false)
        {
            if (submission.ParticipantId == null)
            {
                return;
            }

            var participant = this.GetParticipantData(submission.ParticipantId.Value);

            if (participant != null)
            {
                this.Save(submission, participant.Item1, participant.Item2, resetScore);
            }
        }

        public void SaveInTransaction(Submission submission, bool resetScore = false)
        {
            if (submission.ParticipantId == null)
            {
                return;
            }

            var participant = this.GetParticipantData(submission.ParticipantId.Value);

            if (participant != null)
            {
                using (var transaction = this.participantScores.BeginTransaction())
                {
                    this.Save(submission, participant.Item1, participant.Item2, resetScore);
                    transaction.Commit();
                }
            }
        }

        public void DeleteAllByProblem(int problemId) =>
            this.participantScores.All()
                .Where(x => x.ProblemId == problemId)
                .Delete();

        public void Delete(int participantId, int problemId)
        {
            var isOfficial = this.participantsData.IsOfficial(participantId);

            var existingScore = this.Get(participantId, problemId, isOfficial);

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

        private void Save(Submission submission, string participantUsername, bool isOfficial, bool resetScore)
        {
            if (submission.ParticipantId != null && submission.ProblemId != null)
            {
                var existingScore = this.Get(
                    submission.ParticipantId.Value,
                    submission.ProblemId.Value,
                    isOfficial);

                if (existingScore == null)
                {
                    this.AddNew(submission, participantUsername, isOfficial);
                }
                else if (resetScore ||
                    submission.Points >= existingScore.Points ||
                    submission.Id == existingScore.SubmissionId)
                {
                    this.Update(existingScore, submission.Id, submission.Points);
                }
            }
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

        //TODO: use new Value Tuples structure when updated to .NET Framework 4.7 or above
        private Tuple<string, bool> GetParticipantData(int participantId)
        {
            var participant = this.participantsData
                .GetByIdQuery(participantId)
                .Select(p => new
                {
                    p.IsOfficial,
                    p.User.UserName
                })
                .FirstOrDefault();

            return participant != null ?
                new Tuple<string, bool>(participant.UserName, participant.IsOfficial) :
                null;
        }
    }
}