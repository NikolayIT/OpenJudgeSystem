namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Services.Data.Contests;
    using OJS.Services.Data.ExamGroups;
    using OJS.Services.Data.Users;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Common.Extensions;

    using DetailModelType = OJS.Web.Areas.Administration.ViewModels.User.UserProfileSimpleAdministrationViewModel;
    using Resource = Resources.Areas.Administration.ExamGroups.ExamGroupsController;
    using ViewModelType = OJS.Web.Areas.Administration.ViewModels.ExamGroups.ExamGroupAdministrationViewModel;

    public class ExamGroupsController : LecturerBaseGridController
    {
        private readonly IExamGroupsDataService examGroupsData;
        private readonly IUsersDataService usersData;
        private readonly IContestsDataService contestsData;

        public ExamGroupsController(
            IOjsData data,
            IExamGroupsDataService examGroupsData,
            IUsersDataService usersData,
            IContestsDataService contestsData)
            : base(data)
        {
            this.examGroupsData = examGroupsData;
            this.usersData = usersData;
            this.contestsData = contestsData;
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
            var hasContest = this.examGroupsData
                .GetByIdQuery(model.Id.Value)
                .Any(eg => eg.Contest != null && !eg.Contest.IsDeleted);

            if (hasContest)
            {
                this.TempData.AddDangerMessage(Resource.Cannot_delete_group_with_contest);
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
            var examGroup = this.examGroupsData.GetById(id);
            var user = this.usersData.GetById(userId);

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

        public ActionResult GetAvailableUsersForExamGroup(string userFilter)
        {
            var result = new List<SelectListItem>();

            if (!string.IsNullOrWhiteSpace(userFilter))
            {
                var users = this.usersData
                     .GetAll()
                     .Where(u => u.UserName.ToLower().Contains(userFilter.ToLower()));

                result = users
                    .Select(u => new SelectListItem
                    {
                        Text = u.UserName,
                        Value = u.Id
                    })
                    .ToList();
            }

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetContests(string contestFilter)
        {
            var contests = this.contestsData.GetAll();

            if (!this.User.IsAdmin() && this.User.IsLecturer())
            {
                contests = contests
                    .Where(c => c.Category.Lecturers.Any(l => l.LecturerId == this.UserProfile.Id) ||
                                c.Lecturers.Any(l => l.LecturerId == this.UserProfile.Id));
            }

            if (!string.IsNullOrWhiteSpace(contestFilter))
            {
                contests = contests.Where(c => c.Name.Contains(contestFilter));
            }

            var result = contests
                .ToList()
                .Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString(CultureInfo.InvariantCulture)
                })
                .OrderByDescending(c => int.Parse(c.Value));

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        public override string GetEntityKeyName() => this.GetEntityKeyNameByType(typeof(ExamGroup));

        private ExamGroup GetByIdAsNoTracking(int id) => this.examGroupsData
            .GetByIdQuery(id)
            .AsNoTracking()
            .FirstOrDefault();
    }
}