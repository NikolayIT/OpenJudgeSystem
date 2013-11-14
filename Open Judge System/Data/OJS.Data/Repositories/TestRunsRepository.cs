namespace OJS.Data.Repositories
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Base;
    using OJS.Data.Repositories.Contracts;

    public class TestRunsRepository : GenericRepository<TestRun>, ITestRunsRepository
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
    }
}
