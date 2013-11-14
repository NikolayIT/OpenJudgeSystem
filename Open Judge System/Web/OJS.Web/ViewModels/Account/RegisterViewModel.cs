namespace OJS.Web.ViewModels.Account
{
    using System.ComponentModel.DataAnnotations;

    public class RegisterViewModel
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
        public string UserName { get; set; }

        [Required(
                ErrorMessageResourceName = "Enter_password",
                ErrorMessageResourceType = typeof(Resources.Account.ViewModels))]
        [StringLength(
                100,
                ErrorMessageResourceName = "Password_length_validation_message",
                ErrorMessageResourceType = typeof(Resources.Account.ViewModels),
                MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(
                Name = "Password",
                ResourceType = typeof(Resources.Account.ViewModels))]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(
                Name = "Password_confirm",
                ResourceType = typeof(Resources.Account.ViewModels))]
        [Compare("Password",
                ErrorMessageResourceName = "Passwords_dont_match",
                ErrorMessageResourceType = typeof(Resources.Account.ViewModels))]
        public string ConfirmPassword { get; set; }

        [Required(
                ErrorMessageResourceName = "Email_required",
                ErrorMessageResourceType = typeof(Resources.Account.ViewModels))]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(
                ErrorMessage = null,
                ErrorMessageResourceName = "Email_invalid",
                ErrorMessageResourceType = typeof(Resources.Account.ViewModels))]
        [Display(
                Name = "Email",
                ResourceType = typeof(Resources.Account.ViewModels))]
        public string Email { get; set; }
    }
}