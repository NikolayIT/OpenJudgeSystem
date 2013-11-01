namespace OJS.Data.Repositories.Contracts
{
    using OJS.Data.Contracts;
    using OJS.Data.Models;

    public interface IParticipantsRepository : IRepository<Participant>
    {
        Participant GetWithContest(int contestId, string userId, bool isOfficial);

        bool Any(int contestId, string userId, bool isOfficial);
    }
}
