namespace OJS.Data.Repositories.Base
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    using EntityFramework.Extensions;

    using OJS.Data.Contracts;

    public class DeletableEntityRepository<T> : GenericRepository<T>, IDeletableEntityRepository<T>
        where T : class, IDeletableEntity, new()
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

            this.Update(entity);
        }

        public override int Delete(Expression<Func<T, bool>> filterExpression)
        {
            return this.DbSet.Where(filterExpression).Update(entity => new T { IsDeleted = true, DeletedOn = DateTime.Now });
        }

        public void HardDelete(T entity)
        {
            base.Delete(entity);
        }

        public int HardDelete(Expression<Func<T, bool>> filterExpression)
        {
            return base.Delete(filterExpression);
        }
    }
}
