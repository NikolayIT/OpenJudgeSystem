namespace OJS.Services.Data.ProblemResources
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class ProblemResourcesDataService : IProblemResourcesDataService
    {
        private readonly IEfDeletableEntityRepository<ProblemResource> problemResources;

        public ProblemResourcesDataService(IEfDeletableEntityRepository<ProblemResource> problemResources) =>
            this.problemResources = problemResources;

        public IQueryable<ProblemResource> GetByProblemQuery(int problemId) =>
            this.problemResources.All().Where(pr => pr.ProblemId == problemId);

        public void DeleteByProblem(int id) =>
            this.problemResources.Delete(pr => pr.ProblemId == id);
    }
}