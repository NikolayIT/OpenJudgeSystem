namespace OJS.Web.Areas.Contests.ViewModels.Submissions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using OJS.Data.Models;
    using OJS.Workers.Common.Extensions;

    public class SubmissionDetailsViewModel
    {
        public static Expression<Func<Submission, SubmissionDetailsViewModel>> FromSubmission =>
            submission => new SubmissionDetailsViewModel
            {
                Id = submission.Id,
                UserId = submission.Participant.UserId,
                ProblemId = submission.ProblemId,
                ContestId = submission.Problem.ProblemGroup.ContestId,
                UserName = submission.Participant.User.UserName,
                ProblemName = submission.Problem.Name,
                CompilerComment = submission.CompilerComment,
                FileExtension = submission.FileExtension,
                Content = submission.Content,
                Points = submission.Points,
                MaxPoints = submission.Problem.MaximumPoints,
                CreatedOn = submission.CreatedOn,
                ModifiedOn = submission.ModifiedOn,
                SubmissionType = submission.SubmissionType,
                IsCompiledSuccessfully = submission.IsCompiledSuccessfully,
                IsDeleted = submission.IsDeleted,
                Processed = submission.Processed,
                ProcessingComment = submission.ProcessingComment,
                ShowResults = submission.Problem.ShowResults,
                ShowDetailedFeedback = submission.Problem.ShowDetailedFeedback,
                TotalTests = submission.Problem.Tests.Count,
                TestRuns = submission.TestRuns.AsQueryable().Select(TestRunDetailsViewModel.FromTestRun)
            };

        public int Id { get; set; }

        public string UserId { get; set; }

        public int? ProblemId { get; set; }

        public int ContestId { get; set; }

        public string UserName { get; set; }

        public string ProblemName { get; set; }

        public string CompilerComment { get; set; }

        public string FileExtension { get; set; }

        public byte[] Content { get; set; }

        public int Points { get; set; }

        public short MaxPoints { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public SubmissionType SubmissionType { get; set; }

        public bool IsCompiledSuccessfully { get; set; }

        public bool IsDeleted { get; set; }

        public bool Processed { get; set; }

        public string ProcessingComment { get; set; }

        public bool ShowResults { get; set; }

        public bool ShowDetailedFeedback { get; set; }

        public int TotalTests { get; set; }

        public IEnumerable<TestRunDetailsViewModel> TestRuns { get; set; }

        public bool UserHasAdminPermission { get; set; }

        public bool IsContestActive { get; set; }

        public int ProblemIndexInContest { get; set; }

        public bool IsBinaryFile => !string.IsNullOrWhiteSpace(this.FileExtension);

        public string ContentAsString => this.IsBinaryFile ? "Binary file." : this.Content.Decompress();

        public bool HasTestRuns => this.TestRuns.Any();
    }
}