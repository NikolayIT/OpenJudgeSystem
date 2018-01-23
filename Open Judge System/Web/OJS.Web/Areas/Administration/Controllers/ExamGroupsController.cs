namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Data;
    using OJS.Services.Data.ExamGroups;
    using OJS.Services.Data.Users;
    using OJS.Web.Areas.Administration.Controllers.Common;

    using DetailModelType = OJS.Web.Areas.Administration.ViewModels.ExamGroups.UserInExamGroupViewModel;
    using ViewModelType = OJS.Web.Areas.Administration.ViewModels.ExamGroups.ExamGroupAdministrationViewModel;

    public class ExamGroupsController : LecturerBaseGridController
    {
        private readonly IExamGroupsDataService examGroupsData;
        private readonly IUsersDataService usersData;

        public ExamGroupsController(
            IOjsData data,
            IExamGroupsDataService examGroupsData,
            IUsersDataService usersData)
            : base(data)
        {
            this.examGroupsData = examGroupsData;
            this.usersData = usersData;
        }

        public ActionResult Index() => this.View();

        public override IEnumerable GetData() =>
            this.examGroupsData
            .All()
            .Select(ViewModelType.FromExamGroup);

        public override object GetById(object id) => this.examGroupsData.GetById((int)id);

        public ActionResult Create() => this.View();

        [HttpPost]
        public JsonResult UsersInExamGroup([DataSourceRequest]DataSourceRequest request, int id)
        {
            var users = this.examGroupsData
                .GetUsersById(id)
                .Select(DetailModelType.FromUserProfile);

            return this.Json(users.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult RemoveUserFromExamGroup(
            [DataSourceRequest]DataSourceRequest request,
            DetailModelType model,
            int id)
        {
            this.examGroupsData.RemoveUserByIdAndUser(id, model.UserId);
            return this.GridOperation(request, model);
        }

        [HttpPost]
        public ActionResult AddUserToExamGroup(
            [DataSourceRequest]DataSourceRequest request,
            int id,
            string userId)
        {
            var user = this.usersData.GetById(userId);
            var examGroup = this.examGroupsData.GetById(id);

            examGroup.Users.Add(user);

            this.examGroupsData.Update(examGroup);

            var result = new DetailModelType
            {
                UserId = user.Id,
                Username = user.UserName,
                FirstName = user.UserSettings.FirstName,
                LastName = user.UserSettings.LastName,
                Email = user.Email
            };

            return this.Json(new[] { result }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
    }
}