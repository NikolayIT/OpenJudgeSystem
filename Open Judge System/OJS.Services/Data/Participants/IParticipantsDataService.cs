namespace OJS.Services.Data.Participants
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IParticipantsDataService : IService
    {
        void Add(Participant participant);

        void Update(Participant participant);

        void Update(
            IQueryable<Participant> participantsQuery,
            Expression<Func<Participant, Participant>> updateExpression);

        void Delete(IEnumerable<Participant> participants);

        Participant GetWithContestByContestByUserAndIsOfficial(int contestId, string userId, bool isOfficial);

        IQueryable<Participant> GetByIdQuery(int participantId);

        IQueryable<Participant> GetAll();

        IQueryable<Participant> GetAllByContestAndIsOfficial(int contestId, bool isOfficial);

        bool AnyByContestIdUserIdAndIsOfficial(int contestId, string userId, bool isOfficial);

        bool IsOfficialById(int participantId);

        IQueryable<Participant> GetAllOfficialInOnlineContestByContestAndContestStartTimeRange(
            int contestId,
            DateTime contestStartTimeRangeStart,
            DateTime contestStartTimeRangeEnd);
    }
}