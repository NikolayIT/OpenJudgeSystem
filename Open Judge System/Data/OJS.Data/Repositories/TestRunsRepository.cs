namespace OJS.Data.Repositories
{
    using System.Data.Entity;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Base;
    using OJS.Data.Repositories.Contracts;

    public class TestRunsRepository : EfGenericRepository<TestRun>, ITestRunsRepository
    {
        public TestRunsRepository(IOjsDbContext context)
            : base(context)
        {
        }

        public int DeleteBySubmissionId(int submissionId)
        {
            var testRuns = this.All().Where(x => x.SubmissionId == submissionId).ToList();
            foreach (var testRun in testRuns)
            {
                this.Delete(testRun);
            }

            return testRuns.Count;
        }

        public override void Delete(int id)
        {
            // TODO: evaluate if this is the best solution
            var testRun = this.Context.DbContext.ChangeTracker.Entries<TestRun>().FirstOrDefault(t => t.Property(pr => pr.Id).CurrentValue == id).Entity ?? new TestRun { Id = id };
            this.Context.Entry(testRun).State = EntityState.Deleted;
        }
    }
}
