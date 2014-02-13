namespace OJS.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Security.Claims;
    using System.Text.RegularExpressions;
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

    using Recaptcha;

    [Authorize]
    public class AccountController : BaseController
    {
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

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

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return this.HttpContext.GetOwinContext().Authentication;
            }
        }

        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            this.ViewBag.ReturnUrl = returnUrl;
            return this.View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (this.ModelState.IsValid)
            {
                var user = await this.UserManager.FindAsync(model.UserName, model.Password);
                if (user != null)
                {
                    await this.SignInAsync(user, model.RememberMe);
                    return this.RedirectToLocal(returnUrl);
                }

                this.ModelState.AddModelError(string.Empty, Resources.Account.AccountViewModels.Invalid_username_or_password);
            }

            // If we got this far, something failed, redisplay form
            return this.View(model);
        }

        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            if (this.User.Identity.IsAuthenticated)
            {
                return this.RedirectToAction("Manage");
            }

            return this.View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [RecaptchaControlMvc.CaptchaValidator]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model, bool captchaValid)
        {
            if (this.Data.Users.All().Any(x => x.Email == model.Email))
            {
                this.ModelState.AddModelError("Email", Resources.Account.AccountViewModels.Email_already_registered);
            }

            if (this.Data.Users.All().Any(x => x.UserName == model.UserName))
            {
                this.ModelState.AddModelError("UserName", Resources.Account.AccountViewModels.User_already_registered);
            }

            if (!captchaValid)
            {
                this.ModelState.AddModelError("Captcha", Resources.Account.Views.General.Captcha_invalid);
            }

            if (this.ModelState.IsValid)
            {
                var user = new UserProfile { UserName = model.UserName, Email = model.Email };
                var result = await this.UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await this.SignInAsync(user, isPersistent: false);
                    return this.RedirectToAction(GlobalConstants.Index, "Home");
                }

                this.AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return this.View(model);
        }

        // POST: /Account/Disassociate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
        {
            IdentityResult result =
                await
                this.UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                this.TempData[GlobalConstants.InfoMessage] = Resources.Account.Views.Disassociate.External_login_removed;
            }
            else
            {
                this.TempData[GlobalConstants.DangerMessage] = Resources.Account.Views.Disassociate.Error;
            }

            return this.RedirectToAction("Manage");
        }

        // GET: /Account/Manage
        public ActionResult Manage()
        {
            this.ViewBag.HasLocalPassword = this.HasPassword();
            this.ViewBag.ReturnUrl = Url.Action("Manage");
            return this.View();
        }

        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            bool hasPassword = this.HasPassword();
            this.ViewBag.HasLocalPassword = hasPassword;
            this.ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasPassword)
            {
                if (this.ModelState.IsValid)
                {
                    IdentityResult result =
                        await
                        this.UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        this.TempData[GlobalConstants.InfoMessage] = Resources.Account.Views.Manage.Password_updated;
                        return this.RedirectToAction(GlobalConstants.Index, new { controller = "Settings", area = "Users" });
                    }

                    this.ModelState.AddModelError(string.Empty, Resources.Account.AccountViewModels.Password_incorrect);
                }
            }
            else
            {
                // User does not have a password so remove any validation errors caused by a missing OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (this.ModelState.IsValid)
                {
                    IdentityResult result =
                        await this.UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                    if (result.Succeeded)
                    {
                        this.TempData[GlobalConstants.InfoMessage] = Resources.Account.Views.Manage.Password_updated;
                        return this.RedirectToAction(GlobalConstants.Index, new { controller = "Settings", area = "Users" });
                    }

                    this.AddErrors(result);
                }
            }

            // If we got this far, something failed, redisplay form
            return this.View(model);
        }

        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(
                provider,
                Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await this.AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return this.RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var user = await this.UserManager.FindAsync(loginInfo.Login);
            if (user != null)
            {
                await this.SignInAsync(user, isPersistent: false);
                return this.RedirectToLocal(returnUrl);
            }

            // If a user account was not found - check if he has already registered his email.
            ClaimsIdentity claimsIdentity = this.AuthenticationManager.GetExternalIdentityAsync(DefaultAuthenticationTypes.ExternalCookie).Result;
            var email = claimsIdentity.FindFirstValue(ClaimTypes.Email);

            if (this.Data.Users.All().Any(x => x.Email == email))
            {
                this.TempData[GlobalConstants.DangerMessage] = Resources.Account.Views.ExternalLoginCallback.Email_already_registered;
                return this.RedirectToAction("Login");
            }

            // If the user does not have an account, then prompt the user to create an account
            this.ViewBag.ReturnUrl = returnUrl;
            this.ViewBag.LoginProvider = loginInfo.Login.LoginProvider;

            return this.View(
                "ExternalLoginConfirmation",
                new ExternalLoginConfirmationViewModel { UserName = loginInfo.DefaultUserName, Email = email });
        }

        // POST: /Account/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new ChallengeResult(provider, Url.Action("LinkLoginCallback", "Account"), User.Identity.GetUserId());
        }

        // GET: /Account/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await this.AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo != null)
            {
                var result = await this.UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
                if (result.Succeeded)
                {
                    return this.RedirectToAction("Manage");
                }
            }

            this.TempData[GlobalConstants.DangerMessage] = Resources.Account.Views.ExternalLoginConfirmation.Error;
            return this.RedirectToAction("Manage");
        }

        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(
            ExternalLoginConfirmationViewModel model,
            string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return this.RedirectToAction("Manage");
            }

            if (this.ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await this.AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return this.View("ExternalLoginFailure");
                }

                if (this.Data.Users.All().Any(x => x.Email == model.Email))
                {
                    this.TempData[GlobalConstants.DangerMessage] = Resources.Account.Views.ExternalLoginConfirmation.Email_already_registered;
                    return this.RedirectToAction("ForgottenPassword");
                }

                if (this.Data.Users.All().Any(x => x.UserName == model.UserName))
                {
                    this.ModelState.AddModelError("Username", Resources.Account.Views.ExternalLoginConfirmation.User_already_registered);
                }

                if (!this.ModelState.IsValid)
                {
                    return this.View(model);
                }

                var user = new UserProfile { UserName = model.UserName, Email = model.Email };
                var result = await this.UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await this.UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await this.SignInAsync(user, isPersistent: false);
                        return this.RedirectToLocal(returnUrl);
                    }
                }

                this.AddErrors(result);
            }

            this.ViewBag.ReturnUrl = returnUrl;
            return this.View(model);
        }

        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            this.AuthenticationManager.SignOut();
            return this.RedirectToAction(GlobalConstants.Index, "Home");
        }

        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return this.View();
        }

        [ChildActionOnly]
        public ActionResult RemoveAccountList()
        {
            var linkedAccounts = this.UserManager.GetLogins(User.Identity.GetUserId());
            this.ViewBag.ShowRemoveButton = this.HasPassword() || linkedAccounts.Count > 1;
            return this.PartialView("_RemoveAccountPartial", linkedAccounts);
        }

        [AllowAnonymous]
        public ActionResult ForgottenPassword()
        {
            return this.View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ForgottenPassword(string emailOrUsername)
        {
            if (string.IsNullOrEmpty(emailOrUsername))
            {
                this.ModelState.AddModelError("emailOrUsername", Resources.Account.Views.ForgottenPassword.Email_or_username_required);
                return this.View();
            }

            var userByUsername = this.Data.Users.GetByUsername(emailOrUsername);

            if (userByUsername != null)
            {
                userByUsername.ForgottenPasswordToken = Guid.NewGuid();
                this.Data.SaveChanges();
                this.SendForgottenPasswordToUser(userByUsername);
                this.TempData[GlobalConstants.InfoMessage] = Resources.Account.Views.ForgottenPassword.Email_sent;
                return this.RedirectToAction("ForgottenPassword");
            }

            // using Where() because duplicate email addresses were allowed in the previous
            // judge system
            var usersByEmail = this.Data.Users
                                    .All()
                                    .Where(x => x.Email == emailOrUsername).ToList();

            var usersCount = usersByEmail.Count();

            // notify the user if there are no users registered with this email or username
            if (usersCount == 0)
            {
                this.ModelState.AddModelError("emailOrUsername", Resources.Account.Views.ForgottenPassword.Email_or_username_not_registered);
                return this.View();
            }

            // if there are users registered with this email - send a forgotten password email
            // to each one of them
            foreach (var user in usersByEmail)
            {
                user.ForgottenPasswordToken = Guid.NewGuid();
                this.Data.SaveChanges();
                this.SendForgottenPasswordToUser(user);
            }

            this.TempData[GlobalConstants.InfoMessage] = Resources.Account.Views.ForgottenPassword.Email_sent;
            return this.RedirectToAction("ForgottenPassword");
        }

        [AllowAnonymous]
        public ActionResult ChangePassword(string token)
        {
            Guid guid;

            if (!Guid.TryParse(token, out guid))
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, "Invalid token!");
            }

            var user = this.Data.Users.All().FirstOrDefault(x => x.ForgottenPasswordToken == guid);

            if (user == null)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, "Invalid token!");
            }

            var forgottenPasswordModel = new ForgottenPasswordViewModel
            {
                Token = guid
            };

            return this.View(forgottenPasswordModel);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ChangePassword(ForgottenPasswordViewModel model)
        {
            var user = this.Data.Users.All()
                .FirstOrDefault(x => x.ForgottenPasswordToken == model.Token);

            if (user == null)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, "Invalid token!");
            }

            if (this.ModelState.IsValid)
            {
                IdentityResult removePassword =
                                        await
                                        this.UserManager.RemovePasswordAsync(user.Id);
                if (removePassword.Succeeded)
                {
                    IdentityResult changePassword =
                                        await
                                        this.UserManager.AddPasswordAsync(user.Id, model.Password);

                    if (changePassword.Succeeded)
                    {
                        user.ForgottenPasswordToken = null;
                        this.Data.SaveChanges();

                        this.TempData[GlobalConstants.InfoMessage] = Resources.Account.Views.ChangePasswordView.Password_updated;
                        return this.RedirectToAction("Login");
                    }

                    this.AddErrors(changePassword);
                }

                this.AddErrors(removePassword);
            }

            return this.View(model);
        }

        public ActionResult ChangeEmail()
        {
            return this.View();
        }

        [HttpPost]
        public ActionResult ChangeEmail(ChangeEmailViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                if (this.Data.Users.All().Any(x => x.Email == model.Email))
                {
                    this.ModelState.AddModelError("Email", Resources.Account.AccountViewModels.Email_already_registered);
                }

                var passwordVerificationResult = this.UserManager.PasswordHasher.VerifyHashedPassword(this.UserProfile.PasswordHash, model.Password);

                if (passwordVerificationResult != PasswordVerificationResult.Success)
                {
                    this.ModelState.AddModelError("Password", Resources.Account.AccountViewModels.Incorrect_password);
                }

                if (this.ModelState.IsValid)
                {
                    var currentUser = this.Data.Users.GetById(this.UserProfile.Id);

                    currentUser.Email = model.Email;
                    this.Data.SaveChanges();
                    this.TempData[GlobalConstants.InfoMessage] = "Success";
                    return this.RedirectToAction("Profile", new { controller = "Users", area = string.Empty });
                }
            }

            return this.View(model);
        }

        [Authorize]
        [HttpGet]
        public ActionResult ChangeUsername()
        {
            if (Regex.IsMatch(this.UserProfile.UserName, "^[a-zA-Z]([/._]?[a-zA-Z0-9]+)+$") && this.UserProfile.UserName.Length >= 5 && this.UserProfile.UserName.Length <= 15)
            {
                return this.RedirectToAction(GlobalConstants.Index, new { controller = "Profile", area = "Users" });
            }

            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeUsername(ChangeUsernameViewModel model)
        {
            if (Regex.IsMatch(this.UserProfile.UserName, "^[a-zA-Z]([/._]?[a-zA-Z0-9]+)+$") && this.UserProfile.UserName.Length >= 5 && this.UserProfile.UserName.Length <= 15)
            {
                return this.RedirectToAction(GlobalConstants.Index, new { controller = "Profile", area = "Users" });
            }

            if (this.ModelState.IsValid)
            {
                if (this.Data.Users.All().Any(x => x.UserName == model.Username))
                {
                    this.ModelState.AddModelError("Username", "This username is not available");
                    return this.View(model);
                }

                this.UserProfile.UserName = model.Username;
                this.Data.SaveChanges();

                this.TempData[GlobalConstants.InfoMessage] = Resources.Account.Views.ChangeUsernameView.Username_changed;
                this.AuthenticationManager.SignOut();
                return this.RedirectToAction("Login", new { controller = "Account", area = string.Empty });
            }

            return this.View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.UserManager != null)
            {
                this.UserManager.Dispose();
                this.UserManager = null;
            }

            base.Dispose(disposing);
        }

        private void SendForgottenPasswordToUser(UserProfile user)
        {
            var mailSender = MailSender.Instance;

            var forgottenPasswordEmailTitle = string.Format(
                                                        Resources.Account.AccountEmails.Forgotten_password_title,
                                                        user.UserName);

            var forgottenPasswordEmailBody = string.Format(
                                                Resources.Account.AccountEmails.Forgotten_password_body,
                                                user.UserName,
                                                Url.Action(
                                                        "ChangePassword",
                                                        "Account",
                                                        new { token = user.ForgottenPasswordToken },
                                                        Request.Url.Scheme));

            mailSender.SendMail(user.Email, forgottenPasswordEmailTitle, forgottenPasswordEmailBody);
        }

        #region Helpers
        private async Task SignInAsync(UserProfile user, bool isPersistent)
        {
            this.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await this.UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            this.AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, identity);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                this.ModelState.AddModelError(string.Empty, error);
            }
        }

        private bool HasPassword()
        {
            var user = this.UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }

            return false;
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return this.Redirect(returnUrl);
            }

            return this.RedirectToAction(GlobalConstants.Index, "Home");
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri, string userId = null)
            {
                this.LoginProvider = provider;
                this.RedirectUri = redirectUri;
                this.UserId = userId;
            }

            private string LoginProvider { get; set; }

            private string RedirectUri { get; set; }

            private string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = this.RedirectUri };
                if (this.UserId != null)
                {
                    properties.Dictionary[AccountController.XsrfKey] = this.UserId;
                }

                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, this.LoginProvider);
            }
        }
        #endregion
    }
}
