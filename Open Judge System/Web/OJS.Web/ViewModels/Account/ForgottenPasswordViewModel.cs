namespace OJS.Web.ViewModels.Account
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    using Resource = Resources.Account.AccountViewModels;

    public class ForgottenPasswordViewModel
    {
        [AllowHtml]
        [Required(
            ErrorMessageResourceName = "Enter_new_password_validation",
            ErrorMessageResourceType = typeof(Resource))]
        [Display(
            Name = "Password",
            ResourceType = typeof(Resource))]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [AllowHtml]
        [Required(
            ErrorMessageResourceName = "Enter_new_password_confirmation",
            ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "Password_confirm", ResourceType = typeof(Resource))]
        [System.ComponentModel.DataAnnotations.Compare(
            "Password",
            ErrorMessageResourceName = "New_password_confirm_password_not_matching_validation",
            ErrorMessageResourceType = typeof(Resource))]
        [DataType(DataType.Password)]
        public string PasswordConfirmation { get; set; }

        public Guid Token { get; set; }
    }
}
