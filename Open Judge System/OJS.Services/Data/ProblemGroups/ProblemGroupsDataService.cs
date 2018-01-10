namespace OJS.Services.Data.ProblemGroups
{
    using System.Data.Entity;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class ProblemGroupsDataService : IProblemGroupsDataService
    {
        private readonly IEfGenericRepository<ProblemGroup> problemGroups;

        public ProblemGroupsDataService(IEfGenericRepository<ProblemGroup> problemGroups) =>
            this.problemGroups = problemGroups;

        public int? GetIdByContestAndOrderBy(int contestId, int? orderBy) =>
            this.problemGroups
                .All()
                .FirstOrDefault(pg => pg.ContestId == contestId && pg.OrderBy == orderBy)?.Id;
    }
}