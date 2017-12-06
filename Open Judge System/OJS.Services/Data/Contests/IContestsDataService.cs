namespace OJS.Services.Data.Contests
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IContestsDataService : IService
    {
        Contest GetByIdWithProblems(int contestId);

        IQueryable<Contest> GetByIdQuery(int contestId);

        bool UserHasAccessByIdUserIdAndIsAdmin(int contestId, string userId, bool isAdmin);
    }
}