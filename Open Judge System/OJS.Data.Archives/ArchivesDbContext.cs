namespace OJS.Data.Archives
{
    using System.Data.Entity;

    using OJS.Common.Constants;
    using OJS.Data.Models;

    public class ArchivesDbContext : DbContext, IArchivesDbContext
    {
        public ArchivesDbContext()
            : this(AppSettingConstants.ArchivesDbConnectionStringName)
        {
        }

        protected ArchivesDbContext(string nameOrConnectionString) :
            base(nameOrConnectionString)
        {
        }

        public virtual IDbSet<ArchivedSubmission> Submissions { get; set; }

        public DbContext DbContext => this;
    }
}