namespace OJS.Services.Data.TestRuns
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface ITestRunsDataService : IService
    {
        IQueryable<TestRun> GetAllByTest(int testId);

        void DeleteByProblem(int problemId);

        void DeleteByTest(int testId);

        void DeleteBySubmission(int submissionId);
    }
}