namespace OJS.Web.Areas.Contests.ViewModels.Results
{
    using System.Collections.Generic;
    using System.Linq;

    public class ParticipantResultViewModel
    {
        public string ParticipantUsername { get; set; }

        public string ParticipantFullName { get; set; }

        public IEnumerable<ProblemResultPairViewModel> ProblemResults { get; set; }
            = new List<ProblemResultPairViewModel>();

        public int Total =>
            this.ProblemResults.Where(x => x.ShowResult).Sum(x => x.BestSubmission?.Points ?? 0);

        public int AdminTotal =>
            this.ProblemResults.Sum(x => x.BestSubmission?.Points ?? 0);
    }
}
