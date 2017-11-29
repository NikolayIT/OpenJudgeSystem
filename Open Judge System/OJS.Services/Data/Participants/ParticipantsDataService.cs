namespace OJS.Services.Data.Participants
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class ParticipantsDataService : IParticipantsDataService
    {
        private readonly IEfGenericRepository<Participant> participants;

        public ParticipantsDataService(IEfGenericRepository<Participant> participants) =>
            this.participants = participants;

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
    }
}