namespace OJS.Web.Areas.Administration.ViewModels.AntiCheat
{
    using System;

    public class SubmissionSimilarityViewModel
    {
        public int ProblemId { get; set; }

        public string ProblemName { get; set; }

        public int Points { get; set; }

        public int Differences { get; set; }

        public int FirstSubmissionId { get; set; }

        public int SecondSubmissionId { get; set; }

        public string FirstParticipantName { get; set; }

        public string SecondParticipantName { get; set; }

        public DateTime FirstSubmissionCreatedOn { get; set; }

        public DateTime SecondSubmissionCreatedOn { get; set; }
    }
}