namespace OJS.Web.ViewModels.Account
{
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    using OJS.Common;

    using Resource = Resources.Account.AccountViewModels;

    public class ChangeEmailViewModel
    {
        [AllowHtml]
        [StringLength(
            GlobalConstants.PasswordMaxLength,
            MinimumLength = GlobalConstants.PasswordMinLength,
            ErrorMessageResourceName = "Password_required",
            ErrorMessageResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "Password_required",
            ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "Password", ResourceType = typeof(Resource))]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(
            ErrorMessageResourceName = "Email_required",
            ErrorMessageResourceType = typeof(Resource))]
        [DataType(DataType.EmailAddress)]
        [Display(
            Name = "Email",
            ResourceType = typeof(Resource))]
        [EmailAddress(
            ErrorMessage = null,
            ErrorMessageResourceName = "Email_required",
            ErrorMessageResourceType = typeof(Resource))]
        public string Email { get; set; }

        [Required(
            ErrorMessageResourceName = "Email_confirmation_required",
            ErrorMessageResourceType = typeof(Resource))]
        [DataType(DataType.EmailAddress)]
        [Display(
            Name = "Email_confirm",
            ResourceType = typeof(Resource))]
        [EmailAddress(
            ErrorMessage = null,
            ErrorMessageResourceName = "Email_confirmation_required",
            ErrorMessageResourceType = typeof(Resource))]
        [System.ComponentModel.DataAnnotations.Compare(
            "Email",
            ErrorMessageResourceName = "Email_confirmation_invalid",
            ErrorMessageResourceType = typeof(Resource))]
        public string EmailConfirmation { get; set; }
    }
}
