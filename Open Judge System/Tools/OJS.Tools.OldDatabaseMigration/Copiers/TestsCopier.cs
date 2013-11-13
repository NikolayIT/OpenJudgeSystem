namespace OJS.Tools.OldDatabaseMigration.Copiers
{
    using System;
    using System.Linq;

    using OJS.Data;

    internal sealed class TestsCopier : ICopier
    {
        public void Copy(OjsDbContext context, TelerikContestSystemEntities oldDb)
        {
            // Moving tests in iterations to prevent OutOfMemoryException
            var count = oldDb.Tests.Count();
            const int ElementsByIteration = 10;
            var iterations = Math.Ceiling((decimal)count / ElementsByIteration);
            for (int i = 0; i < iterations; i++)
            {
                GC.Collect();
                var newDb = new OjsDbContext();
                newDb.Configuration.AutoDetectChangesEnabled = false;
                newDb.Configuration.ValidateOnSaveEnabled = false;

                oldDb = new TelerikContestSystemEntities();
                oldDb.Configuration.AutoDetectChangesEnabled = false;
                GC.Collect();
                foreach (var oldTest in oldDb.Tests.OrderBy(x => x.Id).Skip(i * ElementsByIteration).Take(ElementsByIteration))
                {
                    var problem = newDb.Problems.FirstOrDefault(x => x.OldId == oldTest.Task);

                    var test = new Data.Models.Test
                                   {
                                       InputDataAsString = oldTest.Input,
                                       OutputDataAsString = oldTest.Output,
                                       Problem = problem,
                                       IsTrialTest = oldTest.IsZeroTest,
                                       OrderBy = oldTest.Order,
                                   };

                    newDb.Tests.Add(test);
                }

                newDb.SaveChanges();
            }
        }
    }
}