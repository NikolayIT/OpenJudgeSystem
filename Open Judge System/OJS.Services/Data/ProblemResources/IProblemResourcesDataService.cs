namespace OJS.Services.Data.ProblemResources
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IProblemResourcesDataService : IService
    {
        IQueryable<ProblemResource> GetByProblemQuery(int problemId);

        void DeleteByProblem(int id);
    }
}