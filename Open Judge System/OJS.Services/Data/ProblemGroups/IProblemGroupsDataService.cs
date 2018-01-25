namespace OJS.Services.Data.ProblemGroups
{
    using OJS.Services.Common;

    public interface IProblemGroupsDataService : IService
    {
        int? GetIdByContestAndOrderBy(int contestId, int? orderBy);
    }
}