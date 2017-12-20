namespace OJS.Web.Areas.Administration.ViewModels.Contest
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using OJS.Data.Models;

    using Resource = Resources.Areas.Administration.Contests.Views.ChangeTime;

    public class ChangeTimeForParticipantsViewModel
    {
        public static Expression<Func<Contest, ChangeTimeForParticipantsViewModel>> FromContest =>
            contest => new ChangeTimeForParticipantsViewModel
            {
                ContesId = contest.Id,
                ContestName = contest.Name
            };

        [HiddenInput(DisplayValue = false)]
        public int ContesId { get; set; }

        [Display(Name = "Contest", ResourceType = typeof(Resource))]
        public string ContestName { get; set; }

        [Display(Name = "Time_in_minutes", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceName = "Time_required_error", ErrorMessageResourceType = typeof(Resource))]
        public int TimeInMinutes { get; set; }
    }
}