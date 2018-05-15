namespace OJS.Data.Archives
{
    using System.Data.Entity;

    using OJS.Data.Archives.Models;

    public class ArchivesDbContext : DbContext, IArchivesDbContext
    {
        public ArchivesDbContext() :
            base("ArchivesConnection")
        {
        }

        public virtual IDbSet<ArchivedSubmission> Submissions { get; set; }

        public DbContext DbContext => this;
    }
}