namespace OJS.Web.Areas.Users.Controllers
{
    using System.Web.Mvc;

    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Areas.Users.ViewModels;
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
            var userProfileViewModel = new UserSettingsViewModel(profile);

            return this.View(userProfileViewModel);
        }

        [HttpPost]
        public ActionResult Index(UserSettingsViewModel settings)
        {
            if (ModelState.IsValid)
            {
                var user = this.Data.Users.GetByUsername(User.Identity.Name);
                this.UpdateUserSettings(user.UserSettings, settings);
                this.Data.SaveChanges();

                TempData.Add("InfoMessage", "Настройките на профила ви са запазени!");
                return this.Redirect("/Users/Profile");
            }

            return this.View(settings);
        }

        private void UpdateUserSettings(UserSettings model, UserSettingsViewModel viewModel)
        {
            model.FirstName = viewModel.FirstName;
            model.LastName = viewModel.LastName;
            model.City = viewModel.City;
            model.DateOfBirth = viewModel.DateOfBirth;
            model.Company = viewModel.Company;
            model.JobTitle = viewModel.JobTitle;
            model.EducationalInstitution = viewModel.EducationalInstitution;
            model.FacultyNumber = viewModel.FacultyNumber;
        }
    }
}