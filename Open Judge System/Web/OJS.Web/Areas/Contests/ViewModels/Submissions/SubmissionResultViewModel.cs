namespace OJS.Web.Areas.Contests.ViewModels.Submissions
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Linq.Expressions;

    using OJS.Data.Models;
    using OJS.Web.ViewModels.TestRun;

    using Resource = Resources.Areas.Contests.ViewModels.SubmissionsViewModels;

    public class SubmissionResultViewModel
    {
        public SubmissionResultViewModel()
        {
            this.TestRuns = new HashSet<TestRunViewModel>();
        }

        public static Expression<Func<Submission, SubmissionResultViewModel>> FromSubmission
        {
            get
            {
                return submission => new SubmissionResultViewModel
                {
                    TestRuns = submission.TestRuns.AsQueryable().OrderBy(tr => tr.Test.OrderBy).Select(TestRunViewModel.FromTestRun),
                    SubmissionDate = submission.CreatedOn,
                    MaximumPoints = submission.Problem.MaximumPoints,
                    SubmissionId = submission.Id,
                    IsCalculated = submission.Processed,
                    SubmissionPoints = submission.Points,
                    IsCompiledSuccessfully = submission.IsCompiledSuccessfully,
                    IsOfficial = submission.Participant.IsOfficial,
                    ProblemId = submission.ProblemId
                };
            }
        }

        public int SubmissionId { get; set; }

        [Display(Name = "Problem", ResourceType = typeof(Resource))]
        public int? ProblemId { get; set; }

        [Display(Name = "Type", ResourceType = typeof(Resource))]
        public bool IsOfficial { get; set; }

        public short MaximumPoints { get; set; }

        public IEnumerable<TestRunViewModel> TestRuns { get; set; }

        [Display(Name = "Submission_date", ResourceType = typeof(Resource))]
        public DateTime SubmissionDate { get; set; }

        public bool IsCalculated { get; set; }

        [Display(Name = "Maximum_memory", ResourceType = typeof(Resource))]
        public long? MaximumMemoryUsed
        {
            get
            {
                if (!this.TestRuns.Any())
                {
                    return null;
                }

                return this.TestRuns.Max(x => x.MemoryUsed);
            }
        }

        [Display(Name = "Maximum_time", ResourceType = typeof(Resource))]
        public int? MaximumTimeUsed
        {
            get
            {
                if (!this.TestRuns.Any())
                {
                    return null;
                }

                return this.TestRuns.Max(x => x.TimeUsed);
            }
        }

        [Display(Name = "Points", ResourceType = typeof(Resource))]
        public int? SubmissionPoints { get; set; }

        [Display(Name = "Is_compiled_successfully", ResourceType = typeof(Resource))]
        public bool IsCompiledSuccessfully { get; set; }
    }
}