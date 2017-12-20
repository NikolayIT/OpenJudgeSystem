namespace OJS.Services.Data.Participants
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IParticipantsDataService : IService
    {
        IQueryable<Participant> GetByIdQuery(int participantId);

        bool IsOfficial(int participantId);

        void ChangeTimeForActiveInOnlineContestByContestIdAndMinutes(int contestId, int minutes);
    }
}