namespace OJS.Services.Data.Participants
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class ParticipantsDataService : IParticipantsDataService
    {
        private readonly IEfGenericRepository<Participant> participants;

        public ParticipantsDataService(IEfGenericRepository<Participant> participnats) =>
            this.participants = participnats;

        public IQueryable<Participant> GetAll() => this.participants.All();

        public IQueryable<Participant> GetByIdQuery(int participantId) =>
            this.participants
                .All()
                .Where(x => x.Id == participantId);

        public bool IsOfficialById(int participantId) =>
            this.participants
                .All()
                .Where(x => x.Id == participantId)
                .Select(x => x.IsOfficial)
                .FirstOrDefault();

        public IQueryable<Participant> GetAllWithScoresByContestIdAndIsOfficialQuery(int contestId, bool isOfficial) =>
            this.participants
                .All()
                .Where(p => p.ContestId == contestId && p.Scores.Any() && p.IsOfficial == isOfficial);

        public bool AnyByContestIdUserIdAndIsOfficial(int contestId, string userId, bool isOfficial) =>
            this.participants
                .All()
                .Any(p => p.ContestId == contestId &&
                    p.UserId == userId &&
                    p.IsOfficial == isOfficial);
    }
}