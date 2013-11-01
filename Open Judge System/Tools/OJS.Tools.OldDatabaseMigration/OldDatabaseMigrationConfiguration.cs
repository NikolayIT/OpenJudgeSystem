namespace OJS.Tools.OldDatabaseMigration
{
    using System.Collections.Generic;
    using System.Linq;

    using OJS.Data;
    using OJS.Data.Migrations;
    using OJS.Tools.OldDatabaseMigration.Copiers;

    /// <summary>
    /// Entity Framework migration configuration class that copies all required data from the old database to the new one.
    /// </summary>
    /// <remarks>
    /// Old database tables:
    /// * aspnet_Applications -> Useless.
    /// * aspnet_Membership -> Not needed because of the new identity system (AspNetUsers).
    /// * aspnet_Paths -> Useless.
    /// * aspnet_PersonalizationAllUsers -> Useless.
    /// * aspnet_PersonalizationPerUser -> Useless.
    /// * aspnet_Profile -> Useless.
    /// * aspnet_Roles -> Not needed because of the new identity system (AspNetRoles).
    /// * aspnet_SchemaVersions -> Useless.
    /// * aspnet_Users -> Moved all users to the new identity tables (AspNetUsers).
    /// * aspnet_UsersInRoles -> Not needed because of the new identity system (AspNetUserRoles).
    /// * aspnet_WebEvent_Events -> Useless.
    /// * BugReports -> Bug reports will be revisited but will not be copied to the new database.
    /// * Checkers -> Checkers will be seeded.
    /// * Contests -> Contests will be copied ot the table "Contests".
    /// * ContestTypes -> Contest categories (types) will be copied to the table "ContestCategories".
    /// * News -> News will not be copied since they are not relevant.
    /// * Participants -> Participants will be copied to the table "Participants".
    /// * Settings -> Settings will not be copied since they are not relevant to the new system. Anyway a table "Settings" will be available for the runtime settings of the new system.
    /// * Submissions -> ------------- TODO -------------
    /// * Tasks -> Tasks will be copied to the table "Problems".
    /// * Tests -> Tests will be copied to the table "Tests".
    /// * Users -> Users will be copied to the table "AspNetUsers".
    /// </remarks>
    public sealed class OldDatabaseMigrationConfiguration : DefaultMigrationConfiguration
    {
        private readonly TelerikContestSystemEntities oldDb;

        private readonly IEnumerable<ICopier> copiers;

        public OldDatabaseMigrationConfiguration()
        {
            this.AutomaticMigrationsEnabled = true;
            this.AutomaticMigrationDataLossAllowed = true;

            this.oldDb = new TelerikContestSystemEntities();

            this.copiers = new List<ICopier>
                               {
                                   new ContestCategoriesCopier(),
                                   new ContestsCopier(),
                                   new TasksCopier(),
                                   new UsersCopier(),
                                   new TestsCopier(),
                                   //new ParticipantsCopier(),
                                   //new SubmissionsCopier(),
                               };
        }

        protected override void Seed(OjsDbContext context)
        {
            // context.ClearDatabase();
            if (context.Problems.Count() > 400)
            {
                // The data is already copied.
                return;
            }

            this.SeedCheckers(context);
            this.SeedRoles(context);
            this.SeedSubmissionTypes(context);

            foreach (var copier in this.copiers)
            {
                copier.Copy(context, this.oldDb);
                context.SaveChanges(); // Prevents us from forgetting to call SaveChanges in this.XXXCopier().Copy() methods
            }
        }
    }
}
