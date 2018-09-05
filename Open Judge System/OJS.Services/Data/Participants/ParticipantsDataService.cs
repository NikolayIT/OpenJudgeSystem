namespace OJS.Services.Data.Participants
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;

    using EntityFramework.Extensions;

    using OJS.Common.Models;
    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class ParticipantsDataService : IParticipantsDataService
    {
        private readonly IEfGenericRepository<Participant> participants;

        public ParticipantsDataService(IEfGenericRepository<Participant> participants) =>
            this.participants = participants;

        public Participant GetById(int id) => this.participants.GetById(id);

        public Participant GetByContestByUserAndByIsOfficial(int contestId, string userId, bool isOfficial) =>
            this.GetAllByContestByUserAndIsOfficial(contestId, userId, isOfficial)
                .FirstOrDefault();

        public Participant GetWithContestByContestByUserAndIsOfficial(int contestId, string userId, bool isOfficial) =>
            this.GetAllByContestByUserAndIsOfficial(contestId, userId, isOfficial)
                .Include(p => p.Contest)
                .FirstOrDefault();

        public IQueryable<Participant> GetAll() => this.participants.All();

        public IQueryable<Participant> GetByIdQuery(int id) =>
            this.GetAll()
                .Where(p => p.Id == id);

        public IQueryable<Participant> GetAllOfficialInOnlineContestByContestAndContestStartTimeRange(
            int contestId,
            DateTime contestStartTimeRangeStart,
            DateTime contestStartTimeRangeEnd) =>
            this.GetAll()
                .Where(p =>
                    p.ParticipationStartTime >= contestStartTimeRangeStart &&
                    p.ParticipationStartTime <= contestStartTimeRangeEnd &&
                    p.ContestId == contestId &&
                    p.IsOfficial &&
                    p.Contest.Type == ContestType.OnlinePracticalExam);

        public IQueryable<Participant> GetAllByContestAndIsOfficial(int contestId, bool isOfficial) =>
            this.participants
                .All()
                .Where(p =>
                    p.ContestId == contestId &&
                    p.IsOfficial == isOfficial);

        public bool ExistsByContestAndUser(int contestId, string userId) =>
            this.GetAllByContestAndUser(contestId, userId)
                .Any();

        public bool ExistsByContestByUserAndIsOfficial(int contestId, string userId, bool isOfficial) =>
            this.GetAllByContestByUserAndIsOfficial(contestId, userId, isOfficial)
                .Any();

        public bool IsOfficialById(int id) =>
            this.participants
                .All()
                .Where(p => p.Id == id)
                .Select(p => p.IsOfficial)
                .FirstOrDefault();

        public void Add(Participant participant)
        {
            this.participants.Add(participant);
            this.participants.SaveChanges();
        }

        public void Update(Participant participant)
        {
            this.participants.Update(participant);
            this.participants.SaveChanges();
        }

        public void Update(
            IQueryable<Participant> participantsQuery,
            Expression<Func<Participant, Participant>> updateExpression) =>
            participantsQuery.Update(updateExpression);

        public void Delete(IEnumerable<Participant> participantsForDeletion)
        {
            foreach (var participant in participantsForDeletion)
            {
                this.participants.Delete(participant);
            }

            this.participants.SaveChanges();
        }

        public void InvalidateByContestAndIsOfficial(int contestId, bool isOfficial) =>
            this.GetAllByContestAndIsOfficial(contestId, isOfficial)
                .Update(p => new Participant
                {
                    IsInvalidated = true
                });

        private IQueryable<Participant> GetAllByContestAndUser(int contestId, string userId) =>
            this.participants
                .All()
                .Where(p => p.ContestId == contestId && p.UserId == userId);

        private IQueryable<Participant> GetAllByContestByUserAndIsOfficial(
            int contestId,
            string userId,
            bool isOfficial) =>
                this.GetAllByContestAndUser(contestId, userId)
                    .Where(p => p.IsOfficial == isOfficial);
    }
}