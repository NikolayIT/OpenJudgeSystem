namespace OJS.Data.Repositories
{
    using System;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Base;
    using OJS.Data.Repositories.Contracts;

    public class ContestsRepository : DeletableEntityRepository<Contest>, IContestsRepository
    {
        public ContestsRepository(IOjsDbContext context)
            : base(context)
        {
        }

        public IQueryable<Contest> AllActive()
        {
            return
                this.All()
                    .Where(x => x.StartTime <= DateTime.Now && DateTime.Now <= x.EndTime && x.IsVisible);
        }

        public IQueryable<Contest> AllFuture()
        {
            return this.All().Where(x => x.StartTime > DateTime.Now && x.IsVisible);
        }

        public IQueryable<Contest> AllPast()
        {
            return this.All().Where(x => x.EndTime < DateTime.Now && x.IsVisible);
        }

        public IQueryable<Contest> AllVisible()
        {
            return this.All()
                .Where(x => x.IsVisible);
        }

        public IQueryable<Contest> AllVisibleInCategory(int categoryId)
        {
            return this.All()
                .Where(x => x.IsVisible && x.CategoryId == categoryId);
        }
    }
}
