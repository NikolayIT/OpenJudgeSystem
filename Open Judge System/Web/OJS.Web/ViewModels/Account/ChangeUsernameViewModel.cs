namespace OJS.Web.ViewModels.Account
{
    using System.ComponentModel.DataAnnotations;

    using Resource = Resources.Account.AccountViewModels;

    public class ChangeUsernameViewModel
    {
        [StringLength(15, ErrorMessageResourceName = "Username_validation",
            ErrorMessageResourceType = typeof(Resource), MinimumLength = 5)]
        [Required(ErrorMessageResourceName = "Username_required",
            ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "Username", ResourceType = typeof(Resource))]
        [RegularExpression(@"^[a-zA-Z]([/._]?[a-zA-Z0-9]+)+$", ErrorMessageResourceName = "Username_regex_validation",
            ErrorMessageResourceType = typeof(Resource))]
        public string Username { get; set; }

        [Compare("Username", ErrorMessageResourceName = "Username_confirmation_incorrect",
            ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "Username_confirmation", ResourceType = typeof(Resource))]
        public string UsernameConfirmation { get; set; }
    }
}
