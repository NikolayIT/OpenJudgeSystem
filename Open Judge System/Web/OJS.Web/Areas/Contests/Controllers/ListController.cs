﻿namespace OJS.Web.Areas.Contests.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    using OJS.Data;
    using OJS.Services.Business.Contests;
    using OJS.Services.Cache;
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
        private readonly ICacheItemsProviderService cacheItems;

        public ListController(
            IOjsData data,
            IContestsBusinessService contestsBusiness,
            IContestsDataService contestsData,
            IContestCategoriesDataService contestCategoriesData,
            ICacheItemsProviderService cacheItems)
            : base(data)
        {
            this.contestsBusiness = contestsBusiness;
            this.contestsData = contestsData;
            this.contestCategoriesData = contestCategoriesData;
            this.cacheItems = cacheItems;
        }

        public ActionResult Index() => this.View();

        public ActionResult ReadCategories(int? id) =>
            this.Json(this.cacheItems.GetContestSubCategoriesList(id), JsonRequestBehavior.AllowGet);

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
            var contestCategory = this.GetContestCategoryFromCache(id);

            if (contestCategory == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Category_not_found);
            }

            if (id.HasValue && this.contestCategoriesData.HasContestsById(id.Value))
            {
                contestCategory.Contests = this.GetContestsInCategory(id.Value);
            }

            contestCategory.IsUserLecturerInContestCategory =
                this.CheckIfUserHasContestCategoryPermissions(contestCategory.Id);

            var isAjaxRequest = this.Request.IsAjaxRequest();

            this.ViewBag.IsAjax = isAjaxRequest;

            if (isAjaxRequest)
            {
                return this.PartialView(contestCategory);
            }
 
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

            var contests = this.contestsData
                .GetAllVisibleBySubmissionType(submissionType.Id)
                .OrderBy(c => c.OrderBy)
                .Select(ContestViewModel.FromContest);

            this.ViewBag.SubmissionType = submissionType.Name;
            return this.View(contests);
        }

        private IEnumerable<ContestListViewModel> GetContestsInCategory(int categoryId)
        {
            var contests = this.contestsData
                .GetAllVisibleByCategory(categoryId)
                .OrderBy(c => c.OrderBy)
                .ThenByDescending(c => c.EndTime ?? c.PracticeEndTime ?? c.PracticeStartTime)
                .Select(ContestListViewModel.FromContest)
                .ToList();

            foreach (var contest in contests)
            {
                contest.UserIsAdminOrLecturerInContest = this.CheckIfUserHasContestPermissions(contest.Id);

                contest.UserCanCompete = this.contestsBusiness
                    .CanUserCompeteByContestByUserAndIsAdmin(contest.Id, this.UserProfile?.Id, this.User.IsAdmin());

                contest.UserIsParticipant = this.contestsData
                    .IsUserParticipantInByContestAndUser(contest.Id, this.UserProfile?.Id);
            }

            return contests;
        }

        private ContestCategoryViewModel GetContestCategoryFromCache(int? id)
        {
            var contestCategory = new ContestCategoryViewModel
            {
                Id = id ?? default(int),
                CategoryName = Resource.Main_categories
            };

            if (id.HasValue)
            {
                var categoryName = this.cacheItems.GetContestCategoryName(id.Value);

                if (categoryName == null)
                {
                    return null;
                }

                contestCategory.CategoryName = categoryName;
            }

            if (!contestCategory.SubCategories.Any())
            {
                contestCategory.SubCategories = this.cacheItems.GetContestSubCategoriesList(id);
            }

            return contestCategory;
        }   
    }
}