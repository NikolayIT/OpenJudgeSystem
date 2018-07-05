namespace OJS.Services.Data.Problems
{
    using System.Data.Entity;
    using System.Linq;

    using OJS.Common;
    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class ProblemsDataService : IProblemsDataService
    {
        private readonly IEfDeletableEntityRepository<Problem> problems;

        public ProblemsDataService(IEfDeletableEntityRepository<Problem> problems) =>
            this.problems = problems;

        public Problem GetById(int id) => this.problems.GetById(id);

        public IQueryable<Problem> GetAll() => this.problems.All();

        public IQueryable<Problem> GetByIdQuery(int id) =>
            this.problems
                .All()
                .Where(p => p.Id == id);

        public Problem GetWithProblemGroupById(int id) =>
            this.problems
                .All()
                .Include(p => p.ProblemGroup)
                .FirstOrDefault(p => p.Id == id);

        public Problem GetWithContestById(int id) =>
            this.problems
                .All()
                .Include(p => p.ProblemGroup.Contest)
                .FirstOrDefault(p => p.Id == id);

        public IQueryable<Problem> GetAllByContest(int contestId) =>
            this.problems
                .All()
                .Where(p => p.ProblemGroup.ContestId == contestId);

        public IQueryable<Problem> GetAllByProblemGroup(int problemGroupId) =>
            this.problems
                .All()
                .Where(p => p.ProblemGroupId == problemGroupId);

        public bool ExistsById(int id) =>
            this.problems
                .All()
                .Any(p => p.Id == id);

        public int GetNewOrderByContest(int contestId) =>
            this.GetAllByContest(contestId)
                .OrderByDescending(p => p.OrderBy)
                .Select(p => new { p.OrderBy })
                .FirstOrDefault()
                ?.OrderBy + 1 ?? GlobalConstants.ProblemDefaultOrderBy;

        public int GetNewOrderByProblemGroup(int problemGroupId) =>
            this.GetAllByProblemGroup(problemGroupId)
                .OrderByDescending(p => p.OrderBy)
                .Select(p => new { p.OrderBy })
                .FirstOrDefault()
                ?.OrderBy + 1 ?? GlobalConstants.ProblemDefaultOrderBy;

        public string GetNameById(int id) =>
            this.GetByIdQuery(id)
                .Select(p => p.Name)
                .FirstOrDefault();

        public void Add(Problem problem)
        {
            this.problems.Add(problem);
            this.problems.SaveChanges();
        }

        public void Update(Problem problem)
        {
            this.problems.Update(problem);
            this.problems.SaveChanges();
        }
    }
}