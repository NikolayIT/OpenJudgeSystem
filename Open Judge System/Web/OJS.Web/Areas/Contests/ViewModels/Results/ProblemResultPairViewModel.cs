namespace OJS.Web.Areas.Contests.ViewModels.Results
{
    public class ProblemResultPairViewModel
    {
        public int Id { get; set; }

        public bool ShowResult { get; set; }

        public int MaximumPoints { get; set; }

        public int OrderBy { get; set; }

        public string ProblemName { get; set; }

        public BestSubmissionViewModel BestSubmission { get; set; }
    }
}