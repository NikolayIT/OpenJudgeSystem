namespace OJS.Web.Areas.Users.Controllers
{
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Data;
    using OJS.Web.Controllers;
    using OJS.Web.Areas.Users.ViewModels;

    public class ProfileController : BaseController
    {
        public ProfileController(IOjsData data)
            : base(data)
        {
        }

        public ActionResult Index(string id)
        {
            if (id == null)
            {
                id = this.User.Identity.Name;
            }

            var profile = this.Data.Users.GetByUsername(id);

            var userSettingsViewModel = new UserProfileViewModel(profile);

            return this.View(userSettingsViewModel);
        }
    }
}