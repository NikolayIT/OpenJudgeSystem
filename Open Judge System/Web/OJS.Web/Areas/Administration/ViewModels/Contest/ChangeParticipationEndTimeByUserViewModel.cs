namespace OJS.Web.Areas.Administration.ViewModels.Contest
{
    using System.ComponentModel.DataAnnotations;

    public class ChangeParticipationEndTimeByUserViewModel : ChangeParticipationEndTimeViewModel
    {
        [UIHint("UsersSimpleComboBox")]
        public string UserId { get; set; }
    }
}