namespace OJS.Web.Areas.Users.Controllers
{
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

            return this.View(profile.UserSettings);
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
                return this.Redirect("/Users/Profile");
            }

            return this.View(settings);
        }
    }
}