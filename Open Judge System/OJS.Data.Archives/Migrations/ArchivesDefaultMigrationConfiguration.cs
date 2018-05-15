namespace OJS.Data.Archives.Migrations
{
    using System.Data.Entity.Migrations;

    using OJS.Data.Archives.Models;

    public class ArchivesDefaultMigrationConfiguration
        : DbMigrationsConfiguration<ArchivesDbContext>
    {
        public ArchivesDefaultMigrationConfiguration()
        {
            this.AutomaticMigrationsEnabled = true;
            this.AutomaticMigrationDataLossAllowed = false;
        }

        protected override void Seed(ArchivesDbContext context)
        {
            this.SeedSubmissions(context);
            base.Seed(context);
        }

        private void SeedSubmissions(ArchivesDbContext context)
        {
            context.Submissions.AddOrUpdate(
                s => s.Id,
                new ArchivedSubmission
                {
                    Id = 1,
                    Processed = true,
                    IsCompiledSuccessfully = true
                });

            context.SaveChanges();
        }
    }
}