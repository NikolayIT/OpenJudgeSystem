using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OJS.Web.Areas.Contests.ViewModels.Results
{
    public class ParticipantFullResultViewModel
    {
        public string ParticipantName { get; set; }

        public IEnumerable<ProblemFullResultViewModel> ProblemResults { get; set; }

        public int Total
        {
            get
            {
                int totalPoints = 0;
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
