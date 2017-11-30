namespace OJS.Services.Data.Contests
{
    using System.Linq;
    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IContestsDataService : IService
    {
        Contest GetById(int contestId);

        IQueryable<Contest> GetAllActive();

        IQueryable<Contest> GetAllInactive();

        IQueryable<Contest> GetAllUpcoming();

        IQueryable<Contest> GetAllPast();

        IQueryable<Contest> GetAllVisible();

        bool CanBeCompetedByContestId(int contestId);
    }
}