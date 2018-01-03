﻿namespace OJS.Services.Data.Participants
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using OJS.Common.Models;
    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class ParticipantsDataService : IParticipantsDataService
    {
        private readonly IEfGenericRepository<Participant> participants;

        public ParticipantsDataService(IEfGenericRepository<Participant> participants) =>
            this.participants = participants;

        public DateTime? GetOfficialContestEndTimeByUserIdAndContestId(string userId, int contestId) =>
            this.participants
                .All()
                .FirstOrDefault(p => p.UserId == userId && p.ContestId == contestId && p.IsOfficial)?
                .ContestEndTime;

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
            Expression<Func<Participant, bool>> filterExpression,
            Expression<Func<Participant, Participant>> updateExpression)
        {
            this.participants.Update(filterExpression, updateExpression);
            this.participants.SaveChanges();
        }

        public Participant GetWithContestByContestIdUserIdAndIsOfficial(int contestId, string userId, bool isOfficial) =>
            this.participants
                .All()
                .Include(p => p.Contest)
                .FirstOrDefault(p => p.ContestId == contestId && p.UserId == userId && p.IsOfficial == isOfficial);

        public IQueryable<Participant> GetByIdQuery(int participantId) =>
            this.participants
                .All()
                .Where(x => x.Id == participantId);

        public bool IsOfficial(int participantId) =>
            this.participants
                .All()
                .Where(x => x.Id == participantId)
                .Select(x => x.IsOfficial)
                .FirstOrDefault();

        public IQueryable<Participant> GetOfficialInOnlineContestByCreatedOnAfterDateTimeAndBeforeDateTimeAndContest(
            int contestId, 
            DateTime after,
            DateTime before) =>
            this.participants
                .All()
                .Where(p => 
                    p.CreatedOn >= after &&
                    p.CreatedOn <= before &&
                    p.ContestId == contestId &&
                    p.IsOfficial &&
                    p.Contest.Type == ContestType.OnlinePracticalExam);

        public IQueryable<Participant> GetOfficialInOnlineContestByContestStartTimeAfterDateTimeAndBeforeDateTimeAndContest(
            int contestId,
            DateTime after,
            DateTime before) =>
            this.participants
                .All()
                .Where(p => 
                    p.ContestStartTime >= after &&
                    p.ContestStartTime <= before &&
                    p.ContestId == contestId &&
                    p.IsOfficial &&
                    p.Contest.Type == ContestType.OnlinePracticalExam);
    }
}