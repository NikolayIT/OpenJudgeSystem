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

        public IQueryable<ProblemGroup> GetAllByContest(int contestId) =>
            this.problemGroups.All().Where(pg => pg.ContestId == contestId);
    }
}