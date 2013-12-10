namespace OJS.Data.Repositories.Base
{
    using System.Linq;

    using OJS.Data.Contracts;

    public class DeletableEntityRepository<T> :
        GenericRepository<T>, IDeletableEntityRepository<T> where T : class, IDeletableEntity
    {
        public DeletableEntityRepository(IOjsDbContext context)
            : base(context)
        {
        }

        public override IQueryable<T> All()
        {
            return base.All().Where(x => !x.IsDeleted);
        }

        public IQueryable<T> AllWithDeleted()
        {
            return base.All();
        }
    }
}
