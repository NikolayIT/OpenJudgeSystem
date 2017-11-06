namespace OJS.Services.Data.Contests
{
    using System.Linq;
    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IContestsDataService : IService
    {
        Contest GetFirstOrDefault(int id);

        Contest GetContestForSimpleResults(int id);

        Contest GetContestForFullResults(int id);

        bool UserHasAccessToContest(int contestId, string userId, bool isAdmin);

        IQueryable<Contest> GetByIdQuery(int id);
    }
}