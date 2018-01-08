namespace OJS.Data.Configurations
{
    using System.Data.Entity;

    using OJS.Data.Models;

    public static class ManyToManyTableNamesConfiguration
    {
        private const string ProblemsForParticipantsTableName = "ProblemsForParticipants";

        public static void Configure(DbModelBuilder modelBuilder)
        {
            ConfigureProblemsForParticipants(modelBuilder);
        }

        private static void ConfigureProblemsForParticipants(DbModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Participant>()
                .HasMany(participant => participant.Problems)
                .WithMany(problem => problem.Participants)
                .Map(pp =>
                {
                    pp.ToTable(ProblemsForParticipantsTableName);
                });
        }
    }
}