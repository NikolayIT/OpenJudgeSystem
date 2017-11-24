namespace OJS.Services.Data.Participants
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class ParticipantsDataService : IParticipantsDataService
    {
        private readonly IEfGenericRepository<Participant> participnats;

        public ParticipantsDataService(IEfGenericRepository<Participant> participnats) =>
            this.participnats = participnats;

        public IQueryable<Participant> GetAllQuery() => this.participnats.All();

        public IQueryable<Participant> GetAllWithScoresByContestIdAndIsOfficialQuery(int contestId, bool isOfficial) =>
            this.participnats
                .All()
                .Where(p => p.ContestId == contestId && p.Scores.Any() && p.IsOfficial == isOfficial);

        public bool AnyByContestIdUserIdAndIsOfficial(int contestId, string userId, bool isOfficial) =>
            this.participnats
                .All()
                .Any(p => p.ContestId == contestId &&
                    p.UserId == userId &&
                    p.IsOfficial == isOfficial);

        public IQueryable<Participant> GetByContestIdQuery(int contestId, bool isOfficial) =>
            this.participnats.All().Where(p => p.ContestId == contestId && p.IsOfficial == isOfficial);
    }
}