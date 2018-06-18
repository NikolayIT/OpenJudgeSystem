namespace OJS.Data.Repositories.Base
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;

    using EntityFramework.Extensions;

    using OJS.Data.Contracts;
    using OJS.Data.Repositories.Contracts;

    public class EfDeletableEntityRepository<T> : EfGenericRepository<T>, IEfDeletableEntityRepository<T>
        where T : class, IDeletableEntity, new()
    {
        public EfDeletableEntityRepository(DbContext context)
            : base(context)
        {
        }

        public override IQueryable<T> All() => base.All().Where(x => !x.IsDeleted);

        public IQueryable<T> AllWithDeleted() => base.All();

        public override void Delete(T entity)
        {
            entity.IsDeleted = true;
            entity.DeletedOn = DateTime.Now;

            this.Update(entity);
        }

        public override int Delete(Expression<Func<T, bool>> filterExpression) =>
            this.DbSet
                .Where(filterExpression)
                .Update(entity => new T { IsDeleted = true, DeletedOn = DateTime.Now });

        public void HardDelete(T entity) => base.Delete(entity);

        public int HardDelete(Expression<Func<T, bool>> filterExpression) =>
            base.Delete(filterExpression);
    }
}