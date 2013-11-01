namespace OJS.Tools.OldDatabaseMigration.Copiers
{
    using OJS.Data;
    using OJS.Data.Models;

    internal sealed class ContestCategoriesCopier : ICopier
    {
        public void Copy(OjsDbContext context, TelerikContestSystemEntities oldDb)
        {
            foreach (var oldContestType in oldDb.ContestTypes)
            {
                var category = new ContestCategory
                                   {
                                       Name = oldContestType.Name,
                                       IsVisible = oldContestType.IsVisible,
                                       OrderBy = oldContestType.Order,
                                   };
                context.ContestCategories.Add(category);
            }

            context.SaveChanges();
        }
    }
}