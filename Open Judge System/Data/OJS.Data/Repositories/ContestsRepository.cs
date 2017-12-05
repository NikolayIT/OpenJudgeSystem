namespace OJS.Data.Repositories
{
    using OJS.Data.Models;
    using OJS.Data.Repositories.Base;
    using OJS.Data.Repositories.Contracts;

    public class ContestsRepository : EfDeletableEntityRepository<Contest>, IContestsRepository
    {
        public ContestsRepository(IOjsDbContext context)
            : base(context)
        {
        }
    }
}