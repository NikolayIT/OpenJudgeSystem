namespace OJS.Web.Areas.Contests.ViewModels.Results
{
    using System;
    using System.Linq.Expressions;

    using OJS.Data.Models;

    public class ProblemResultPairViewModel
    {    
        public ProblemResultPairViewModel(ParticipantScore partScore, bool isForFullResults)
        {
            if (isForFullResults)
            {
                this.Id = partScore.ProblemId;
                this.MaximumPoints = partScore.Problem.MaximumPoints;
                this.ShowResult = partScore.Problem.ShowResults;
                this.BestSubmission = new BestSubmissionViewModel
                {
                    Id = partScore.SubmissionId,
                    Points = partScore.Points,
                    IsCompiledSuccessfully = partScore.Submission.IsCompiledSuccessfully,
                    TestRuns = TestRunFullResultsViewModel.FromCache(partScore.Submission.TestRunsCache)
                };
            }
            else
            {
                this.Id = partScore.ProblemId;
                this.ShowResult = partScore.Problem.ShowResults;
                this.BestSubmission = new BestSubmissionViewModel
                {
                    Id = partScore.SubmissionId,
                    Points = partScore.Points
                };
            }
        }

        public int Id { get; set; }

        public bool ShowResult { get; set; }

        public int MaximumPoints { get; set; }

        public BestSubmissionViewModel BestSubmission { get; set; }
    }
}