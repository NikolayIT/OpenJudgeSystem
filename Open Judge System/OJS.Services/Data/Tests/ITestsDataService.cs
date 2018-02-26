namespace OJS.Services.Data.Tests
{
    using OJS.Services.Common;

    public interface ITestsDataService : IService
    {
        void DeleteByProblem(int problemId);

        void DeleteByContest(int contestId);
    }
}