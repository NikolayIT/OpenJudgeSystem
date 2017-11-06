namespace OJS.Web.Areas.Contests.ViewModels.Results
{
    using OJS.Data.Models;

    public class ProblemResultPairViewModel
    {
        public int Id { get; set; }

        public bool ShowResult { get; set; }

        public int MaximumPoints { get; set; }

        public BestSubmissionViewModel BestSubmission { get; set; }
    }
}