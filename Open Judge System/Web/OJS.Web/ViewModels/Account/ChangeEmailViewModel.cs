namespace OJS.Web.ViewModels.Account
{
    using System.ComponentModel.DataAnnotations;

    public class ChangeEmailViewModel
    {
        [StringLength(int.MaxValue, MinimumLength = 1, ErrorMessageResourceName = "Password_required",
            ErrorMessageResourceType = typeof(Resources.Account.ViewModels))]
        [Required(ErrorMessageResourceName = "Password_required",
            ErrorMessageResourceType = typeof(Resources.Account.ViewModels))]
        [Display(Name = "Password", ResourceType = typeof(Resources.Account.ViewModels))]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessageResourceName = "Email_required",
            ErrorMessageResourceType = typeof(Resources.Account.ViewModels))]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email", ResourceType = typeof(Resources.Account.ViewModels))]
        [EmailAddress(ErrorMessage = null, ErrorMessageResourceName = "Email_required",
            ErrorMessageResourceType = typeof(Resources.Account.ViewModels))]
        public string Email { get; set; }

        [Required(ErrorMessageResourceName = "Email_confirmation_required",
            ErrorMessageResourceType = typeof(Resources.Account.ViewModels))]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email_confirm", ResourceType = typeof(Resources.Account.ViewModels))]
        [EmailAddress(ErrorMessage = null, ErrorMessageResourceName = "Email_confirmation_required",
            ErrorMessageResourceType = typeof(Resources.Account.ViewModels))]
        [Compare("Email", ErrorMessageResourceName = "Email_confirmation_invalid",
            ErrorMessageResourceType = typeof(Resources.Account.ViewModels))]
        public string EmailConfirmation { get; set; }
    }
}
