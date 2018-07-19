namespace OJS.Services.Business.ExamGroups
{
    using System.Collections.Generic;

    public interface IExamGroupsBusinessService
    {
        void AddUsersByIdAndUserIds(int id, IEnumerable<string> userIds);

        void AddUsersByIdAndUsernames(int id, IEnumerable<string> usernames);

        void RemoveUsersByIdAndUserIds(int id, IEnumerable<string> userIds);

        void AddExternalUserByIdAndUser(int id, string userId);

        void AddExternalUserByIdAndUsername(int id, string username);
    }
}