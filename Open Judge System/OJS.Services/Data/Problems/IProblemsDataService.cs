namespace OJS.Services.Data.Problems
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IProblemsDataService : IService
    {
        IQueryable<Problem> GetByIdQuery(int id);

        Problem GetWithProblemGroupById(int id);

        Problem GetWithContestById(int id);

        void DeleteByProblem(Problem problem);

        void DeleteAllByContestId(int contestId);
    }
}