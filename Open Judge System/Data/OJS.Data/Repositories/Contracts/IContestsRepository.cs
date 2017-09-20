namespace OJS.Data.Repositories.Contracts
{
    using System.Linq;

    using OJS.Data.Contracts;
    using OJS.Data.Models;

    public interface IContestsRepository : IDeletableEntityRepository<Contest>
    {
        IQueryable<Contest> AllActive();

        IQueryable<Contest> AllUpcoming();

        IQueryable<Contest> AllPast();

        IQueryable<Contest> AllVisible();

        IQueryable<Contest> AllVisibleByCategory(int categoryId);
    }
}
