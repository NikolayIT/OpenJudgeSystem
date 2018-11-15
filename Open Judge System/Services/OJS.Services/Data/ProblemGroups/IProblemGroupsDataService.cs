namespace OJS.Services.Data.ProblemGroups
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IProblemGroupsDataService : IService
    {
        ProblemGroup GetById(int id);

        ProblemGroup GetByProblem(int problemId);

        IQueryable<ProblemGroup> GetByIdQuery(int id);

        IQueryable<ProblemGroup> GetAll();

        IQueryable<ProblemGroup> GetAllWithDeleted();

        IQueryable<ProblemGroup> GetAllByContest(int contestId);

        IQueryable<Problem> GetProblemsById(int id);

        void Add(ProblemGroup problemGroup);

        bool IsFromContestByIdAndContest(int id, int contestId);
    }
}