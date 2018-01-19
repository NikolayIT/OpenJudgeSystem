namespace OJS.Services.Business.ExamGroups
{
    using System.Collections.Generic;

    public interface IExamGroupsBusinessService
    {
        void AddUsersByIdAndUserIds(int id, IEnumerable<string> userIds);

        void RemoveUsersByIdAndUserIds(int id, IEnumerable<string> userIds);

        void AddExternalUserByIdAndUser(int id, string userId);
    }
}