namespace OJS.Web.Areas.Contests.ViewModels.Results
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using OJS.Data.Models;

    public class SubmissionFullResultsViewModel
    {
        public static Expression<Func<Submission, SubmissionFullResultsViewModel>> FromSubmission
        {
            get
            {
                return submission => new SubmissionFullResultsViewModel
                {
                    Id = submission.Id,
                    SubmissionType = submission.SubmissionType.Name,
                    MaxTimeUsed = submission.TestRuns.Max(x => x.TimeUsed),
                    MaxMemoryUsed = submission.TestRuns.Max(x => x.MemoryUsed),
                    TestRuns = submission.TestRuns.AsQueryable().Select(TestRunFullResultsViewModel.FromTestRun),
                    Points = submission.Points,
                    IsCompiledSuccessfully = submission.IsCompiledSuccessfully
                };
            }
        }

        public int Id { get; set; }

        public string SubmissionType { get; set; }

        public IEnumerable<TestRunFullResultsViewModel> TestRuns { get; set; }

        public int? MaxTimeUsed { get; set; }

        public long? MaxMemoryUsed { get; set; }

        public int Points { get; set; }

        public bool IsCompiledSuccessfully { get; set; }
    }
}
