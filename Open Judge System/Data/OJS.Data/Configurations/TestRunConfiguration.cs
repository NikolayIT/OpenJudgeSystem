namespace OJS.Data.Configurations
{
    using System.Data.Entity.ModelConfiguration;

    using OJS.Data.Models;

    public class TestRunConfiguration : EntityTypeConfiguration<TestRun>
    {
        public TestRunConfiguration()
        {
            /*
             * The following configuration fixes:
             * Introducing FOREIGN KEY constraint 'FK_dbo.TestRuns_dbo.Tests_TestId' on table 'TestRuns' may cause cycles or multiple cascade paths. Specify ON DELETE NO ACTION or ON UPDATE NO ACTION, or modify other FOREIGN KEY constraints.
             */
            this.HasRequired(x => x.Test)
                .WithMany(x => x.TestRuns)
                .HasForeignKey(x => x.TestId)
                .WillCascadeOnDelete(false);
        }
    }
}
