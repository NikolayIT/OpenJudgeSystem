namespace OJS.Web.Areas.Contests.ViewModels.Contests
{
    using System;

    public class OnlineContestConfirmViewModel
    {
        public int ContesId { get; set; }

        public string ContestName { get; set; }

        public TimeSpan ContestDuration { get; set; }

        public int ProblemGroupsCount { get; set; }
    }
}