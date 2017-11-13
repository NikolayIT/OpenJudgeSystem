namespace OJS.Services.Data.Participants
{
    using System.Data.Entity;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class ParticipantsDataService : IParticipantsDataService
    {
        private readonly IEfGenericRepository<Participant> participnats;

        public ParticipantsDataService(IEfGenericRepository<Participant> participnats) =>
            this.participnats = participnats;

        public IQueryable<Participant> GetAll() => this.participnats.All();

        public bool Any(int contestId, string userId, bool isOfficial) =>
            this.participnats
                .All()
                .Any(p => p.ContestId == contestId &&
                    p.UserId == userId &&
                    p.IsOfficial == isOfficial);

        public Participant GetWithContest(int contestId, string userId, bool isOfficial) =>
            this.participnats
                .All()
                .Include(p => p.Contest)
                .FirstOrDefault(p => p.ContestId == contestId &&
                    p.UserId == userId &&
                    p.IsOfficial == isOfficial);

        public IQueryable<Participant> GetWithUsersAndScoresForContest(int contestId, bool isOfficial) =>
            this.participnats
                .All()
                .Include(par => par.User)
                .Include(par => par.Contest.Problems.Select(pr => pr.ParticipantScores))
                .Where(p => p.IsOfficial == isOfficial && p.ContestId == contestId);

        public IQueryable<Participant> GetWithUsersScoresAndSubmissionForContest(int contestId, bool isOfficial) =>
            this.participnats
                .All()
                .Include(par => par.User)
                .Include(par => par.Contest.Problems.Select(pr => pr.ParticipantScores.Select(sc => sc.Submission)))
                .Where(p => p.IsOfficial == isOfficial && p.ContestId == contestId);

    }
}