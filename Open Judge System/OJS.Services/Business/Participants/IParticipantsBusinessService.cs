namespace OJS.Services.Business.Participants
{
    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IParticipantsBusinessService : IService
    {
        bool CanCompeteByContestAndUserId(Contest contest, string userId);

        Participant CreateNewByContestUserIdAndIsOfficial(Contest contest, string userId, bool isOfficial);
    }
}