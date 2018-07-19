namespace OJS.Web.Areas.Administration.ViewModels.ExamGroups
{
    public class BulkAddUsersToExamGroupViewModel
    {
        public BulkAddUsersToExamGroupViewModel()
        {
        }

        public BulkAddUsersToExamGroupViewModel(int examGroupId, string examGroupName)
        {
            this.ExamGroupId = examGroupId;
            this.ExamGroupName = examGroupName;
        }

        public string UserNamesText { get; set; }

        public int ExamGroupId { get; set; }

        public string ExamGroupName { get; set; }
    }
}