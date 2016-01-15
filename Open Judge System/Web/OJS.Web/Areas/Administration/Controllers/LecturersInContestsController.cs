namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Data;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Areas.Administration.ViewModels.LecturersInContests;
    using OJS.Web.ViewModels.Common;

    using DatabaseModelType = OJS.Data.Models.LecturerInContest;

    public class LecturersInContestsController : AdministrationBaseController
    {
        private const int MaxContestsToTake = 5;

        public LecturersInContestsController(IOjsData data)
            : base(data)
        {
        }

        [HttpGet]
        public ActionResult AvailableContests(string text)
        {
            var contests = this.Data.Contests.All();

            if (!string.IsNullOrWhiteSpace(text))
            {
                contests = contests.Where(x => x.Name.Contains(text));
            }

            var result = contests
                .OrderBy(x => x.Name)
                .Take(MaxContestsToTake)
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
        public ActionResult AddContestToLecturer(
            [DataSourceRequest]DataSourceRequest request,
            string userId,
            LecturerInContestShortViewModel model)
        {
            model.LecturerId = userId;

            var isInContest = this.Data.LecturersInContests
                .All()
                .Any(x => x.LecturerId == model.LecturerId && x.ContestId == model.ContestId);

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

                var contestName = this.Data.Contests
                    .All()
                    .Where(x => x.Id == model.ContestId)
                    .Select(x => x.Name)
                    .FirstOrDefault();

                model.ContestName = contestName;
            }

            return this.Json(new[] { model }.ToDataSourceResult(request, this.ModelState));
        }

        [HttpPost]
        public ActionResult DeleteContestForLecturer(
            [DataSourceRequest]DataSourceRequest request,
            LecturerInContestShortViewModel model)
        {
            var lecturerInContest = this.Data.LecturersInContests
                .All()
                .FirstOrDefault(x => x.LecturerId == model.LecturerId && x.ContestId == model.ContestId);

            this.Data.LecturersInContests.Delete(lecturerInContest);
            this.Data.SaveChanges();

            return this.Json(this.ModelState.ToDataSourceResult(request));
        }
    }
}