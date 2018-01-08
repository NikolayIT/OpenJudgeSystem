namespace OJS.Services.Data.Participants
{
    using System;
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

        public ParticipantsDataService(IEfGenericRepository<Participant> participnats) =>
            this.participants = participnats;

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


        public Participant GetWithContestByContestByUserAndIsOfficial(int contestId, string userId, bool isOfficial) =>
            this.participants
                .All()
                .Include(p => p.Contest)
                .FirstOrDefault(p => p.ContestId == contestId && p.UserId == userId && p.IsOfficial == isOfficial);

        public IQueryable<Participant> GetAll() => this.participants.All();

        public IQueryable<Participant> GetByIdQuery(int participantId) =>
            this.participants
                .All()
                .Where(x => x.Id == participantId);

        public bool IsOfficialById(int participantId) =>
            this.participants
                .All()
                .Where(x => x.Id == participantId)
                .Select(x => x.IsOfficial)
                .FirstOrDefault();

        public IQueryable<Participant> GetAllOfficialInOnlineContestByContestAndContestStartTimeRange(
            int contestId,
            DateTime contestStartTimeRangeStart,
            DateTime contestStartTimeRangeEnd) =>
            this.participants
                .All()
                .Where(p =>
                    p.ContestStartTime >= contestStartTimeRangeStart &&
                    p.ContestStartTime <= contestStartTimeRangeEnd &&
                    p.ContestId == contestId &&
                    p.IsOfficial &&
                    p.Contest.Type == ContestType.OnlinePracticalExam);

        public IQueryable<Participant> GetAllWithScoresByContestIdAndIsOfficialQuery(int contestId, bool isOfficial) =>
            this.participants
                .All()
                .Where(p => p.ContestId == contestId && p.Scores.Any() && p.IsOfficial == isOfficial);

        public bool AnyByContestIdUserIdAndIsOfficial(int contestId, string userId, bool isOfficial) =>
            this.participants
                .All()
                .Any(p => p.ContestId == contestId &&
                    p.UserId == userId &&
                    p.IsOfficial == isOfficial);
    }
}