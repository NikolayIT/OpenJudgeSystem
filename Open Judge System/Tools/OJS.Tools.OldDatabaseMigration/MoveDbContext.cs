namespace OJS.Tools.OldDatabaseMigration
{
    using OJS.Data;

    /*
    public class Program
    {
        public static void Main()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<MoveDbContext, OldDatabaseMigrationConfiguration>());
            var migrator = new DbMigrator(new OldDatabaseMigrationConfiguration());
            migrator.Update();
        }
    }
     */

    public class MoveDbContext : OjsDbContext
    {
        public MoveDbContext()
            : base("MoveDbContext")
        {
        }
    }
}
