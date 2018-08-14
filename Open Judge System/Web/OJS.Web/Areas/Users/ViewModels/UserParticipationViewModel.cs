namespace OJS.Web.Areas.Users.ViewModels
{
    using System;

    public class UserParticipationViewModel
    {
        public int ContestId { get; set; }

        public string ContestName { get; set; }

        public int? CompeteResult { get; set; }

        public int? PracticeResult { get; set; }

        public int? ContestCompeteMaximumPoints { get; set; }

        public int? ContestPracticeMaximumPoints { get; set; }

        public DateTime RegistrationTime { get; set; }
    }
}