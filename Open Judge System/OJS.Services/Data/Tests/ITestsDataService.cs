namespace OJS.Services.Data.Tests
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface ITestsDataService : IService
    {
        Test GetById(int id);

        IQueryable<Test> GetByIdQuery(int id);

        IQueryable<Test> GetAllByProblem(int problemId);

        IQueryable<Test> GetAllNonTrialByProblem(int problemId);

        void Add(Test test);

        void Update(Test test);

        void Delete(Test test);

        void DeleteByProblem(int problemId);
    }
}