namespace OJS.Data.Repositories.Contracts
{
    using System.Data.Entity.Infrastructure;

    using OJS.Data.Contracts;

    public interface IEfGenericRepository<T> : IRepository<T>
        where T : class
    {
        DbContextConfiguration ContextConfiguration { get; }
    }
}