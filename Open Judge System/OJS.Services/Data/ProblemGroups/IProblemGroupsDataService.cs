namespace OJS.Services.Data.ProblemGroups
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IProblemGroupsDataService : IService
    {
        IQueryable<ProblemGroup> GetAll();

        IQueryable<ProblemGroup> GetByIdQuery(int id);

        int? GetIdByContestAndOrderBy(int contestId, int? orderBy);

        IQueryable<Problem> GetProblemsById(int id);
    }
}