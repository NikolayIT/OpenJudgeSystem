namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Services.Data.ExamGroups;
    using OJS.Services.Data.Users;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Common.Extensions;

    using DetailModelType = OJS.Web.Areas.Administration.ViewModels.ExamGroups.UserInExamGroupViewModel;
    using Resource = Resources.Areas.Administration.ExamGroups.ExamGroupsController;
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

        public override object GetById(object id) => this.GetByIdAsNoTracking((int)id);

        [HttpPost]
        public ActionResult Create([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            this.BaseCreate(model.GetEntityModel());
            return this.GridOperation(request, model);
        }

        [HttpPost]
        public ActionResult Update([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            if (model.Id.HasValue)
            {
                var entity = this.GetByIdAsNoTracking(model.Id.Value);

                var examGroup = model.GetEntityModel(entity);

                this.BaseUpdate(examGroup);
            }

            return this.GridOperation(request, model);
        }

        [HttpPost]
        public ActionResult Destroy([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            if (model.Id.HasValue && this.examGroupsData.GetUsersByIdQuery(model.Id.Value).Any())
            {
                this.TempData.AddDangerMessage(Resource.Cannot_delete_group_with_users);
                this.ModelState.AddModelError(string.Empty, string.Empty);
                return this.GridOperation(request, model);
            }

            this.BaseDestroy(model.Id);
            return this.GridOperation(request, model);
        }

        [HttpPost]
        public JsonResult UsersInExamGroup([DataSourceRequest]DataSourceRequest request, int id)
        {
            var users = this.examGroupsData
                .GetUsersByIdQuery(id)
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

        public ActionResult GetAvailableUsersForExamGroup(string text)
        {
            var users = this.usersData.GetAll();

            if (!string.IsNullOrEmpty(text))
            {
                users = users.Where(u => u.UserName.ToLower().Contains(text.ToLower()));
            }

            var result = users
                .ToList()
                .Select(pr => new SelectListItem
                {
                    Text = pr.UserName,
                    Value = pr.Id
                });

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        public override string GetEntityKeyName() => this.GetEntityKeyNameByType(typeof(ExamGroup));

        private ExamGroup GetByIdAsNoTracking(int id) => this.examGroupsData
            .GetByIdQuery(id)
            .AsNoTracking()
            .FirstOrDefault();
    }
}