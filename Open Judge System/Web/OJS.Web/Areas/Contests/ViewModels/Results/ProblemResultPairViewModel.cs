namespace OJS.Web.Areas.Contests.ViewModels.Results
{
    using System;
    using System.Linq.Expressions;

    using OJS.Common.Models;
    using OJS.Data.Models;

    public class ProblemResultPairViewModel
    {
        public static Expression<Func<ParticipantScore, ProblemResultPairViewModel>> FromParticipantScoreAsSimpleResult =>
            score => new ProblemResultPairViewModel
            {
                ProblemId = score.ProblemId,
                ShowResult = score.Problem.ShowResults,
                BestSubmission = new BestSubmissionViewModel
                {
                    Id = score.SubmissionId,
                    Points = score.Points
                }
            };

        public static Expression<Func<ParticipantScore, ProblemResultPairViewModel>> FromParticipantScoreAsFullResult =>
            score => new ProblemResultPairViewModel
            {
                ProblemId = score.ProblemId,
                ShowResult = score.Problem.ShowResults,
                MaximumPoints = score.Problem.MaximumPoints,
                BestSubmission = new BestSubmissionViewModel
                {
                    Id = score.SubmissionId,
                    Points = score.Points,
                    IsCompiledSuccessfully = score.Submission != null &&
                        score.Submission.IsCompiledSuccessfully,
                    SubmissionType = score.Submission != null
                        ? score.Submission.SubmissionType.Name
                        : null,
                    TestRunsCache = score.Submission != null
                        ? score.Submission.TestRunsCache
                        : null
                }
            };

        public static Expression<Func<ParticipantScore, ProblemResultPairViewModel>> FromParticipantScoreAsExportResult =>
            score => new ProblemResultPairViewModel
            {
                ProblemId = score.ProblemId,
                ShowResult = score.Problem.ShowResults,
                IsExcludedFromHomework = score.Problem.ProblemGroup.Type == ProblemGroupType.ExcludedFromHomework,
                BestSubmission = new BestSubmissionViewModel
                {
                    Id = score.SubmissionId,
                    Points = score.Points
                }
            };

        public int ProblemId { get; set; }

        public bool ShowResult { get; set; }

        public bool IsExcludedFromHomework { get; set; }

        public int MaximumPoints { get; set; }

        public BestSubmissionViewModel BestSubmission { get; set; }
    }
}