namespace OJS.Web.ViewModels.Account
{
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    using OJS.Common;

    using Resource = Resources.Account.AccountViewModels;

    public class RegisterViewModel
    {
        [StringLength(
            GlobalConstants.UsernameMaxLength,
            MinimumLength = GlobalConstants.UsernameMinLength,
            ErrorMessageResourceName = "Username_validation",
            ErrorMessageResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "Username_required",
            ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "Username", ResourceType = typeof(Resource))]
        [RegularExpression(
            GlobalConstants.UsernameRegEx,
            ErrorMessageResourceName = "Username_regex_validation",
            ErrorMessageResourceType = typeof(Resource))]
        public string UserName { get; set; }

        [AllowHtml]
        [Required(
            ErrorMessageResourceName = "Enter_password",
            ErrorMessageResourceType = typeof(Resource))]
        [StringLength(
            GlobalConstants.PasswordMaxLength,
            MinimumLength = GlobalConstants.PasswordMinLength,
            ErrorMessageResourceName = "Password_length_validation_message",
            ErrorMessageResourceType = typeof(Resource))]
        [DataType(DataType.Password)]
        [Display(
            Name = "Password",
            ResourceType = typeof(Resource))]
        public string Password { get; set; }

        [AllowHtml]
        [DataType(DataType.Password)]
        [Display(Name = "Password_confirm", ResourceType = typeof(Resource))]
        [System.ComponentModel.DataAnnotations.Compare(
            "Password",
            ErrorMessageResourceName = "Passwords_dont_match",
            ErrorMessageResourceType = typeof(Resource))]
        public string ConfirmPassword { get; set; }

        [Required(
            ErrorMessageResourceName = "Email_required",
            ErrorMessageResourceType = typeof(Resource))]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(
            ErrorMessage = null,
            ErrorMessageResourceName = "Email_invalid",
            ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "Email", ResourceType = typeof(Resource))]
        public string Email { get; set; }
    }
}
