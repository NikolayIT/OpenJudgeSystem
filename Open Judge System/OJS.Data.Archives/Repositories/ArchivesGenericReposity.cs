namespace OJS.Data.Archives.Repositories
{
    using System.Data.SqlClient;

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

        public void CreateDatabaseIfNotExists()
        {
            var connectionString = this.Context.Database.Connection.ConnectionString;
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = connectionStringBuilder.InitialCatalog;

            connectionStringBuilder.InitialCatalog = "master";

            using (var connection = new SqlConnection(connectionStringBuilder.ToString()))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"select * from master.dbo.sysdatabases where name='{databaseName}'";

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            // exists
                            return;
                        }
                    }

                    command.CommandText = $"CREATE DATABASE {databaseName}";
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}