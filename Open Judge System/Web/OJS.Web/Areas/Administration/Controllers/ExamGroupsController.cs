namespace OJS.Web.Areas.Administration.Controllers
{
    using System;
    using System.Collections;
    using System.Data.Entity;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Common;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Services.Business.ExamGroups;
    using OJS.Services.Data.Contests;
    using OJS.Services.Data.ExamGroups;
    using OJS.Services.Data.Users;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Areas.Administration.ViewModels.ExamGroups;
    using OJS.Web.Common.Attributes;
    using OJS.Web.Common.Extensions;

    using DetailModelType = OJS.Web.Areas.Administration.ViewModels.User.UserProfileSimpleAdministrationViewModel;
    using GeneralResource = Resources.Areas.Administration.AdministrationGeneral;
    using Resource = Resources.Areas.Administration.ExamGroups.ExamGroupsControllers;
    using ViewModelType = OJS.Web.Areas.Administration.ViewModels.ExamGroups.ExamGroupAdministrationViewModel;

    public class ExamGroupsController : LecturerBaseGridController
    {
        private readonly IExamGroupsDataService examGroupsData;
        private readonly IUsersDataService usersData;
        private readonly IContestsDataService contestsData;
        private readonly IExamGroupsBusinessService examGroupsBusiness;

        public ExamGroupsController(
            IOjsData data,
            IExamGroupsDataService examGroupsData,
            IUsersDataService usersData,
            IContestsDataService contestsData,
            IExamGroupsBusinessService examGroupsBusiness)
            : base(data)
        {
            this.examGroupsData = examGroupsData;
            this.usersData = usersData;
            this.contestsData = contestsData;
            this.examGroupsBusiness = examGroupsBusiness;
        }

        public ActionResult Index() => this.View();

        public override IEnumerable GetData()
        {
            var examGroups = this.examGroupsData.GetAll();

            if (this.User.IsLecturer() && !this.User.IsAdmin())
            {
                examGroups = this.examGroupsData.GetAllByLecturer(this.UserProfile.Id);
            }

            return examGroups.Select(ViewModelType.FromExamGroup);
        }

        public override object GetById(object id) => this.GetByIdAsNoTracking((int)id);

        [HttpPost]
        public ActionResult Create([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            if (!model.ContestId.HasValue)
            {
                this.ModelState.AddModelError(nameof(model.ContestId), string.Empty);
                return this.GridOperation(request, model);
            }

            var contestId = this.contestsData
                .GetByIdQuery(model.ContestId.Value)
                .Select(c => c.Id)
                .FirstOrDefault();

            if (contestId == default(int))
            {
                this.ModelState.AddModelError(nameof(model.ContestId), string.Empty);
                return this.GridOperation(request, model);
            }

            if (!this.UserHasContestRights(contestId))
            {
                this.ModelState.AddModelError(nameof(model.ContestId), Resource.Cannot_attach_contest);
                return this.GridOperation(request, model);
            }

            model.Id = (int)this.BaseCreate(model.GetEntityModel());
            return this.GridOperation(request, model);
        }

        [HttpPost]
        public ActionResult Update([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            if (!model.Id.HasValue)
            {
                return this.GridOperation(request, model);
            }

            var examGroup = model.GetEntityModel(this.GetByIdAsNoTracking(model.Id.Value));

            if (examGroup.ContestId.HasValue)
            {
                if (!this.contestsData.ExistsById(examGroup.ContestId.Value))
                {
                    this.ModelState.AddModelError(nameof(model.ContestId), string.Empty);
                    return this.GridOperation(request, model);
                }

                if (!this.UserHasContestRights(examGroup.ContestId.Value))
                {
                    this.ModelState.AddModelError(nameof(model.ContestId), Resource.Cannot_attach_contest);
                    return this.GridOperation(request, model);
                }
            }

            this.BaseUpdate(examGroup);

            return this.GridOperation(request, model);
        }

        [HttpPost]
        public ActionResult Destroy([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            if (model.ContestId != null)
            {
                if (!this.UserHasContestRights(model.ContestId.Value))
                {
                    this.ModelState.AddModelError(string.Empty, GeneralResource.No_privileges_message);
                    return this.GridOperation(request, model);
                }

                if (this.contestsData.IsActiveById(model.ContestId.Value))
                {
                    this.ModelState.AddModelError(string.Empty, Resource.Cannot_delete_group_with_active_contest);
                    return this.GridOperation(request, model);
                }
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
            var contestId = this.examGroupsData.GetContestIdById(id);

            if (!contestId.HasValue)
            {
                this.ModelState.AddModelError(string.Empty, Resource.Cannot_remove_users);
                return this.RedirectToAction<ExamGroupsController>(c => c.Index());
            }

            if (!this.UserHasContestRights(contestId.Value))
            {
                this.TempData.AddDangerMessage(GeneralResource.No_privileges_message);
                return this.RedirectToAction<ExamGroupsController>(c => c.Index());
            }

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

            if (examGroup.ContestId == null)
            {
                this.ModelState.AddModelError(string.Empty, Resource.Cannot_add_users);
                return this.GridOperation(request, null);
            }

            if (!this.UserHasContestRights(examGroup.ContestId.Value))
            {
                this.ModelState.AddModelError(string.Empty, GeneralResource.No_privileges_message);
                return this.GridOperation(request, null);
            }

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

        [AjaxOnly]
        public ActionResult BulkAddUsersPartial(int id, string name) =>
            this.PartialView("_BulkAddUsersToExamGroup", new BulkAddUsersToExamGroupViewModel(id, name));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BulkAddUsersToExamGroup(BulkAddUsersToExamGroupViewModel model)
        {
            var contestId = this.examGroupsData.GetContestIdById(model.ExamGroupId);

            if (!contestId.HasValue)
            {
                return this.JsonError(Resource.Cannot_add_users);
            }

            if (!this.UserHasContestRights(contestId.Value))
            {
                return this.JsonError(GeneralResource.No_privileges_message);
            }

            var usernames = (model.UserNamesText ?? string.Empty)
                .Split(new[] { ",", " ", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Where(username => Regex.IsMatch(username, GlobalConstants.UsernameRegEx));

            this.examGroupsBusiness.AddUsersByIdAndUsernames(model.ExamGroupId, usernames);

            return this.JsonSuccess(Resource.Users_added);
        }

        public override string GetEntityKeyName() => this.GetEntityKeyNameByType(typeof(ExamGroup));

        private ExamGroup GetByIdAsNoTracking(int id) =>
            this.examGroupsData
                .GetByIdQuery(id)
                .AsNoTracking()
                .FirstOrDefault();

        private bool UserHasContestRights(int contestId) =>
            this.User.IsAdmin() ||
            this.contestsData.IsUserLecturerInByContestAndUser(contestId, this.UserProfile.Id);
    }
}