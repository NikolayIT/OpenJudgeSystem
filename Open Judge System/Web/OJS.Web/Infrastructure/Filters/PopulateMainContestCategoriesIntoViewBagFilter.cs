namespace OJS.Web.Infrastructure.Filters
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Common.Constants;
    using OJS.Services.Common.Cache;
    using OJS.Services.Data.ContestCategories;
    using OJS.Web.Infrastructure.Filters.Attributes;
    using OJS.Web.Infrastructure.Filters.Contracts;
    using OJS.Web.ViewModels;

    public class PopulateMainContestCategoriesIntoViewBagFilter
        : IActionFilter<PopulateMainContestCategoriesIntoViewBagAttribute>
    {
        private readonly IContestCategoriesDataService contestCategoeriesData;
        private readonly ICacheService cache;

        public PopulateMainContestCategoriesIntoViewBagFilter(
            IContestCategoriesDataService contestCategoeriesData,
            ICacheService cache)
        {
            this.contestCategoeriesData = contestCategoeriesData;
            this.cache = cache;
        }

        public void OnActionExecuting(
            PopulateMainContestCategoriesIntoViewBagAttribute attribute,
            ActionExecutingContext filterContext)
        {
        }

        public void OnActionExecuted(
            PopulateMainContestCategoriesIntoViewBagAttribute attribute,
            ActionExecutedContext filterContext)
        {
            var httpContext = filterContext.RequestContext.HttpContext;
            var request = httpContext.Request;

            var viewResult = filterContext.Result as ViewResult;

            if (viewResult == null || request.IsAjaxRequest())
            {
                return;
            }

            var mainContestCategories = this.cache.Get(
                CacheConstants.MainContestCategoriesDropDown,
                this.GetMainContestCategoeriesDropDown);

            viewResult.ViewBag.MainCategories = mainContestCategories;
        }

        private IEnumerable<CategoryMenuItemViewModel> GetMainContestCategoeriesDropDown() =>
            this.contestCategoeriesData
                .GetAllVisible()
                .Where(x => !x.ParentId.HasValue)
                .OrderBy(x => x.OrderBy)
                .Select(CategoryMenuItemViewModel.FromCategory)
                .ToList();
    }
}