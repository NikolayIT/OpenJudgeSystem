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

        public ProblemResource GetById(int id) =>
            this.problemResources
                .All()
                .FirstOrDefault(pr => pr.Id == id);

        public IQueryable<ProblemResource> GetByProblemQuery(int problemId) =>
            this.problemResources
                .All()
                .Where(pr => pr.ProblemId == problemId);

        public void DeleteById(int id)
        {
            this.problemResources.Delete(id);
            this.problemResources.SaveChanges();
        }

        public void DeleteByProblem(int problemId) =>
            this.problemResources.Delete(pr => pr.ProblemId == problemId);
    }
}