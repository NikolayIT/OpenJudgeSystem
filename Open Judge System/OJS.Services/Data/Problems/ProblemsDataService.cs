namespace OJS.Services.Data.Problems
{
    using System.Data.Entity;
    using System.Linq;

    using OJS.Common.Models;
    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class ProblemsDataService : IProblemsDataService
    {
        private readonly IEfDeletableEntityRepository<Problem> problems;

        public ProblemsDataService(IEfDeletableEntityRepository<Problem> problems) =>
            this.problems = problems;

        public Problem GetById(int id) => this.problems.GetById(id);

        public Problem GetWithProblemGroupById(int id) =>
            this.problems.All().Include(p => p.ProblemGroup).FirstOrDefault(p => p.Id == id);

        public Problem GetWithContestById(int id) =>
            this.problems.All().Include(p => p.ProblemGroup.Contest).FirstOrDefault(p => p.Id == id);

        public IQueryable<Problem> GetByIdQuery(int id) =>
            this.problems.All().Where(p => p.Id == id);

        public IQueryable<Problem> GetByContestQuery(int contestId) =>
            this.problems.All().Where(p => p.ProblemGroup.ContestId == contestId);

        public bool ExistsById(int id) => this.problems.All().Any(p => p.Id == id);

        public bool IsFromOnlineContestById(int id) =>
            this.GetByIdQuery(id)
                .Select(p => p.ProblemGroup.Contest.Type)
                .FirstOrDefault() == ContestType.OnlinePracticalExam;

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