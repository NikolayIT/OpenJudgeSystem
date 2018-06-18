namespace OJS.Data.Archives.Repositories
{
    using OJS.Data.Archives.Repositories.Contracts;
    using OJS.Data.Repositories.Base;

    public class ArchivesGenericReposity<T> : EfGenericRepository<T>, IArchivesGenericRepository<T>
        where T : class
    {
        // Requires ArchivesDbContext as parameter in order for this context to be injected correctly
        public ArchivesGenericReposity(ArchivesDbContext context)
            : base(context)
        {
        }

        public void CreateDatabaseIfNotExists() =>
            this.Context.Database.CreateIfNotExists();
    }
}