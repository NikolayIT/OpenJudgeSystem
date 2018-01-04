namespace OJS.Services.Data.Participants
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IParticipantsDataService : IService
    {
        DateTime? GetOfficialContestEndTimeByUserIdAndContestId(string userId, int contestId);

        void Add(Participant participant);

        void Update(Participant participant);

        void Update(
            Expression<Func<Participant, bool>> filterExpression,
            Expression<Func<Participant, Participant>> updateExpression);

        Participant GetWithContestByContestByUserAndIsOfficial(int contestId, string userId, bool isOfficial);

        IQueryable<Participant> GetByIdQuery(int participantId);

        bool IsOfficial(int participantId);

        IQueryable<Participant> GetAllOfficialInOnlineContestByContestAndContestStartTimeRange(
            int contestId,
            DateTime contestStartTimeRangeStart,
            DateTime contestStartTimeRangeEnd);
    }
}