namespace OJS.Web.Areas.Contests.ViewModels.Results
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    
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
                var totalPoints = 0;
                foreach (var problemResult in this.ProblemResults)
                {
                    if (problemResult.BestSubmission != null)
                    {
                        totalPoints += problemResult.BestSubmission.Points;
                    }
                }

                return totalPoints;
            }
        }
    }
}
