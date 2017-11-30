namespace OJS.Data.Repositories.Contracts
{
    using OJS.Data.Contracts;

    public interface IEfDeletableEntityRepository<T> :
        IEfGenericRepository<T>,
        IDeletableEntityRepository<T>
            where T : class
    {
    }
}