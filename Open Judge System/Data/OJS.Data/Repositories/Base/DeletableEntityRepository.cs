namespace OJS.Data.Repositories.Base
{
    using System.Linq;

    using OJS.Data.Contracts;
    using System;
    using System.Data.Entity;

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

        public override void Delete(T entity)
        {
            entity.IsDeleted = true;
            entity.DeletedOn = DateTime.Now;
            var entry = this.Context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                this.DbSet.Attach(entity);
            }

            entry.State = EntityState.Modified;
        }

        public void HardDelete(T entity)
        {
            base.Delete(entity);
        }
    }
}
