namespace OJS.Web.ViewModels.Submission
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using OJS.Data.Models;
    using OJS.Web.ViewModels.TestRun;

    public class SubmissionViewModel
    {
        public static Expression<Func<Submission, SubmissionViewModel>> FromSubmission
        {
            get
            {
                return submission => new SubmissionViewModel
                {
                    Id = submission.Id,
                    SubmitedOn = submission.CreatedOn,
                    ProblemId = submission.ProblemId,
                    ProblemName = submission.Problem.Name,
                    ProblemMaximumPoints = submission.Problem.MaximumPoints,
                    Contest = submission.Problem.ProblemGroup.Contest.Name,
                    ParticipantId = submission.ParticipantId,
                    ParticipantName = submission.Participant.User.UserName,
                    SubmissionType = submission.SubmissionType.Name,
                    Processed = submission.Processed,
                    Points = submission.Points,
                    IsCompiledSuccessfully = submission.IsCompiledSuccessfully,
                    TestResults = submission.TestRuns.AsQueryable().Where(x => !x.Test.IsTrialTest).Select(TestRunViewModel.FromTestRun)
                };
            }
        }

        public int Id { get; set; }

        public int? ParticipantId { get; set; }

        public string ParticipantName { get; set; }

        public DateTime SubmitedOn { get; set; }

        public int? ProblemId { get; set; }

        public string ProblemName { get; set; }

        public int ProblemMaximumPoints { get; set; }

        public string Contest { get; set; }

        public string ProgrammingLanguage { get; set; }

        public bool IsCompiledSuccessfully { get; set; }

        public IEnumerable<TestRunViewModel> TestResults { get; set; }

        public bool Processed { get; set; }

        public int Points { get; set; }

        public bool HasFullPoints => this.Points == this.ProblemMaximumPoints;

        public int MaxUsedTime
        {
            get
            {
                return this.TestResults.Any() ? this.TestResults.Select(x => x.TimeUsed).Max() : 0;
            }
        }

        public long MaxUsedMemory
        {
            get
            {
                return this.TestResults.Any() ? this.TestResults.Select(x => x.MemoryUsed).Max() : 0;
            }
        }

        public string SubmissionType { get; set; }
    }
}