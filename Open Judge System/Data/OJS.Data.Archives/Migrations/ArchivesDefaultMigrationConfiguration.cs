namespace OJS.Data.Archives.Migrations
{
    using System.Data.Entity.Migrations;

    public class ArchivesDefaultMigrationConfiguration
        : DbMigrationsConfiguration<ArchivesDbContext>
    {
        public ArchivesDefaultMigrationConfiguration()
        {
            this.AutomaticMigrationsEnabled = true;
            this.AutomaticMigrationDataLossAllowed = false;
        }
    }
}