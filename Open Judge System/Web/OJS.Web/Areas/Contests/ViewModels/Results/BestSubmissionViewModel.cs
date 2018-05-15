namespace OJS.Web.Areas.Contests.ViewModels.Results
{
    using System.Collections.Generic;

    public class BestSubmissionViewModel
    {
        public int? Id { get; set; }

        public int Points { get; set; }

        public bool IsCompiledSuccessfully { get; set; }

        public string SubmissionType { get; set; }

        public string TestRunsCache { get; set; }

        public IEnumerable<TestRunFullResultsViewModel> TestRuns =>
            TestRunFullResultsViewModel.FromCache(this.TestRunsCache);
    }
}