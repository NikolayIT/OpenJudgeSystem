namespace OJS.Web.Areas.Users.Controllers
{
    using System.Web.Mvc;
    using OJS.Data;
    using OJS.Web.Controllers;

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

            return this.View(profile);
        }
    }
}