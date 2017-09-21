namespace OJS.Web.Areas.Administration.ViewModels.Contest
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Linq.Expressions;

    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;

    using Resource = Resources.Areas.Administration.Contests.ViewModels.TransferContestParticipants;

    public class TransferParticipantsViewModel
    {
        [ExcludeFromExcel]
        public static Expression<Func<Contest, TransferParticipantsViewModel>> FromContest =>
            contest => new TransferParticipantsViewModel
            {
                Name = contest.Name,
                CategoryName = contest.Category.Name,
                OfficialParticipantsCount = contest.Participants.Count(p => p.IsOfficial)
            };

        [Display(Name = "Name", ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "Name_required",
            ErrorMessageResourceType = typeof(Resource))]
        public string Name { get; set; }

        [Display(Name = "Category_name", ResourceType = typeof(Resource))]
        public string CategoryName { get; set; }

        [Display(Name = "Official_participants_count", ResourceType = typeof(Resource))]
        public int OfficialParticipantsCount { get; set; }
    }
}