namespace OJS.Data.Repositories
{
    using System.Data.Entity;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Base;
    using OJS.Data.Repositories.Contracts;

    public class ParticipantsRepository : GenericRepository<Participant>, IParticipantsRepository
    {
        public ParticipantsRepository(IOjsDbContext context)
            : base(context)
        {
        }

        public Participant GetWithContest(int contestId, string userId, bool isOfficial)
        {
            return
                this.All()
                    .Include(x => x.Contest)
                    .FirstOrDefault(x => x.ContestId == contestId && x.UserId == userId && x.IsOfficial == isOfficial);
        }

        public bool Any(int contestId, string userId, bool isOfficial)
        {
            return
                this.All()
                    .Any(x => x.ContestId == contestId && x.UserId == userId && x.IsOfficial == isOfficial);
        }
    }
}
