namespace OJS.Web.Areas.Contests.ViewModels.Results
{
    public class ProblemResultPairViewModel
    {
        public int ProblemId { get; set; }

        public bool ShowResult { get; set; }

        public int MaximumPoints { get; set; }

        public BestSubmissionViewModel BestSubmission { get; set; }

        public bool IsPartOfUserProblems { get; set; }
    }
}