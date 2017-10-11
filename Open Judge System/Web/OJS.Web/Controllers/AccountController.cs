namespace OJS.Web.Controllers
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.Owin.Security;

    using OJS.Common;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Common;
    using OJS.Web.ViewModels.Account;

    [Authorize]
    public class AccountController : BaseController
    {
        public AccountController(IOjsData data)
            : this(data, new OjsUserManager<UserProfile>(new UserStore<UserProfile>(data.Context.DbContext)))
        {
        }

        public AccountController(IOjsData data, UserManager<UserProfile> userManager)
            : base(data)
        {
            this.UserManager = userManager;
        }

        public UserManager<UserProfile> UserManager { get; private set; }

        private IAuthenticationManager AuthenticationManager => this.HttpContext.GetOwinContext().Authentication;

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            this.ViewBag.ReturnUrl = returnUrl;
            return this.View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (this.ModelState.IsValid)
            {
                ExternalUserViewModel externalUser;
                try
                {
                    externalUser = await this.GetExternalUser(model.UserName);
                }
                catch (Exception)
                {
                    this.TempData[GlobalConstants.InfoMessage] = Resources.Account.AccountControllers.Inactive_login_system;
                    return this.RedirectToAction("Index", "Home", new { area = string.Empty });
                }

                if (externalUser != null)
                {
                    var userEntity = externalUser.Entity;
                    this.AddOrUpdateUser(userEntity);

                    var user = await this.UserManager.FindAsync(model.UserName, model.Password);
                    if (user != null)
                    {
                        await this.SignInAsync(userEntity, model.RememberMe);
                        return this.RedirectToLocal(returnUrl);
                    }
                }

                this.ModelState.AddModelError(string.Empty, Resources.Account.AccountViewModels.Invalid_username_or_password);
            }

            // If we got this far, something failed, redisplay form
            return this.View(model);
        }

        [AllowAnonymous]
        public ActionResult Register() => this.RedirectToExternalSystemMessage();

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Register(RegisterViewModel model, bool captchaValid) => this.RedirectToExternalSystemMessage();

        [HttpPost]
        public ActionResult Disassociate(string loginProvider, string providerKey) => this.RedirectToExternalSystemMessage();

        public ActionResult Manage() => this.RedirectToExternalSystemMessage();

        /// <summary>
        /// Informs the user that the registration proccess is 
        /// disabled on this site and he must register from exturnal source
        /// </summary>
        [AllowAnonymous]
        public ActionResult ExternalNotify() => this.View();

        [HttpPost]
        public ActionResult Manage(ManageUserViewModel model) => this.RedirectToExternalSystemMessage();

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ExternalLogin(string provider, string returnUrl) => this.RedirectToExternalSystemMessage();

        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl) => this.RedirectToExternalSystemMessage();

        [HttpPost]
        public ActionResult LinkLogin(string provider) => this.RedirectToExternalSystemMessage();

        public ActionResult LinkLoginCallback() => this.RedirectToExternalSystemMessage();

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ExternalLoginConfirmation(
            ExternalLoginConfirmationViewModel model,
            string returnUrl) => this.RedirectToExternalSystemMessage();

        [HttpPost]
        public ActionResult LogOff()
        {
            this.AuthenticationManager.SignOut();
            return this.RedirectToAction(GlobalConstants.Index, "Home");
        }

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure() => this.View();

        [ChildActionOnly]
        public ActionResult RemoveAccountList() => this.RedirectToExternalSystemMessage();

        [AllowAnonymous]
        public ActionResult ForgottenPassword() => this.RedirectToExternalSystemMessage();

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ForgottenPassword(string emailOrUsername) => this.RedirectToExternalSystemMessage();

        [AllowAnonymous]
        public ActionResult ChangePassword(string token) => this.RedirectToExternalSystemMessage();

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ChangePassword(ForgottenPasswordViewModel model) => this.RedirectToExternalSystemMessage();

        public ActionResult ChangeEmail() => this.RedirectToExternalSystemMessage();

        [HttpPost]
        public ActionResult ChangeEmail(ChangeEmailViewModel model) => this.RedirectToExternalSystemMessage();

        [Authorize]
        public ActionResult ChangeUsername() => this.RedirectToExternalSystemMessage();

        [HttpPost]
        public ActionResult ChangeUsername(ChangeUsernameViewModel model) => this.RedirectToExternalSystemMessage();

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.UserManager != null)
            {
                this.UserManager.Dispose();
                this.UserManager = null;
            }

            base.Dispose(disposing);
        }

        private ActionResult RedirectToExternalSystemMessage() => this.RedirectToAction(
            nameof(this.ExternalNotify),
            "Account",
            new { area = string.Empty });

        private void AddOrUpdateUser(UserProfile user)
        {
            var existingUser = this.Data.Users.GetById(user.Id);
            if (existingUser == null)
            {
                this.Data.Users.Add(user);
            }
            else
            {
                existingUser.PasswordHash = user.PasswordHash;
                existingUser.SecurityStamp = user.SecurityStamp;
                existingUser.Email = user.Email;
                existingUser.ForgottenPasswordToken = user.ForgottenPasswordToken;
                existingUser.IsDeleted = user.IsDeleted;
                existingUser.DeletedOn = user.DeletedOn;
                existingUser.ModifiedOn = user.ModifiedOn;
                existingUser.UserSettings = user.UserSettings;
            }

            this.Data.SaveChanges();
        }

        private async Task<ExternalUserViewModel> GetExternalUser(string userName)
        {
            using (var httpClient = new HttpClient())
            {
                var jsonMediaType = new MediaTypeWithQualityHeaderValue(GlobalConstants.JsonMimeType);
                httpClient.DefaultRequestHeaders.Accept.Add(jsonMediaType);

                var response = await httpClient.PostAsJsonAsync(Settings.GetExternalUserUrl, new { userName });
                if (response.IsSuccessStatusCode)
                {
                    var externalUser = await response.Content.ReadAsAsync<ExternalUserViewModel>();
                    return externalUser;
                }

                throw new HttpException((int)response.StatusCode, "An error has occurred while connecting to the external system.");
            }
        }

        private async Task SignInAsync(UserProfile user, bool isPersistent)
        {
            this.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await this.UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            this.AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, identity);
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (this.Url.IsLocalUrl(returnUrl))
            {
                return this.Redirect(returnUrl);
            }

            return this.RedirectToAction(GlobalConstants.Index, "Home");
        }
    }
}