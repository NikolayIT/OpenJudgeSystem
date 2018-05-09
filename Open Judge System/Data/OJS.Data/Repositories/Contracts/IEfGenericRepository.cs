namespace OJS.Data.Repositories.Contracts
{
    using OJS.Data.Contracts;

    public interface IEfGenericRepository<T> : IRepository<T>
        where T : class
    {
    }
}