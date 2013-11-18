namespace OJS.Web.ViewModels.Account
{
    using System.ComponentModel.DataAnnotations;

    using CompareMvc = System.Web.Mvc.CompareAttribute;

    public class ManageUserViewModel
    {
        [Required(
                ErrorMessageResourceName = "Enter_password_validation",
                ErrorMessageResourceType = typeof(Resources.Account.ViewModels))]
        [DataType(DataType.Password)]
        [Display(
                Name = "Current_password",
                ResourceType = typeof(Resources.Account.ViewModels))]
        public string OldPassword { get; set; }

        [Required(
                ErrorMessageResourceName = "Enter_new_password_validation",
                ErrorMessageResourceType = typeof(Resources.Account.ViewModels))]
        [StringLength(100,
                ErrorMessageResourceName = "Password_length_validation_message",
                ErrorMessageResourceType = typeof(Resources.Account.ViewModels),
                MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(
                Name = "New_password",
                ResourceType = typeof(Resources.Account.ViewModels))]
        public string NewPassword { get; set; }

        [Required(
                ErrorMessageResourceName = "Enter_new_password_confirmation",
                ErrorMessageResourceType = typeof(Resources.Account.ViewModels))]
        [DataType(DataType.Password)]
        [Display(
                Name = "New_password_confirmation",
                ResourceType = typeof(Resources.Account.ViewModels))]
        [CompareMvc(
                "NewPassword",
                ErrorMessageResourceName = "New_password_confirm_password_not_matching_validation",
                ErrorMessageResourceType = typeof(Resources.Account.ViewModels))]
        public string ConfirmPassword { get; set; }
    }
}