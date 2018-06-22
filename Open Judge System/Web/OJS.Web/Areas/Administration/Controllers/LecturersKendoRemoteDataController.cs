namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Data;
    using OJS.Services.Data.ContestCategories;
    using OJS.Services.Data.Contests;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Common.Attributes;
    using OJS.Web.Common.Extensions;
    using OJS.Web.ViewModels.Common;

    public class LecturersKendoRemoteDataController : KendoRemoteDataBaseController
    {
        private readonly IContestsDataService contestsData;
        private readonly IContestCategoriesDataService contestCategoriesData;

        public LecturersKendoRemoteDataController(
            IOjsData data,
            IContestsDataService contestsData,
            IContestCategoriesDataService contestCategoriesData)
            : base(data)
        {
            this.contestsData = contestsData;
            this.contestCategoriesData = contestCategoriesData;
        }

        private bool UserIsNotAdminButLecturer => !this.User.IsAdmin() && this.User.IsLecturer();

        [AjaxOnly]
        public JsonResult GetAvailableContestsContaining(string contestFilter)
        {
            var contests = this.contestsData.GetAll();

            if (this.UserIsNotAdminButLecturer)
            {
                contests = this.contestsData.GetAllByLecturer(this.UserProfile.Id);
            }

            if (!string.IsNullOrWhiteSpace(contestFilter))
            {
                contests = contests.Where(c => c.Name.Contains(contestFilter)).Take(DefaultItemsToTake);
            }

            var result = contests
                .OrderByDescending(c => c.CreatedOn)
                .Select(DropdownViewModel.FromContest);

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        public JsonResult GetAvailableCategories()
        {
            var categories = this.contestCategoriesData.GetAllVisible();

            if (this.UserIsNotAdminButLecturer)
            {
                categories = this.contestCategoriesData.GetAllVisibleByLecturer(this.UserProfile.Id);
            }

            var result = categories
                .OrderBy(cc => cc.Name)
                .Select(DropdownViewModel.FromContestCategory);

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        public JsonResult GetCascadeContestsFromCategory(int categoryId)
        {
            var contests = this.contestsData.GetAllVisibleByCategory(categoryId);

            if (this.UserIsNotAdminButLecturer)
            {
                contests = this.contestsData.GetAllVisibleByCategoryAndLecturer(categoryId, this.UserProfile.Id);
            }

            var result = contests
                .OrderBy(c => c.Name)
                .Select(DropdownViewModel.FromContest);

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}