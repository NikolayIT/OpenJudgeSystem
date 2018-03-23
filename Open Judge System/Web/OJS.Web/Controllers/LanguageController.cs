namespace OJS.Web.Controllers
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using OJS.Web.Common;

    public class LanguageController : Controller
    {
        public void ChangeCulture(string lang)
        {
            var languageCookie = new HttpCookie(WebConstants.LanguageCookieName)
            {
                Value = lang,
                Expires = DateTime.Now.AddDays(10)
            };

            this.Response.SetCookie(languageCookie);

            this.Response.Redirect(this.Request?.UrlReferrer?.AbsoluteUri ?? "/");
        }
    }
}