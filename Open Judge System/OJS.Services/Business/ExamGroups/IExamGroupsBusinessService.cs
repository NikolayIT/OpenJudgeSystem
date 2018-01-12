namespace OJS.Services.Business.ExamGroups
{
    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IExamGroupsBusinessService : IService
    {
        void AddOrUpdate(ExamGroup examGroup);
    }
}