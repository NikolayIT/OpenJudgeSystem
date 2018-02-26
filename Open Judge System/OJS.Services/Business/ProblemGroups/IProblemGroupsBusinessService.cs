namespace OJS.Services.Business.ProblemGroups
{
    using OJS.Services.Common;

    public interface IProblemGroupsBusinessService : IService
    {
        ServiceResult DeleteById(int id);

        void DeleteByProblem(int problemId);
    }
}