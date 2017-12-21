namespace OJS.Web.Areas.Contests.ViewModels.Participants
{
    using System;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Web.Areas.Contests.ViewModels.Contests;

    public class ParticipantViewModel
    {
        public ParticipantViewModel(Participant participant, bool official)
        {
            this.Contest = ContestViewModel.FromContest.Compile()(participant.Contest);
            this.LastSubmissionTime = participant.Submissions.Any() ? (DateTime?)participant.Submissions.Max(x => x.CreatedOn) : null;
            this.ContestIsCompete = official;
            this.ContestEndTime = participant.ContestEndTime;
        }

        public ContestViewModel Contest { get; set; }

        public DateTime? LastSubmissionTime { get; set; }

        public DateTime? ContestEndTime { get; set; }

        public bool ContestIsCompete { get; set; }

        public double? RemainingTimeInMilliseconds
        {
            get
            {
                if (this.ContestEndTime.HasValue)
                {
                    return (this.ContestEndTime.Value - DateTime.Now).TotalMilliseconds;
                }

                return null;
            }
        }
    }
}