namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Areas.Administration.ViewModels.LecturersInContestCategories;
    using OJS.Web.ViewModels.Common;

    public class LecturersInContestCategoriesController : AdministrationBaseController
    {
        private const int MaxContestCategoriesToTake = 5;

        public LecturersInContestCategoriesController(IOjsData data)
            : base(data)
        {
        }

        [HttpGet]
        public ActionResult AvailableContestCategories(string text)
        {
            var contestCategories = this.Data.ContestCategories.All();

            if (!string.IsNullOrWhiteSpace(text))
            {
                contestCategories = contestCategories.Where(x => x.Name.Contains(text));
            }

            var result = contestCategories
                .OrderBy(x => x.Name)
                .Take(MaxContestCategoriesToTake)
                .Select(DropdownViewModel.FromContestCategory);

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ReadContestCategoriesForLecturer([DataSourceRequest]DataSourceRequest request, string lecturerId)
        {
            var trainings = this.Data.LecturersInContestCategories
                .All()
                .Where(x => x.LecturerId == lecturerId)
                .Select(LecturerInContestCategoryShortViewModel.ViewModel);

            return this.Json(trainings.ToDataSourceResult(request));
        }

        [HttpPost]
        public ActionResult AddContestCategoryToLecturer(
            [DataSourceRequest]DataSourceRequest request,
            string userId,
            LecturerInContestCategoryShortViewModel model)
        {
            model.LecturerId = userId;

            var isInContestCategory = this.Data.LecturersInContestCategories
                .All()
                .Any(x => x.LecturerId == model.LecturerId && x.ContestCategoryId == model.ContestCategoryId);

            if (isInContestCategory)
            {
                this.ModelState.AddModelError("LecturerId", "Потребителят вече е лектор в тази категория състезания.");
            }
            else
            {
                var lecturerInContestCategory = new LecturerInContestCategory
                {
                    LecturerId = model.LecturerId,
                    ContestCategoryId = model.ContestCategoryId
                };

                this.Data.LecturersInContestCategories.Add(lecturerInContestCategory);
                this.Data.SaveChanges();

                var contestCategoryName = this.Data.ContestCategories
                    .All()
                    .Where(x => x.Id == model.ContestCategoryId)
                    .Select(x => x.Name)
                    .FirstOrDefault();

                model.ContestCategoryName = contestCategoryName;
            }

            return this.Json(new[] { model }.ToDataSourceResult(request, this.ModelState));
        }

        [HttpPost]
        public ActionResult DeleteContestCategoryForLecturer(
            [DataSourceRequest]DataSourceRequest request,
            LecturerInContestCategoryShortViewModel model)
        {
            var lecturerInContestCategory = this.Data.LecturersInContestCategories
                .All()
                .FirstOrDefault(x => x.LecturerId == model.LecturerId && x.ContestCategoryId == model.ContestCategoryId);

            this.Data.LecturersInContestCategories.Delete(lecturerInContestCategory);
            this.Data.SaveChanges();

            return this.Json(this.ModelState.ToDataSourceResult(request));
        }
    }
}