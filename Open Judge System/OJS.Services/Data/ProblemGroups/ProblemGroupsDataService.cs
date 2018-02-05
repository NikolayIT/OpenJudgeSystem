namespace OJS.Services.Data.ProblemGroups
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class ProblemGroupsDataService : IProblemGroupsDataService
    {
        private readonly IEfGenericRepository<ProblemGroup> problemGroups;

        public ProblemGroupsDataService(IEfGenericRepository<ProblemGroup> problemGroups) =>
            this.problemGroups = problemGroups;

        public IQueryable<ProblemGroup> GetAll() => this.problemGroups.All();

        public int? GetIdByContestAndOrderBy(int contestId, int? orderBy) =>
            this.problemGroups
                .All()
                .FirstOrDefault(pg => pg.ContestId == contestId && pg.OrderBy == orderBy)?.Id;

        public IQueryable<Problem> GetProblemsById(int id) =>
            this.GetAll()
                .Where(pg => pg.Id == id)
                .SelectMany(eg => eg.Problems)
                .Where(p => !p.IsDeleted);
    }
}