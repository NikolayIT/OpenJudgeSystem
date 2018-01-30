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

        IQueryable<ExamGroup> GetAll();

        IQueryable<UserProfile> GetUsersByIdQuery(int id);

        IQueryable<ExamGroup> GetByIdQuery(int id);

        void RemoveUserByIdAndUser(int id, string userId);

        void RemoveReferencesFromContestByContest(int contestId);
    }
}