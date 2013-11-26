namespace OJS.Web.ViewModels.Account
{
    using System.ComponentModel.DataAnnotations;

    public class ExternalLoginConfirmationViewModel
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
        public string UserName { get; set; }
        
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}
