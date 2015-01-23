namespace OJS.Web.Areas.Contests.ViewModels.Results
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ParticipantResultViewModel
    {
        public ParticipantResultViewModel()
        {
            this.ProblemResults = new List<ProblemResultPairViewModel>();
        }

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

        public double? GetContestTimeInMinutes(DateTime? contestStartTime)
        {
            var lastSubmission = this.ProblemResults
                .Where(x => x.ShowResult && x.BestSubmission != null)
                .OrderByDescending(x => x.BestSubmission.CreatedOn)
                .Select(x => x.BestSubmission)
                .FirstOrDefault();

            if (contestStartTime.HasValue && lastSubmission != null)
            {
                var lastSubmissionTime = lastSubmission.CreatedOn;
                var contestTimeInMinutes = (lastSubmissionTime - contestStartTime.Value).TotalMinutes;
                return contestTimeInMinutes;
            }

            return null;
        }
    }
}