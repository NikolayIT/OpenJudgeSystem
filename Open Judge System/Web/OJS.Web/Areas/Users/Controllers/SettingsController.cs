namespace OJS.Web.Areas.Users.Controllers
{
    using System;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Controllers;

    [Authorize]
    public class SettingsController : BaseController
    {
        public SettingsController(IOjsData data)
            : base(data)
        {

        }

        [HttpGet]
        public ActionResult Index()
        {
            string currentUserName = this.User.Identity.Name;

            var profile = this.Data.Users.GetByUsername(currentUserName);

            return View(profile.UserSettings);
        }

        [HttpPost]
        public ActionResult Index(UserSettings settings)
        {
            if (ModelState.IsValid)
            {
                var user = this.Data.Users.GetByUsername(User.Identity.Name);
                user.UserSettings = settings;
                this.Data.SaveChanges();

                TempData.Add("InfoMessage", "Настройките на профила ви са запазени!");
                return Redirect("/Users/Profile");
            }

            return View(settings);
        }
    }
}