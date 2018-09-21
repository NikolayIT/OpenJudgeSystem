namespace OJS.Services.Business.Problems
{
    using OJS.Services.Common;

    public interface IProblemsBusinessService : IService
    {
        void RetestById(int id);

        void DeleteById(int id);

        void DeleteByContest(int contestId);

        ServiceResult CopyToContestByIdByContestAndProblemGroup(int id, int contestId, int? problemGroupId);
    }
}