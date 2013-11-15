namespace OJS.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.Owin.Security;

    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Common;
    using OJS.Web.ViewModels.Account;

    using Recaptcha;

    using Resources;

    [Authorize]
    public class AccountController : BaseController
    {
        public AccountController(IOjsData data)
            : this(data, new UserManager<UserProfile>(new UserStore<UserProfile>(data.Context.DbContext)))
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
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return this.View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await this.UserManager.FindAsync(model.UserName, model.Password);
                if (user != null)
                {
                    await this.SignInAsync(user, model.RememberMe);
                    return this.RedirectToLocal(returnUrl);
                }

                this.ModelState.AddModelError(string.Empty, Resources.Account.ViewModels.Invalid_username_or_password);
            }

            // If we got this far, something failed, redisplay form
            return this.View(model);
        }

        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
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
                ModelState.AddModelError("Email", Resources.Account.ViewModels.Email_already_registered);
            }

            if (this.Data.Users.All().Any(x=>x.UserName == model.UserName))
            {
                ModelState.AddModelError("UserName", Resources.Account.ViewModels.User_already_registered);
            }

            if (!captchaValid)
            {
                ModelState.AddModelError("Captcha", Resources.Account.Views.General.Captcha_invalid);
            }

            if (ModelState.IsValid)
            {
                var user = new UserProfile { UserName = model.UserName, Email = model.Email };
                var result = await this.UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await this.SignInAsync(user, isPersistent: false);
                    return this.RedirectToAction("Index", "Home");
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
            ManageMessageId? message;
            IdentityResult result =
                await
                this.UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }

            return this.RedirectToAction("Manage", new { Message = message });
        }

        // GET: /Account/Manage
        public ActionResult Manage(ManageMessageId? message)
        {
            // TODO: remove parameter - use TempData instead
            ViewBag.StatusMessage = message == ManageMessageId.ChangePasswordSuccess
                                        ? "Паролата ви беше сменена."
                                        : message == ManageMessageId.SetPasswordSuccess
                                              ? "Паролата беше запазена."
                                              : message == ManageMessageId.RemoveLoginSuccess
                                                    ? "Външния логин беше премахнат."
                                                    : message == ManageMessageId.Error ? "Възникна грешка." : string.Empty;

            ViewBag.HasLocalPassword = this.HasPassword();
            ViewBag.ReturnUrl = Url.Action("Manage");
            return this.View();
        }

        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            bool hasPassword = this.HasPassword();
            ViewBag.HasLocalPassword = hasPassword;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasPassword)
            {
                if (ModelState.IsValid)
                {
                    IdentityResult result =
                        await
                        this.UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return this.RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }

                    this.ModelState.AddModelError(string.Empty, Resources.Account.ViewModels.Password_incorrect);
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

                if (ModelState.IsValid)
                {
                    IdentityResult result =
                        await this.UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                    if (result.Succeeded)
                    {
                        return this.RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
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

            // If the user does not have an account, then prompt the user to create an account
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
            return this.View(
                "ExternalLoginConfirmation",
                new ExternalLoginConfirmationViewModel { UserName = loginInfo.DefaultUserName });
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
            if (loginInfo == null)
            {
                return this.RedirectToAction("Manage", new { Message = ManageMessageId.Error });
            }

            var result = await this.UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            if (result.Succeeded)
            {
                return this.RedirectToAction("Manage");
            }

            return this.RedirectToAction("Manage", new { Message = ManageMessageId.Error });
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

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await this.AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return this.View("ExternalLoginFailure");
                }

                var user = new UserProfile { UserName = model.UserName };
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

            ViewBag.ReturnUrl = returnUrl;
            return this.View(model);
        }

        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            this.AuthenticationManager.SignOut();
            return this.RedirectToAction("Index", "Home");
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
            ViewBag.ShowRemoveButton = this.HasPassword() || linkedAccounts.Count > 1;
            return this.PartialView("_RemoveAccountPartial", linkedAccounts);
        }

        [AllowAnonymous]
        public ActionResult ForgottenPassword()
        {
            return this.View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ForgottenPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                this.ModelState.AddModelError("Email", Resources.Account.Views.ForgottenPassword.Email_required);
            }

            // using Where() because duplicate email addresses were allowed in the previous
            // judge system
            var usersWithEmail = this.Data.Users
                                            .All()
                                            .Where(x => x.Email == email);

            var usersCount = usersWithEmail.Count();

            if (usersCount == 0)
            {
                ViewBag.StatusMessage = Resources.Account.Views.ForgottenPassword.Email_not_registered;
                return this.View();
            }

            if (usersCount != 1)
            {
                ViewBag.StatusMessage = Resources.Account.Views.ForgottenPassword.Email_registered_more_than_once;
                return this.View();
            }

            var user = usersWithEmail.FirstOrDefault();

            user.ForgottenPasswordToken = Guid.NewGuid();
            this.Data.SaveChanges();

            // TODO: create and localize the message for the forgot your password email.
            var mailSender = MailSender.Instance;
            mailSender.SendMail(user.Email, "Forgot your password", Url.Action("ChangePassword", new { token = user.ForgottenPasswordToken }));

            // TODO: Redirect to the correct page
            return this.RedirectToAction("Index", new { Model = string.Empty });
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

            if (ModelState.IsValid)
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

                        return this.RedirectToAction("Login", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }

                    this.AddErrors(changePassword);
                }

                this.AddErrors(removePassword);
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

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

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
                ModelState.AddModelError(string.Empty, error);
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

        public enum ManageMessageId
        {
            ChangePasswordSuccess,

            SetPasswordSuccess,

            RemoveLoginSuccess,

            Error
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return this.Redirect(returnUrl);
            }

            return this.RedirectToAction("Index", "Home");
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
                    properties.Dictionary[XsrfKey] = this.UserId;
                }

                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, this.LoginProvider);
            }
        }
        #endregion
    }
}
