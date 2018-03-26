namespace OJS.Web.Controllers
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using OJS.Common;

    public class LanguageController : Controller
    {
        private const int LanguageCookieExpirationYears = 5;

        public void ChangeLanguageCookie(string language)
        {
            var languageCookie = new HttpCookie(GlobalConstants.LanguageCookieName)
            {
                Value = language,
                Expires = DateTime.Now.AddYears(LanguageCookieExpirationYears)
            };

            this.Response.SetCookie(languageCookie);

            this.Response.Redirect(this.Request?.UrlReferrer?.AbsoluteUri ?? "/");
        }
    }
}