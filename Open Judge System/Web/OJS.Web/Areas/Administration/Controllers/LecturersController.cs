namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Services.Data.Users;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Areas.Administration.ViewModels.Lecturers;

    public class LecturersController : AdministrationBaseGridController
    {
        private readonly IUsersDataService usersData;

        public LecturersController(
            IOjsData data,
            IUsersDataService usersData)
            : base(data) =>
                this.usersData = usersData;

        public override IEnumerable GetData()
        {
            var lecturerRoleName = SystemRole.Lecturer.GetDescription();

            var lectureRoleId = this.Data.Roles
                .All()
                .Where(x => x.Name == lecturerRoleName)
                .Select(x => x.Id)
                .FirstOrDefault();

            return this.usersData
                .GetAllByRole(lectureRoleId)
                .Select(LecturerGridViewModel.ViewModel);
        }

        public override object GetById(object id) => this.Data.Users.GetById((string)id);

        public override string GetEntityKeyName() => this.GetEntityKeyNameByType(typeof(LecturerGridViewModel));

        public ActionResult Index() => this.View();
    }
}