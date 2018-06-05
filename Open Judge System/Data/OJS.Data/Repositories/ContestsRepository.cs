namespace OJS.Data.Repositories
{
    using System.Data.Entity;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Base;
    using OJS.Data.Repositories.Contracts;

    public class ContestsRepository : EfDeletableEntityRepository<Contest>, IContestsRepository
    {
        public ContestsRepository(DbContext context)
            : base(context)
        {
        }
    }
}