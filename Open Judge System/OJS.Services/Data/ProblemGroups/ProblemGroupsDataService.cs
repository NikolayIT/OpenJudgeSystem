namespace OJS.Services.Data.ProblemGroups
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class ProblemGroupsDataService : IProblemGroupsDataService
    {
        private readonly IEfDeletableEntityRepository<ProblemGroup> problemGroups;

        public ProblemGroupsDataService(IEfDeletableEntityRepository<ProblemGroup> problemGroups) =>
            this.problemGroups = problemGroups;

        public ProblemGroup GetById(int id) => this.problemGroups.GetById(id);

        public ProblemGroup GetByProblem(int problemId) =>
            this.problemGroups.All().FirstOrDefault(pg => pg.Problems.Any(p => p.Id == problemId));

        public IQueryable<ProblemGroup> GetByIdQuery(int id) =>
            this.GetAll().Where(pg => pg.Id == id);

        public IQueryable<ProblemGroup> GetAll() => this.problemGroups.All();

        public IQueryable<ProblemGroup> GetAllByContest(int contestId) =>
            this.problemGroups.All().Where(pg => pg.ContestId == contestId);

        public IQueryable<Problem> GetProblemsById(int id) =>
            this.GetByIdQuery(id).SelectMany(eg => eg.Problems).Where(p => !p.IsDeleted);
    }
}