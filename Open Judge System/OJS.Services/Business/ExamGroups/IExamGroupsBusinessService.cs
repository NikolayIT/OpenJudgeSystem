namespace OJS.Services.Business.ExamGroups
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OJS.Services.Common;

    public interface IExamGroupsBusinessService : IService
    {
        Task AddUsersByIdAndUserIds(int id, IEnumerable<string> userIds);
    }
}