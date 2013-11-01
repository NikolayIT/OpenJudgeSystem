namespace OJS.Tools.OldDatabaseMigration.Copiers
{
    using OJS.Data;

    public interface ICopier
    {
        void Copy(OjsDbContext context, TelerikContestSystemEntities oldDb);
    }
}
