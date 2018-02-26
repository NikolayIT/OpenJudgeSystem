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

        IQueryable<Problem> GetByIdQuery(int id);

        IQueryable<Problem> GetByContestQuery(int contestId);

        int? GetContestIdById(int id);

        bool ExistsById(int id);

        bool IsFromOnlineContestById(int id);

        void Add(Problem problem);

        void Update(Problem problem);

        void Delete(Problem problem);

        void DeleteById(int id);

        void DeleteByContest(int contestId);
    }
}