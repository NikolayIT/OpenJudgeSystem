namespace OJS.Services.Data.Participants
{
    using System;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IParticipantsDataService : IService
    {
        DateTime? GetOfficialContestEndTimeByUserIdAndContestId(string userId, int contestId);

        void Add(Participant participant);

        void Update(Participant participant);

        Participant GetWithContestByContestIdUserIdAndIsOfficial(int contestId, string userId, bool isOfficial);

        IQueryable<Participant> GetByIdQuery(int participantId);

        bool IsOfficial(int participantId);

        IQueryable<Participant> GetOfficialInOnlineContestByCreatedOnAfterDateTimeAndBeforeDateTimeAndContest(
            int contestId,
            DateTime after,
            DateTime before);
    }
}