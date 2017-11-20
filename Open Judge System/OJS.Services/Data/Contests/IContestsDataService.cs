namespace OJS.Services.Data.Contests
{
    using System.Linq;
    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IContestsDataService : IService
    {
        Contest GetById(int contestId);

        IQueryable<Contest> GetAll();

        bool UserHasAccessToContest(int contestId, string userId, bool isAdmin);

        IQueryable<Contest> GetByIdQuery(int id);

        Contest GetWithProblems(int contestId);
    }
}