namespace OJS.Services.Business.ExamGroups
{
    using OJS.Data.Models;
    using OJS.Services.Data.ExamGroups;

    public class ExamGroupsBusinessService : IExamGroupsBusinessService
    {
        private readonly IExamGroupsDataService examGroupsData;

        public ExamGroupsBusinessService(IExamGroupsDataService examGroupsData) =>
            this.examGroupsData = examGroupsData;

        public void AddOrUpdate(ExamGroup examGroup)
        {
            var judgeExamGroup = this.examGroupsData
                .GetByExternalIdAndAppTenant(examGroup.ExternalExamGroupId, examGroup.AppTenant);

            if (judgeExamGroup == null)
            {
                this.examGroupsData.Add(examGroup);
            }
            else
            {
                this.examGroupsData.Update(judgeExamGroup);
            }
        }
    }
}