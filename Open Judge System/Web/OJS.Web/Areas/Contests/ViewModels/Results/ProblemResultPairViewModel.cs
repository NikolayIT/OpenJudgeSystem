namespace OJS.Web.Areas.Contests.ViewModels.Results
{
    public class ProblemResultPairViewModel
    {
        public int Id { get; set; }

        public bool ShowResult { get; set; }

        public BestSubmissionViewModel BestSubmission { get; set; }

        public string ProblemName { get; set; }
    }
}