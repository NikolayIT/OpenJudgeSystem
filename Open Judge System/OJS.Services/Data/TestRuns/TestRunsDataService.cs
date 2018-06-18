namespace OJS.Services.Data.TestRuns
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class TestRunsDataService : ITestRunsDataService
    {
        private readonly IEfGenericRepository<TestRun> testRuns;

        public TestRunsDataService(IEfGenericRepository<TestRun> testRuns) =>
            this.testRuns = testRuns;

        public IQueryable<TestRun> GetAllByTest(int testId) =>
            this.testRuns
                .All()
                .Where(tr => tr.TestId == testId);

        public void DeleteByProblem(int problemId) =>
            this.testRuns.Delete(tr => tr.Submission.ProblemId == problemId);

        public void DeleteByTest(int testId) =>
            this.testRuns.Delete(tr => tr.TestId == testId);

        public void DeleteBySubmission(int submissionId) =>
            this.testRuns.Delete(tr => tr.SubmissionId == submissionId);
    }
}