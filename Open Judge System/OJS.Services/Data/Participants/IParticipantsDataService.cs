namespace OJS.Services.Data.Participants
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IParticipantsDataService : IService
    {
        IQueryable<Participant> GetByIdQuery(int participantId);

        IQueryable<Participant> GetAll();

        IQueryable<Participant> GetAllWithScoresByContestIdAndIsOfficialQuery(int contestId, bool isOfficial);

        bool AnyByContestIdUserIdAndIsOfficial(int contestId, string userId, bool isOfficial);

        bool IsOfficialById(int participantId);
    }
}