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
                return submission => new SubmissionDetailsViewModel
                {
                    Id = submission.Id,
                    UserId = submission.Participant.UserId,
                    UserName = submission.Participant.User.UserName,
                    CompilerComment = submission.CompilerComment,
                    Content = submission.Content,
                    FileExtension = submission.FileExtension,
                    CreatedOn = submission.CreatedOn,
                    IsCompiledSuccessfully = submission.IsCompiledSuccessfully,
                    IsDeleted = submission.IsDeleted,
                    Points = submission.Points,
                    Processed = submission.Processed,
                    Processing = submission.Processing,
                    ProblemId = submission.ProblemId,
                    ProblemName = submission.Problem.Name,
                    ProcessingComment = submission.ProcessingComment,
                    SubmissionType = submission.SubmissionType,
                    TestRuns = submission.TestRuns.AsQueryable().Select(TestRunDetailsViewModel.FromTestRun),
                    ShowResults = submission.Problem.ShowResults,
                    ShowDetailedFeedback = submission.Problem.ShowDetailedFeedback
                };
            }
        }

        public IEnumerable<TestRunDetailsViewModel> TestRuns { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public string ProblemName { get; set; }

        public int Id { get; set; }

        public string CompilerComment { get; set; }

        public string FileExtension { get; set; }

        public byte[] Content { get; set; }

        public bool IsBinaryFile => !string.IsNullOrWhiteSpace(this.FileExtension);

        public string ContentAsString => this.IsBinaryFile ? "Binary file." : this.Content.Decompress();

        public DateTime CreatedOn { get; set; }

        public bool IsCompiledSuccessfully { get; set; }

        public bool IsDeleted { get; set; }

        public int Points { get; set; }

        public bool Processed { get; set; }

        public bool Processing { get; set; }

        public int? ProblemId { get; set; }

        public string ProcessingComment { get; set; }

        public SubmissionType SubmissionType { get; set; }

        public bool ShowResults { get; set; }

        public bool ShowDetailedFeedback { get; set; }

        public bool UserHasAdminPermission { get; set; }

        public bool HasTestRuns => this.TestRuns.Any();
    }
}