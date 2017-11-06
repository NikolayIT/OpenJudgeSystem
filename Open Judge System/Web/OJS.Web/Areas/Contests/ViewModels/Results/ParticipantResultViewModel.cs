namespace OJS.Web.Areas.Contests.ViewModels.Results
{
    using System.Collections.Generic;
    using System.Linq;

    public class ParticipantResultViewModel
    {
        public string ParticipantUsername { get; set; }

        public string ParticipantFullName { get; set; }

        public string ParticipantFirstName { get; set; }

        public string ParticipantLastName { get; set; }

        public IEnumerable<ProblemResultPairViewModel> ProblemResults { get; set; }

        public int Total { get; set; }

        public int AdminTotal => this.ProblemResults.Sum(pr => pr.BestSubmission?.Points ?? 0);
    }
}