namespace OJS.Web.Areas.Contests.ViewModels.Results
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    using OJS.Data.Models;

    public class SubmissionFullResultsViewModel
    {
        private IEnumerable<TestRunFullResultsViewModel> testRuns;

        public static Expression<Func<Submission, SubmissionFullResultsViewModel>> FromSubmission
        {
            get
            {
                return submission => new SubmissionFullResultsViewModel
                {
                    Id = submission.Id,
                    SubmissionType = submission.SubmissionType.Name,
                    TestRunCache = submission.TestRunsCache,
                    Points = submission.Points,
                    IsCompiledSuccessfully = submission.IsCompiledSuccessfully,
                    CreatedOn = submission.CreatedOn
                };
            }
        }

        public int Id { get; set; }

        public string SubmissionType { get; set; }

        public string TestRunCache { get; set; }

        public IEnumerable<TestRunFullResultsViewModel> TestRuns
        {
            get
            {
                if (this.testRuns == null)
                {
                    this.testRuns = TestRunFullResultsViewModel.FromCache(this.TestRunCache);
                }

                return this.testRuns;
            }
        }
        
        public int Points { get; set; }

        public bool IsCompiledSuccessfully { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
