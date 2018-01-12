namespace OJS.Services.Business.Users
{
    using OJS.Services.Common;

    public interface IUsersBusinessService : IService
    {
        void AddToExamGroupByIdAndExternalExamGroup(string userId, int? externalExamGroupId);
    }
}