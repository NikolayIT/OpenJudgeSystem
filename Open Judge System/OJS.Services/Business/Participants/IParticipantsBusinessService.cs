namespace OJS.Services.Business.Participants
{
    using System;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IParticipantsBusinessService : IService
    {
        Participant CreateNewByContestUserIdIsOfficialAndIsAdmin(
            Contest contest,
            string userId,
            bool isOfficial,
            bool isAdmin);

        void ExtendContestEndTimeForAllActiveParticipantsByContestByParticipantContestStartTimeRangeAndTimeIntervalInMinutes(
            int contestId,
            int minutes,
            DateTime contestStartTimeRangeStart,
            DateTime contestStartTimeRangeEnd);
    }
}