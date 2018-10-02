namespace OJS.Data.Archives.Repositories.Contracts
{
    using OJS.Data.Repositories.Contracts;

    public interface IArchivesGenericRepository<T> : IEfGenericRepository<T>
        where T : class
    {
        void CreateDatabaseIfNotExists();
    }
}