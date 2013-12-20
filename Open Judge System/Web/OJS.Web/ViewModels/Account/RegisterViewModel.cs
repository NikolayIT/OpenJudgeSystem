namespace OJS.Web.ViewModels.Account
{
    using System.ComponentModel.DataAnnotations;

    using Resource = Resources.Account.AccountViewModels;

    public class RegisterViewModel
    {
        [StringLength(15, ErrorMessageResourceName = "Username_validation",
            ErrorMessageResourceType = typeof(Resource), MinimumLength = 5)]
        [Required(ErrorMessageResourceName = "Username_required",
            ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "Username", ResourceType = typeof(Resource))]
        [RegularExpression(@"^[a-zA-Z]([/._]?[a-zA-Z0-9]+)+$", ErrorMessageResourceName = "Username_regex_validation",
            ErrorMessageResourceType = typeof(Resource))]
        public string UserName { get; set; }

        [Required(ErrorMessageResourceName = "Enter_password",
            ErrorMessageResourceType = typeof(Resource))]
        [StringLength(100, ErrorMessageResourceName = "Password_length_validation_message",
            ErrorMessageResourceType = typeof(Resource), MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(Resource))]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Password_confirm", ResourceType = typeof(Resource))]
        [Compare("Password", ErrorMessageResourceName = "Passwords_dont_match",
            ErrorMessageResourceType = typeof(Resource))]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessageResourceName = "Email_required",
            ErrorMessageResourceType = typeof(Resource))]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = null, ErrorMessageResourceName = "Email_invalid",
            ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "Email", ResourceType = typeof(Resource))]
        public string Email { get; set; }
    }
}
