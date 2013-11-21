namespace OJS.Web.Areas.Contests.ViewModels.Submissions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    
    using OJS.Common.Extensions;
    using OJS.Data.Models;

    public class SubmissionDetailsViewModel
    {
        public static Expression<Func<Submission, SubmissionDetailsViewModel>> FromSubmission
        {
            get
            {
                return sub => new SubmissionDetailsViewModel
                {
                    Id = sub.Id,
                    UserId = sub.Participant.UserId,
                    UserName = sub.Participant.User.UserName,
                    CompilerComment = sub.CompilerComment,
                    Content = sub.Content,
                    CreatedOn = sub.CreatedOn,
                    IsCompiledSuccessfully = sub.IsCompiledSuccessfully,
                    IsDeleted = sub.IsDeleted,
                    Points = sub.Points,
                    Processed = sub.Processed,
                    Processing = sub.Processing,
                    ProblemId = sub.ProblemId,
                    ProblemName = sub.Problem.Name,
                    ProcessingComment = sub.ProcessingComment,
                    SubmissionType = sub.SubmissionType,
                    TestRuns = sub.TestRuns.AsQueryable().Select(TestRunDetailsViewModel.FromTestRun)
                };
            }
        }

        public IEnumerable<TestRunDetailsViewModel> TestRuns { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public string ProblemName { get; set; }

        public int Id { get; set; }

        public string CompilerComment { get; set; }

        public byte[] Content { get; set; }

        public string ContentAsString
        {
            get
            {
                return this.Content.Decompress();
            }
        }

        public DateTime CreatedOn { get; set; }

        public bool IsCompiledSuccessfully { get; set; }

        public bool IsDeleted { get; set; }

        public int Points { get; set; }

        public bool Processed { get; set; }

        public bool Processing { get; set; }

        public int? ProblemId { get; set; }

        public string ProcessingComment { get; set; }

        public SubmissionType SubmissionType { get; set; }
    }
}