namespace OJS.Services.Data.Problems
{
    using System.Data.Entity;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class ProblemsDataService : IProblemsDataService
    {
        private readonly IEfDeletableEntityRepository<Problem> problems;

        public ProblemsDataService(IEfDeletableEntityRepository<Problem> problems) =>
            this.problems = problems;

        public IQueryable<Problem> GetByIdQuery(int id) =>
            this.problems.All().Where(p => p.Id == id);

        public IQueryable<Problem> GetAllByContest(int contestId) =>
            this.problems.All().Where(p => p.ContestId == contestId);

        public Problem GetById(int id) => this.problems.GetById(id);

        public Problem GetWithContestById(int problemId) =>
            this.problems.All().Include(p => p.Contest).FirstOrDefault(p => p.Id == problemId);

        public void DeleteByProblem(Problem problem)
        {
            this.problems.Delete(problem);
            this.problems.SaveChanges();
        }

        public void DeleteAllByContestId(int contestId) => this.problems.Delete(p => p.ContestId == contestId);
    }
}