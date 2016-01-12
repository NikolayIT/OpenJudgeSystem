namespace OJS.Web.ViewModels.Account
{
    using System.ComponentModel.DataAnnotations;

    using OJS.Common;

    using Resource = Resources.Account.AccountViewModels;

    public class ExternalLoginConfirmationViewModel
    {
        [StringLength(
                GlobalConstants.UsernameMaxLength,
                MinimumLength = GlobalConstants.UsernameMinLength,
                ErrorMessageResourceName = "Username_validation",
                ErrorMessageResourceType = typeof(Resource))]
        [Required(
                ErrorMessageResourceName = "Username_required",
                ErrorMessageResourceType = typeof(Resource))]
        [Display(
                Name = "Username",
                ResourceType = typeof(Resource))]
        [RegularExpression(
                GlobalConstants.UsernameRegEx,
                ErrorMessageResourceName = "Username_regex_validation",
                ErrorMessageResourceType = typeof(Resource))]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}
