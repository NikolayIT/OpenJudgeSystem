namespace OJS.Data.Repositories.Contracts
{
    using System.Data;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;

    using OJS.Data.Contracts;

    public interface IEfGenericRepository<T> : IRepository<T>
        where T : class
    {
        /// <summary>
        /// Gets DbContextConfiguration Class that provides
        /// access to configuration options for the context.
        /// </summary>
        DbContextConfiguration ContextConfiguration { get; }

        /// <summary>
        /// Starts a database transaction.
        /// </summary>
        DbContextTransaction BeginTransaction();

        /// <summary>
        /// Starts a database transaction with the specified isolation level.
        /// </summary>
        DbContextTransaction BeginTransaction(IsolationLevel isolationLevel);
    }
}