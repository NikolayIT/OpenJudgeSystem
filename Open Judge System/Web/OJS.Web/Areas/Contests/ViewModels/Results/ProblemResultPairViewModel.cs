namespace OJS.Web.Areas.Contests.ViewModels.Results
{
    using System;
    using System.Linq.Expressions;

    using OJS.Data.Models;

    public class ProblemResultPairViewModel
    {    
        public int Id { get; set; }

        public string ProblemName { get; set; }

        public bool ShowResult { get; set; }

        public int MaximumPoints { get; set; }

        public BestSubmissionViewModel BestSubmission { get; set; }

        public static Expression<Func<ParticipantScore, ProblemResultPairViewModel>> FromParticipantScore(bool isForFullResults)
        {
            if (isForFullResults)
            {
                return partScore => new ProblemResultPairViewModel
                {
                    Id = partScore.ProblemId,
                    MaximumPoints = partScore.Problem.MaximumPoints,
                    ProblemName = partScore.Problem.Name,
                    BestSubmission = new BestSubmissionViewModel
                    {
                        Id = partScore.SubmissionId,
                        Points = partScore.Points,
                        IsCompiledSuccessfully = partScore.Submission.IsCompiledSuccessfully,
                        TestRuns = TestRunFullResultsViewModel.FromCache(partScore.Submission.TestRunsCache)
                    }
                };
            }

            return partScore => new ProblemResultPairViewModel
            {
                Id = partScore.ProblemId,
                ShowResult = partScore.Problem.ShowResults,
                BestSubmission = new BestSubmissionViewModel
                {
                    Id = partScore.SubmissionId,
                    Points = partScore.Points
                }
            };
        }
    }
}