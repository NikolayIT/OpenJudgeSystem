namespace OJS.Data.Archives
{
    using System.Data.Entity;
    using System.Data.SqlClient;

    using OJS.Common.Constants;
    using OJS.Data.Models;

    public class ArchivesDbContext : DbContext
    {
        public ArchivesDbContext() :
            base(AppSettingConstants.ArchivesDbConnectionStringName)
        {
        }

        public virtual IDbSet<ArchivedSubmission> Submissions { get; set; }

        public DbContext DbContext => this;

        public void CreateDatabaseIfNotExists(string connectionString)
        {
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