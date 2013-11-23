namespace OJS.Web.Areas.Contests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using OJS.Data.Models;

    public class ParticipantViewModel
    {
        public ParticipantViewModel(Participant participant)
        {
            this.Contest = ContestViewModel.FromContest.Compile()(participant.Contest);
            this.LastSubmissionTime = participant.Submissions.Any() ? (DateTime?)participant.Submissions.Max(x => x.CreatedOn) : null;
        }

        public ContestViewModel Contest { get; set; }

        public DateTime? LastSubmissionTime { get; set; }
    }
}