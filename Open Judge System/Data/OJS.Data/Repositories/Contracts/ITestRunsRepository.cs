namespace OJS.Data.Repositories.Contracts
{
    using OJS.Data.Contracts;
    using OJS.Data.Models;

    public interface ITestRunsRepository : IRepository<TestRun>
    {
        int DeleteBySubmissionId(int submissionId);
    }
}
