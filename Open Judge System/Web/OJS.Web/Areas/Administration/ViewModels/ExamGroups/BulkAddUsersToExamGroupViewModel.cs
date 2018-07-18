namespace OJS.Web.Areas.Administration.ViewModels.ExamGroups
{
    public class BulkAddUsersToExamGroupViewModel
    {
        public BulkAddUsersToExamGroupViewModel()
        {
        }

        public BulkAddUsersToExamGroupViewModel(int examGroupId) =>
            this.ExamGroupId = examGroupId;

        public string UserNamesText { get; set; }

        public int ExamGroupId { get; set; }
    }
}