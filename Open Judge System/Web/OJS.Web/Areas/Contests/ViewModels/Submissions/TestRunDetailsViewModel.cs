namespace OJS.Web.Areas.Contests.ViewModels.Submissions
{
    using OJS.Common.Models;

    public class TestRunDetailsViewModel
    {
        public bool IsTrialTest { get; set; }

        public int Order { get; set; }

        public TestRunResultType ResultType { get; set; }

        public string ExecutionComment { get; set; }

        public string CheckerComment { get; set; }

        public int TimeUsed { get; set; }

        public long MemoryUsed { get; set; }

        public int Id { get; set; }
    }
}