namespace OJS.Services.Data.Problems
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IProblemsDataService : IService
    {
        Problem GetById(int id);

        Problem GetWithProblemGroupById(int id);

        Problem GetWithContestById(int id);

        IQueryable<Problem> GetAll();

        IQueryable<Problem> GetByIdQuery(int id);

        IQueryable<Problem> GetAllByContest(int contestId);

        IQueryable<Problem> GetAllByProblemGroup(int problemGroupId);

        bool ExistsById(int id);

        int GetNewOrderByContest(int contestId);

        int GetNewOrderByProblemGroup(int problemGroupId);

        string GetNameById(int id);

        void Add(Problem problem);

        void Update(Problem problem);
    }
}