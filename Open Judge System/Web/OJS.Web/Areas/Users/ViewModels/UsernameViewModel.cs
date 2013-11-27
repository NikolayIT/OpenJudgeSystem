namespace OJS.Web.Areas.Users.ViewModels
{
    using System.ComponentModel.DataAnnotations;
    using CompareMvc = System.Web.Mvc.CompareAttribute;

    public class UsernameViewModel
    {
        [StringLength(
        15,
        ErrorMessageResourceName = "Username_validation",
        ErrorMessageResourceType = typeof(Resources.Account.ViewModels),
        MinimumLength = 5)]
        [Required(
                ErrorMessageResourceName = "Username_required",
                ErrorMessageResourceType = typeof(Resources.Account.ViewModels))]
        [Display(
                Name = "Username",
                ResourceType = typeof(Resources.Account.ViewModels))]
        [RegularExpression(
                @"^[a-zA-Z]([/._]?[a-zA-Z0-9]+)+$",
                ErrorMessageResourceName = "Username_regex_validation",
                ErrorMessageResourceType = typeof(Resources.Account.ViewModels))]
        public string Username { get; set; }

        [CompareMvc("Username")]
        public string UsernameConfirmation { get; set; }
    }
}