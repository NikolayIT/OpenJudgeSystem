namespace OJS.Services.Data.ExamGroups
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IExamGroupsDataService : IService
    {
        void Add(ExamGroup examGroup);

        void Update(ExamGroup examGroup);

        ExamGroup GetById(int id);
        
        ExamGroup GetByExternalIdAndAppId(int? externalId, string appId);

        int? GetIdByExternalIdAndAppId(int? externalId, string appId);

        IQueryable<ExamGroup> All();

        IQueryable<UserProfile> GetUsersById(int id);
    }
}