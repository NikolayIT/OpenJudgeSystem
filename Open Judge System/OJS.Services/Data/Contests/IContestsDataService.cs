namespace OJS.Services.Data.Contests
{
    using System.Linq;
    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IContestsDataService : IService
    {
        Contest GetFirstOrDefault(int id);

        IQueryable<Contest> All();

        bool UserHasAccessToContest(int contestId, string userId, bool isAdmin);

        IQueryable<Contest> GetByIdQuery(int id);
    }
}