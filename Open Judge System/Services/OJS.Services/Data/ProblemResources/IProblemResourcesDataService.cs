namespace OJS.Services.Data.ProblemResources
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IProblemResourcesDataService : IService
    {
        ProblemResource GetById(int id);

        IQueryable<ProblemResource> GetByProblemQuery(int problemId);

        void DeleteById(int id);

        void DeleteByProblem(int problemId);
    }
}