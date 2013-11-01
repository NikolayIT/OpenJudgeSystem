namespace OJS.Data.Repositories.Contracts
{
    using System.Linq;

    using OJS.Data.Contracts;
    using OJS.Data.Models;

    public interface IContestsRepository : IRepository<Contest>, IDeletableEntityRepository<Contest>
    {
        IQueryable<Contest> AllActive();

        IQueryable<Contest> AllFuture();

        IQueryable<Contest> AllPast();

        IQueryable<Contest> AllVisible();

        IQueryable<Contest> AllVisibleInCategory(int categoryId);
    }
}
