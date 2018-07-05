namespace OJS.Web.Areas.Contests.ViewModels.Contests
{
    using System.ComponentModel.DataAnnotations;

    public class NewContestIpViewModel
    {
        public int ContestId { get; set; }

        [Display(Name = "Парола за ново IP за състезание")]
        [Required(ErrorMessage = "Моля, въведете парола.")]
        public string NewIpPassword { get; set; }
    }
}