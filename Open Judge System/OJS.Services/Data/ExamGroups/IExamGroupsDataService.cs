namespace OJS.Services.Data.ExamGroups
{
    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IExamGroupsDataService : IService
    {
        void Add(ExamGroup examGroup);

        void Update(ExamGroup examGroup);

        ExamGroup GetByExternalIdAndAppTenant(int? externalId, string appTenant);
    }
}