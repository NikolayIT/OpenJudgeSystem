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
            var contests = this.contestsData
                .GetAll()
                .OrderByDescending(c => c.CreatedOn);

            if (this.UserIsNotAdminButLecturer)
            {
                contests = this.contestsData
                    .GetAllByLecturer(this.UserProfile.Id)
                    .OrderByDescending(c => c.CreatedOn);
            }

            if (!string.IsNullOrWhiteSpace(contestFilter))
            {
                contests = contests
                    .Where(c => c.Name.Contains(contestFilter))
                    .Take(DefaultItemsToTake)
                    .OrderByDescending(c => c.CreatedOn);
            }

            var result = contests
                .Select(c => new DropdownViewModel
                {
                    Name = c.Name,
                    Id = c.Id
                });

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        public JsonResult GetAvailableCategories()
        {
            var categories = this.contestCategoriesData.GetAll();

            if (this.UserIsNotAdminButLecturer)
            {
                categories = this.contestCategoriesData.GetByLecturer(this.UserProfile.Id);
            }

            var result = categories
                .Select(cc => new DropdownViewModel
                {
                    Name = cc.Name,
                    Id = cc.Id
                });

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        public JsonResult GetCascadeContestsFromCategory(int categoryId)
        {
            var contests = this.contestsData.GetByCategory(categoryId);

            var result = contests.Select(c => new DropdownViewModel
            {
                Name = c.Name,
                Id = c.Id
            });

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}