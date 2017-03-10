namespace OJS.Web.Areas.Contests.ViewModels.Results
{
    public class ProblemFullResultViewModel
    {
        public string ProblemName { get; set; }

        public int ProblemOrderBy { get; set; }

        public SubmissionFullResultsViewModel BestSubmission { get; set; }

        public int MaximumPoints { get; set; }
    }
}
