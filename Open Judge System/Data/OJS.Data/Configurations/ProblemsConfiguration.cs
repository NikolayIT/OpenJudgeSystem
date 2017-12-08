namespace OJS.Data.Configurations
{
    using System.Data.Entity.ModelConfiguration;

    using OJS.Data.Models;

    public class ProblemsConfiguration : EntityTypeConfiguration<Problem>
    {
        public ProblemsConfiguration()
        {
            /*
             * The following configuration fixes:
             * Introducing FOREIGN KEY constraint 'FK_dbo.Problems_dbo.Contests_ContestId' on table 'Problems' may cause cycles or multiple cascade paths. Specify ON DELETE NO ACTION or ON UPDATE NO ACTION, or modify other FOREIGN KEY constraints.
             */
            this.HasRequired(p => p.Contest)
                .WithMany(c => c.Problems)
                .HasForeignKey(p => p.ContestId)
                .WillCascadeOnDelete(false);
        }
    }
}