namespace OJS.Web.ViewModels.Account
{
    using System.ComponentModel.DataAnnotations;

    using Resource = Resources.Account.AccountViewModels;

    public class ExternalLoginConfirmationViewModel
    {
        [StringLength(
                15,
                ErrorMessageResourceName = "Username_validation",
                ErrorMessageResourceType = typeof(Resource),
                MinimumLength = 5)]
        [Required(
                ErrorMessageResourceName = "Username_required",
                ErrorMessageResourceType = typeof(Resource))]
        [Display(
                Name = "Username",
                ResourceType = typeof(Resource))]
        [RegularExpression(
                @"^[a-zA-Z]([/._]?[a-zA-Z0-9]+)+$",
                ErrorMessageResourceName = "Username_regex_validation",
                ErrorMessageResourceType = typeof(Resource))]
        public string UserName { get; set; }
        
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}
