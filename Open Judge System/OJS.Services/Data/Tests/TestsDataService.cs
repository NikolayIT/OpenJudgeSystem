namespace OJS.Services.Data.Tests
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class TestsDataService : ITestsDataService
    {
        private readonly IEfGenericRepository<Test> tests;

        public TestsDataService(IEfGenericRepository<Test> tests) =>
            this.tests = tests;

        public Test GetById(int id) => this.tests.GetById(id);

        public IQueryable<Test> GetByIdQuery(int id) =>
            this.tests
                .All()
                .Where(t => t.Id == id);

        public IQueryable<Test> GetAllByProblem(int problemId) =>
            this.tests
                .All()
                .Where(t => t.ProblemId == problemId);

        public IQueryable<Test> GetAllNonTrialByProblem(int problemId) =>
            this.GetAllByProblem(problemId)
                .Where(t => !t.IsTrialTest);

        public void Add(Test test)
        {
            this.tests.Add(test);
            this.tests.SaveChanges();
        }

        public void Update(Test test)
        {
            this.tests.Update(test);
            this.tests.SaveChanges();
        }

        public void Delete(Test test)
        {
            this.tests.Delete(test);
            this.tests.SaveChanges();
        }

        public void DeleteByProblem(int problemId) =>
            this.tests.Delete(t => t.ProblemId == problemId);
    }
}