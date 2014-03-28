namespace OJS.Web.ViewModels.Account
{
    using System.ComponentModel.DataAnnotations;

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
        [StringLength(GlobalConstants.UsernameMaxLength,
            MinimumLength = GlobalConstants.UsernameMinLength,
            ErrorMessage = "Потребителското име трябва да бъде между {2} и {1} символа.")]
        [RegularExpression(GlobalConstants.UsernameRegEx, ErrorMessage = "Невалиден формат на потребителското име.")]
        public string UserName { get; set; }

        [Required(
                ErrorMessageResourceName = "Enter_password",
                ErrorMessageResourceType = typeof(Resource))]
        [DataType(DataType.Password)]
        [Display(
                Name = "Password",
                ResourceType = typeof(Resource))]
        [StringLength(GlobalConstants.PasswordMaxLength,
            MinimumLength = GlobalConstants.PasswordMinLength,
            ErrorMessage = "{0}та трябва да бъде между {2} и {1} символа.")]
        public string Password { get; set; }

        [Display(
                Name = "Remember_me",
                ResourceType = typeof(Resource))]
        public bool RememberMe { get; set; }
    }
}