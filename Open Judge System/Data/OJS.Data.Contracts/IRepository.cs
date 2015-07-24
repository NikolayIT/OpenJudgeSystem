namespace OJS.Data.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public interface IRepository<T> where T : class
    {
        IQueryable<T> All();

        T GetById(int id);

        void Add(T entity);

        /// <remarks>Use only in transaction scope</remarks>
        void Add(IEnumerable<T> entities);

        void Update(T entity);

        int Update(Expression<Func<T, bool>> filterExpression, Expression<Func<T, T>> updateExpression);

        void Delete(T entity);

        void Delete(int id);

        int Delete(Expression<Func<T, bool>> filterExpression);

        void Detach(T entity);

        void UpdateValues(Expression<Func<T, object>> entity);
    }
}
