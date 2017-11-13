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

        public IQueryable<Participant> GetParticipantsWithSubmissions() =>
            this.participnats
                .All()
                .Include(par => par.User)
                .Include(par => par.Contest.Problems
                    .Select(pr => pr.ParticipantScores
                        .Select(sc => sc.Submission)));

        public IQueryable<Participant> GetParticipantsWithParticipantScores() =>
            this.participnats
                .All()
                .Include(par => par.User)
                .Include(par => par.Contest.Problems.Select(pr => pr.ParticipantScores));
    }
}