namespace OJS.Services.Data.Problems
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IProblemsDataService : IService
    {
        IQueryable<Problem> GetByIdQuery(int problemId);

        Problem GetWithContestById(int problemId);

        void DeleteByProblem(Problem problem);

        void DeleteAllByContestId(int contestId);
    }
}