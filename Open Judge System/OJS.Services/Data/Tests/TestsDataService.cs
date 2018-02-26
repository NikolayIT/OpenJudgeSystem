namespace OJS.Services.Data.Tests
{
    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class TestsDataService : ITestsDataService
    {
        private readonly IEfGenericRepository<Test> tests;

        public TestsDataService(IEfGenericRepository<Test> tests) =>
            this.tests = tests;

        public void DeleteByProblem(int problemId) =>
            this.tests.Delete(t => t.ProblemId == problemId);

        public void DeleteByContest(int contestId) =>
            this.tests.Delete(t => t.Problem.ProblemGroup.ContestId == contestId);
    }
}