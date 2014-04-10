namespace OJS.Web.Areas.Contests.ViewModels.Participants
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using OJS.Data.Models;
    using OJS.Web.Areas.Contests.ViewModels.Contests;

    public class ParticipantViewModel
    {
        public ParticipantViewModel(Participant participant, bool official)
        {
            this.Contest = ContestViewModel.FromContest.Compile()(participant.Contest);
            this.LastSubmissionTime = participant.Submissions.Any() ? (DateTime?)participant.Submissions.Max(x => x.CreatedOn) : null;
            this.ContestIsCompete = official;
        }

        public ContestViewModel Contest { get; set; }

        public DateTime? LastSubmissionTime { get; set; }

        public bool ContestIsCompete { get; set; }
    }
}