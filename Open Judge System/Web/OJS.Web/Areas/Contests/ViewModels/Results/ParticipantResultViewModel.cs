namespace OJS.Web.Areas.Contests.ViewModels.Results
{
    using System.Collections.Generic;
    using System.Linq;

    public class ParticipantResultViewModel
    {
        public string ParticipantUsername { get; set; }

        public string ParticipantFirstName { get; set; }

        public string ParticipantLastName { get; set; }

        public string ParticipantFullName
        {
            get
            {
                return string.Format("{0} {1}", ParticipantFirstName, ParticipantLastName).Trim();
            }
        }

        public IEnumerable<ProblemResultPairViewModel> ProblemResults { get; set; }

        public int Total
        {
            get
            {
                return this.ProblemResults.Where(x => x.ShowResult).Sum(x => x.BestSubmission == null ? 0 : x.BestSubmission.Points);
            }
        }

        public int AdminTotal
        {
            get
            {
                return this.ProblemResults.Sum(x => x.BestSubmission == null ? 0 : x.BestSubmission.Points);
            }
        }
    }
}