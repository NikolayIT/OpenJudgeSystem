namespace OJS.Web.Areas.Contests.ViewModels.Participants
{
    using System;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Web.Areas.Contests.ViewModels.Contests;

    public class ParticipantViewModel
    {
        private readonly DateTime? participationEndTime;

        public ParticipantViewModel(Participant participant, bool official, bool isAdminOrLecturer)
        {
            this.Contest = ContestViewModel.FromContest.Compile()(participant.Contest);
            this.LastSubmissionTime = participant.Submissions.Any()
                ? (DateTime?)participant.Submissions.Max(x => x.CreatedOn)
                : null;
            this.ContestIsCompete = official;
            this.participationEndTime = participant.ParticipationEndTime;

            if (official &&
                !isAdminOrLecturer &&
                this.Contest.IsOnline)
            {
                this.Contest.Problems = participant.Problems
                    .AsQueryable()
                    .OrderBy(p => p.ProblemGroup.OrderBy)
                    .ThenBy(p => p.OrderBy)
                    .ThenBy(p => p.Name)
                    .Select(ContestProblemViewModel.FromProblem);
            }
        }

        public ContestViewModel Contest { get; set; }

        public DateTime? LastSubmissionTime { get; set; }

        public bool ContestIsCompete { get; set; }

        public double? RemainingTimeInMilliseconds
        {
            get
            {
                if (this.Contest.IsOnline && this.ContestIsCompete && this.participationEndTime.HasValue)
                {
                    return (this.participationEndTime.Value - DateTime.Now).TotalMilliseconds;
                }

                return null;
            }
        }
    }
}