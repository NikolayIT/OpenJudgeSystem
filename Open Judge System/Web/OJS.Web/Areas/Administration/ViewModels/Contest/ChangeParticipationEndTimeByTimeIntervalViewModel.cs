namespace OJS.Web.Areas.Administration.ViewModels.Contest
{
    using System;
    using System.ComponentModel.DataAnnotations;

    using Resource = Resources.Areas.Administration.Contests.Views.ChangeTime;

    public class ChangeParticipationEndTimeByTimeIntervalViewModel : ChangeParticipationEndTimeViewModel
    {
        [Display(Name = nameof(Resource.Participants_created_after), ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = nameof(Resource.Time_required_error),
            ErrorMessageResourceType = typeof(Resource))]
        public DateTime? ParticipantsCreatedAfterDateTime { get; set; }

        [Display(Name = nameof(Resource.Participants_created_before), ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = nameof(Resource.Time_required_error),
            ErrorMessageResourceType = typeof(Resource))]
        public DateTime? ParticipantsCreatedBeforeDateTime { get; set; } = DateTime.Now;
    }
}