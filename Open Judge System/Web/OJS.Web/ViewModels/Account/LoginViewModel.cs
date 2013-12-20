namespace OJS.Web.ViewModels.Account
{
    using System.ComponentModel.DataAnnotations;

    using Resource = Resources.Account.AccountViewModels;

    public class LoginViewModel
    {
        [Required(
                ErrorMessageResourceName = "Username_required",
                ErrorMessageResourceType = typeof(Resource))]
        [Display(
                Name = "Username",
                ResourceType = typeof(Resource))]
        public string UserName { get; set; }

        [Required(
                ErrorMessageResourceName = "Enter_password",
                ErrorMessageResourceType = typeof(Resource))]
        [DataType(DataType.Password)]
        [Display(
                Name = "Password",
                ResourceType = typeof(Resource))]
        public string Password { get; set; }

        [Display(
                Name = "Remember_me",
                ResourceType = typeof(Resource))]
        public bool RememberMe { get; set; }
    }
}