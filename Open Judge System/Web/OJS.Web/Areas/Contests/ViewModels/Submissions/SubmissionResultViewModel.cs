namespace OJS.Web.Areas.Contests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Linq.Expressions;

    using OJS.Common.Models;
    using OJS.Data.Models;
    using OJS.Web.ViewModels.TestRun;

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
                    TestRuns = submission.TestRuns.AsQueryable().Select(TestRunViewModel.FromTestRun),
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

        [Display(Name = "Задача")]
        public int? ProblemId { get; set; }

        public bool IsOfficial { get; set; }

        public short MaximumPoints { get; set; }

        public IEnumerable<TestRunViewModel> TestRuns { get; set; }

        [Display(Name = "Изпратено на")]
        public DateTime SubmissionDate { get; set; }

        public bool IsCalculated { get; set; }

        [Display(Name = "Макс. памет")]
        public long? MaximumMemoryUsed
        {
            get
            {
                if (this.TestRuns.Count() == 0)
                {
                    return null;
                }

                return this.TestRuns.Max(x => x.MemoryUsed);
            }
        }

        [Display(Name = "Макс. време")]
        public int? MaximumTimeUsed
        {
            get
            {
                if (this.TestRuns.Count() == 0)
                {
                    return null;
                }

                return this.TestRuns.Max(x => x.TimeUsed);
            }
        }

        [Display(Name = "Точки")]
        public int? SubmissionPoints { get; set; }

        [Display(Name = "Успешно компилирана")]
        public bool IsCompiledSuccessfully { get; set; }
    }
}