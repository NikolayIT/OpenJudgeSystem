namespace OJS.Services.Business.Participants
{
    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IParticipantsBusinessService : IService
    {
        bool CanCompeteByContestAndUserId(Contest contest, string userId);

        bool CanCompeteByContestIdAndUserId(int contestId, string userId);

        Participant CreateNewByContestUserIdIsOfficialAndIsAdmin(
            Contest contest,
            string userId,
            bool isOfficial,
            bool isAdmin);
    }
}