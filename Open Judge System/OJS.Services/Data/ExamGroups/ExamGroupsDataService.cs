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

        public ExamGroup GetByExternalIdAndAppTenant(int? externalId, string appTenant) =>
            this.examGroups
            .All()
            .FirstOrDefault(eg => eg.ExternalExamGroupId == externalId && eg.AppTenant == appTenant);
    }
}