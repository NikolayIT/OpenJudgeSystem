namespace OJS.Services.Data.ProblemGroups
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IProblemGroupsDataService : IService
    {
        ProblemGroup GetById(int id);

        IQueryable<ProblemGroup> GetByIdQuery(int id);

        IQueryable<ProblemGroup> GetAll();

        int? GetIdByContestAndOrderBy(int contestId, int? orderBy);

        IQueryable<Problem> GetProblemsById(int id);

        void Delete(ProblemGroup problemGroup);

        bool ExistsByContestAndOrderBy(int contestId, int orderBy);
    }
}