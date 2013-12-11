namespace OJS.Web.Areas.Contests.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Common.Extensions;
    using OJS.Data;
    using OJS.Web.Areas.Contests.ViewModels;
    using OJS.Web.Controllers;

    public class ListController : BaseController
    {
        public ListController(IOjsData data)
            : base(data)
        {
        }

        public ActionResult Index()
        {
            var contests = this.Data.Contests.All().Select(ContestViewModel.FromContest).ToList();
            return this.View(contests);
        }

        public ActionResult ReadCategories(int? id)
        {
            var categories =
                this.Data.ContestCategories.All()
                    .Where(x => x.IsVisible)
                    .Where(x => id.HasValue ? x.ParentId == id : x.ParentId == null)
                    .OrderBy(x => x.OrderBy)
                    .Select(ContestCategoryListViewModel.FromCategory);

            return this.Json(categories, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetParents(int id)
        {
            var categoryIds = new List<int>();
            var category = this.Data.ContestCategories.GetById(id);

            categoryIds.Add(category.Id);
            var parent = category.Parent;

            while (parent != null)
            {
                categoryIds.Add(parent.Id);
                parent = parent.Parent;
            }

            categoryIds.Reverse();

            return this.Json(categoryIds, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ByCategory(int id)
        {
            var contestCategory =
                this.Data.ContestCategories.All()
                    .Where(x => x.Id == id && !x.IsDeleted && x.IsVisible)
                    .OrderBy(x => x.OrderBy)
                    .Select(ContestCategoryViewModel.FromContestCategory)
                    .FirstOrDefault();

            if (this.Request.IsAjaxRequest())
            {
                return this.PartialView(contestCategory);
            }

            return this.View(contestCategory);
        }

        public ActionResult BySubmissionType(string submissionType)
        {
            var submissionName = submissionType.FromUrlSafeString();
            this.ViewBag.SubmissionType = submissionName;

            var contests =
                this.Data.Contests
                                .All()
                                .Where(c => !c.IsDeleted &&
                                            c.IsVisible &&
                                            c.SubmissionTypes.Any(s => s.Name == submissionName))
                                .OrderBy(x => x.OrderBy)
                                .Select(ContestViewModel.FromContest);

            return this.View(contests);
        }
    }
}