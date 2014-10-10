namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Areas.Administration.ViewModels.LecturersInContests;
    using OJS.Web.ViewModels.Common;

    using DatabaseModelType = OJS.Data.Models.LecturerInContest;
    using DetailModelType = OJS.Web.Areas.Administration.ViewModels.LecturersInContests.LecturerInContestGridViewModel;

    public class LecturersInContestsController : AdministrationBaseGridController
    {
        private const int MaxTrainigsToTake = 5;

        public LecturersInContestsController(IOjsData data)
            : base(data)
        {
        }

        public ActionResult Index()
        {
            return this.View();
        }

        public override IEnumerable GetData()
        {
            var lecturerRoleName = SystemRole.Lecturer.GetDescription();

            var lectureRoleId = this.Data.Roles
                .All()
                .Where(x => x.Name == lecturerRoleName)
                .Select(x => x.Id)
                .FirstOrDefault();

            return this.Data.Users
                .All()
                .Where(x => x.Roles.Any(y => y.RoleId == lectureRoleId))
                .Select(DetailModelType.ViewModel);
        }

        public override object GetById(object id)
        {
            return this.Data.Users.GetById((string)id);
        }

        public override string GetEntityKeyName()
        {
            return this.GetEntityKeyNameByType(typeof(DetailModelType));
        }

        [HttpGet]
        public ActionResult AvailableContests(string text)
        {
            var contests = this.Data.Contests.All();

            if (!string.IsNullOrWhiteSpace(text))
            {
                contests = contests.Where(x => x.Name.StartsWith(text));
            }

            var result = contests
                .OrderBy(x => x.Name)
                .Take(MaxTrainigsToTake)
                .Select(DropdownViewModel.FromContest);

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ReadContestsForLecturer([DataSourceRequest]DataSourceRequest request, string lecturerId)
        {
            var trainings = this.Data.LecturersInContests
                .All()
                .Where(x => x.LecturerId == lecturerId)
                .Select(LecturerInContestShortViewModel.ViewModel);

            return this.Json(trainings.ToDataSourceResult(request));
        }

        [HttpPost]
        public ActionResult AddContestToLecturer([DataSourceRequest]DataSourceRequest request, string userId, LecturerInContestShortViewModel model)
        {
            model.LecturerId = userId;

            var isInContest =
                this.Data.LecturersInContests.All()
                    .Any(x => x.LecturerId == model.LecturerId && x.LecturerId == model.LecturerId);

            if (isInContest)
            {
                this.ModelState.AddModelError("LecturerId", "Потребителят вече е лектор в това състезание.");
            }
            else
            {
                var lecturerInContest = new DatabaseModelType
                {
                    LecturerId = model.LecturerId,
                    ContestId = model.ContestId
                };

                this.Data.LecturersInContests.Add(lecturerInContest);
                this.Data.SaveChanges();

                model.ContestName = this.Data.Contests.GetById(model.ContestId).Name;
            }

            return this.Json(new[] { model }.ToDataSourceResult(request, this.ModelState));
        }

        [HttpPost]
        public ActionResult UpdateContestForLecturer([DataSourceRequest] DataSourceRequest request, LecturerInContestShortViewModel model)
        {
            var trainerInTraining = this.Data.LecturersInContests
                .All()
                .FirstOrDefault(x => x.LecturerId == model.LecturerId && x.ContestId == model.ContestId);

            if (trainerInTraining == null)
            {
                this.ModelState.AddModelError("ContestId", "Няма такъв лектор в състезанието.");
            }
            else
            {
                this.Data.SaveChanges();
            }

            return this.Json(new[] { model }.ToDataSourceResult(request, this.ModelState));
        }

        [HttpPost]
        public ActionResult DeleteContestForLecturer([DataSourceRequest]DataSourceRequest request, LecturerInContestShortViewModel model)
        {
            var trainerInTraining = this.Data.LecturersInContests
                .All()
                .FirstOrDefault(x => x.LecturerId == model.LecturerId && x.ContestId == model.ContestId);

            this.Data.LecturersInContests.Delete(trainerInTraining);
            this.Data.SaveChanges();

            return this.Json(this.ModelState.ToDataSourceResult(request));
        }
    }
}