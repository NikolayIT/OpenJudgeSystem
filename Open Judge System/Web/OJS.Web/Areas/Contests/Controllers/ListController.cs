namespace OJS.Web.Areas.Contests.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    using OJS.Data;
    using OJS.Services.Business.Contests;
    using OJS.Services.Data.ContestCategories;
    using OJS.Services.Data.Contests;
    using OJS.Web.Areas.Contests.ViewModels.Contests;
    using OJS.Web.Areas.Contests.ViewModels.Submissions;
    using OJS.Web.Common.Extensions;
    using OJS.Web.Controllers;

    using Resource = Resources.Areas.Contests.ContestsGeneral;

    public class ListController : BaseController
    {
        private readonly IContestsBusinessService contestsBusiness;
        private readonly IContestsDataService contestsData;
        private readonly IContestCategoriesDataService contestCategoriesData;

        public ListController(
            IOjsData data,
            IContestsBusinessService contestsBusiness,
            IContestsDataService contestsData,
            IContestCategoriesDataService contestCategoriesData)
            : base(data)
        {
            this.contestsBusiness = contestsBusiness;
            this.contestsData = contestsData;
            this.contestCategoriesData = contestCategoriesData;
        }

        public ActionResult Index() => this.View();

        public ActionResult ReadCategories(int? id)
        {
            var categories = this.Data.ContestCategories
                .All()
                .Where(cc => cc.IsVisible && (id.HasValue ? cc.ParentId == id : cc.ParentId == null))
                .OrderBy(cc => cc.OrderBy)
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

        public ActionResult ByCategory(int? id)
        {
            ContestCategoryViewModel contestCategory;
            if (id.HasValue)
            {
                var categories = this.contestCategoriesData
                    .GetVisibleByIdQuery(id.Value)
                    .OrderBy(cc => cc.OrderBy);

                if (this.contestCategoriesData.HasContestsById(id.Value))
                {
                    contestCategory = categories
                        .Select(ContestCategoryViewModel.FromLeafContestCategory)
                        .FirstOrDefault();
                }
                else
                {
                    contestCategory = categories
                        .Select(ContestCategoryViewModel.FromContestCategory)
                        .FirstOrDefault();
                }
            }
            else
            {
                contestCategory = new ContestCategoryViewModel
                {
                    CategoryName = Resource.Main_categories,
                    SubCategories = this.Data.ContestCategories
                        .All()
                        .Where(cc => cc.IsVisible && !cc.IsDeleted && cc.Parent == null)
                        .OrderBy(cc => cc.OrderBy)
                        .Select(ContestCategoryListViewModel.FromCategory)
                };
            }

            if (contestCategory == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Category_not_found);
            }

            foreach (var contest in contestCategory.Contests)
            {
                contest.UserIsAdminOrLecturerInContest = this.CheckIfUserHasContestPermissions(contest.Id);

                contest.UserCanCompete = this.contestsBusiness
                    .CanUserCompeteByContestByUserAndIsAdmin(contest.Id, this.UserProfile?.Id, this.User.IsAdmin());

                contest.UserIsParticipant = this.contestsData
                    .IsUserParticipantInByContestAndUser(contest.Id, this.UserProfile?.Id);
            }

            contestCategory.IsUserLecturerInContestCategory = this.CheckIfUserHasContestCategoryPermissions(contestCategory.Id);

            if (this.Request.IsAjaxRequest())
            {
                this.ViewBag.IsAjax = true;
                return this.PartialView(contestCategory);
            }

            this.ViewBag.IsAjax = false;
            return this.View(contestCategory);
        }

        public ActionResult BySubmissionType(int? id, string submissionTypeName)
        {
            SubmissionTypeViewModel submissionType;
            if (id.HasValue)
            {
                submissionType = this.Data.SubmissionTypes
                    .All()
                    .Where(st => st.Id == id.Value)
                    .Select(SubmissionTypeViewModel.FromSubmissionType)
                    .FirstOrDefault();
            }
            else
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, Resource.Invalid_request);
            }

            if (submissionType == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Submission_type_not_found);
            }

            var contests = this.Data.Contests
                .All()
                .Where(c => c.IsVisible && c.ProblemGroups
                    .SelectMany(pg => pg.Problems)
                    .Any(p => p.SubmissionTypes.Any(s => s.Id == submissionType.Id)))
                .OrderBy(c => c.OrderBy)
                .Select(ContestViewModel.FromContest);

            this.ViewBag.SubmissionType = submissionType.Name;
            return this.View(contests);
        }
    }
}