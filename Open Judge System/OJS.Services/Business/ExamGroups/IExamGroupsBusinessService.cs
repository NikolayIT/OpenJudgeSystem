namespace OJS.Services.Business.ExamGroups
{
    using System.Collections.Generic;

    using OJS.Services.Common;

    public interface IExamGroupsBusinessService : IService
    {
        void AddUsersByIdAndUserIds(int id, IEnumerable<string> userIds);

        void RemoveUsersByIdAndUserIds(int id, IEnumerable<string> userIds);
    }
}