namespace OJS.Web.Areas.Contests.ViewModels.Submissions
{
    using System.Collections.Generic;

    using OJS.Data.Models;

    public class SubmissionDetailsViewModel : Submission
    {
        public new IEnumerable<TestRunDetailsViewModel> TestRuns { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public string ProblemName { get; set; }
    }
}