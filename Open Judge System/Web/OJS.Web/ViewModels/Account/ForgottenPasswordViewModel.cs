namespace OJS.Web.ViewModels.Account
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class ForgottenPasswordViewModel
    {
        [Required(
                ErrorMessageResourceName = "Enter_new_password_validation",
                ErrorMessageResourceType = typeof(Resources.Account.ViewModels))]
        [Display(
                Name = "Password",
                ResourceType = typeof(Resources.Account.ViewModels))]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(
                ErrorMessageResourceName = "Enter_new_password_confirmation",
                ErrorMessageResourceType = typeof(Resources.Account.ViewModels))]
        [Display(
                Name = "Password_confirm",
                ResourceType = typeof(Resources.Account.ViewModels))]
        [Compare(
                "Password",
                ErrorMessageResourceName = "New_password_confirm_password_not_matching_validation",
                ErrorMessageResourceType = typeof(Resources.Account.ViewModels))]
        [DataType(DataType.Password)]
        public string PasswordConfirmation { get; set; }

        public Guid Token { get; set; }
    }
}