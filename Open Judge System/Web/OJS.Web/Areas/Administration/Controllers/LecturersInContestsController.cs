namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Data;
    using OJS.Services.Data.Contests;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Areas.Administration.ViewModels.LecturersInContests;

    using DatabaseModelType = OJS.Data.Models.LecturerInContest;

    public class LecturersInContestsController : AdministrationBaseController
    {
        private readonly IContestsDataService contestsData;

        public LecturersInContestsController(
            IOjsData data,
            IContestsDataService contestsData)
            : base(data) => this.contestsData = contestsData;

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

                model.ContestName = this.contestsData.GetNameById(model.ContestId);
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