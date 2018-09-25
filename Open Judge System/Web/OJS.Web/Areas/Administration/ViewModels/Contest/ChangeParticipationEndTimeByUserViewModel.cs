namespace OJS.Web.Areas.Administration.ViewModels.Contest
{
    using System.ComponentModel.DataAnnotations;

    using Resource = Resources.Areas.Administration.Contests.Views.ChangeTime;

    public class ChangeParticipationEndTimeByUserViewModel : ChangeParticipationEndTimeViewModel
    {
        [UIHint("UsersSimpleComboBox")]
        [Required(
            ErrorMessageResourceName = nameof(Resource.Username_required),
            ErrorMessageResourceType = typeof(Resource))]
        public string UserId { get; set; }
    }
}