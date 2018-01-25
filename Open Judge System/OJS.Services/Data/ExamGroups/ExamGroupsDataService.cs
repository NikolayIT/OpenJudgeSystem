namespace OJS.Services.Data.ExamGroups
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class ExamGroupsDataService : IExamGroupsDataService
    {
        private readonly IEfGenericRepository<ExamGroup> examGroups;

        public ExamGroupsDataService(IEfGenericRepository<ExamGroup> examGroups) =>
            this.examGroups = examGroups;

        public void Add(ExamGroup examGroup)
        {
            this.examGroups.Add(examGroup);
            this.examGroups.SaveChanges();
        }

        public void Update(ExamGroup examGroup)
        {
            this.examGroups.Update(examGroup);
            this.examGroups.SaveChanges();
        }

        public ExamGroup GetById(int id) => this.examGroups.GetById(id);

        public ExamGroup GetByExternalIdAndAppId(int? externalId, string appId) =>
            this.examGroups
                .All()
                .FirstOrDefault(eg => eg.ExternalExamGroupId == externalId && eg.ExternalAppId == appId);

        public int? GetIdByExternalIdAndAppId(int? externalId, string appId) =>
            this.examGroups
                .All()
                .Where(eg => eg.ExternalExamGroupId == externalId && eg.ExternalAppId == appId)
                .Select(eg => eg.Id)
                .FirstOrDefault();
    }
}