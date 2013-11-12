namespace OJS.Web.ViewModels.Account
{
    using System.ComponentModel.DataAnnotations;

    public class LoginViewModel
    {
        [Required(
                ErrorMessageResourceName = "Username_required",
                ErrorMessageResourceType = typeof(Resources.Account.ViewModels))]
        [Display(
                Name = "Username",
                ResourceType = typeof(Resources.Account.ViewModels))]
        public string UserName { get; set; }

        [Required(
                ErrorMessageResourceName = "Enter_password",
                ErrorMessageResourceType = typeof(Resources.Account.ViewModels))]
        [DataType(DataType.Password)]
        [Display(
                Name = "Password",
                ResourceType = typeof(Resources.Account.ViewModels))]
        public string Password { get; set; }

        [Display(
                Name = "Remember_me",
                ResourceType = typeof(Resources.Account.ViewModels))]
        public bool RememberMe { get; set; }
    }
}