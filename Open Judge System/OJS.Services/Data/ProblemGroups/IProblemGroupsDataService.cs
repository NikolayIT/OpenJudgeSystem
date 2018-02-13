namespace OJS.Services.Data.ProblemGroups
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IProblemGroupsDataService : IService
    {
        IQueryable<ProblemGroup> GetAllByContest(int contestId);
    }
}