namespace OJS.Web.ViewModels.Account
{
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    using OJS.Common;

    using Resource = Resources.Account.AccountViewModels;

    public class LoginViewModel
    {
        [Required(
            ErrorMessageResourceName = "Username_required",
            ErrorMessageResourceType = typeof(Resource))]
        [Display(
            Name = "Username",
            ResourceType = typeof(Resource))]
        [StringLength(
            GlobalConstants.UsernameMaxLength,
            MinimumLength = GlobalConstants.UsernameMinLength,
            ErrorMessageResourceName = "Username_validation",
            ErrorMessageResourceType = typeof(Resource))]
        [RegularExpression(
            GlobalConstants.UsernameRegEx,
            ErrorMessageResourceName = "Username_regex_validation",
            ErrorMessageResourceType = typeof(Resource))]
        public string UserName { get; set; }

        [AllowHtml]
        [Required(
            ErrorMessageResourceName = "Enter_password",
            ErrorMessageResourceType = typeof(Resource))]
        [DataType(DataType.Password)]
        [Display(
            Name = "Password",
            ResourceType = typeof(Resource))]
        [StringLength(
            GlobalConstants.PasswordMaxLength,
            MinimumLength = GlobalConstants.PasswordMinLength,
            ErrorMessageResourceName = "Password_length_validation_message",
            ErrorMessageResourceType = typeof(Resource))]
        public string Password { get; set; }

        [Display(
            Name = "Remember_me",
            ResourceType = typeof(Resource))]
        public bool RememberMe { get; set; }
    }
}