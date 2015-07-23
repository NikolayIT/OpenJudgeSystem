namespace OJS.Web.Areas.Contests.ViewModels.Results
{
    using System.Collections.Generic;
    using System.Linq;

    public class ParticipantFullResultViewModel
    {
        public string ParticipantUsername { get; set; }

        public string ParticipantFirstName { get; set; }

        public string ParticipantLastName { get; set; }

        public string ParticipantFullName
        {
            get
            {
                return string.Format("{0} {1}", this.ParticipantFirstName, this.ParticipantLastName).Trim();
            }
        }

        public IEnumerable<ProblemFullResultViewModel> ProblemResults { get; set; }

        public int Total
        {
            get
            {
                return
                    this.ProblemResults.Where(problemResult => problemResult.BestSubmission != null)
                        .Sum(problemResult => problemResult.BestSubmission.Points);
            }
        }
    }
}
