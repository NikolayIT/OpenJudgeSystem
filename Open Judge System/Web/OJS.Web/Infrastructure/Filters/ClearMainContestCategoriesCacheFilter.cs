namespace OJS.Web.Infrastructure.Filters
{
    using System.Web.Mvc;

    using OJS.Common.Constants;
    using OJS.Services.Common.Cache;
    using OJS.Web.Infrastructure.Filters.Attributes;
    using OJS.Web.Infrastructure.Filters.Contracts;

    public class ClearMainContestCategoriesCacheFilter : IActionFilter<ClearMainContestCategoriesCacheAttribute>
    {
        private readonly ICacheService cache;

        public ClearMainContestCategoriesCacheFilter(ICacheService cache) =>
            this.cache = cache;

        public void OnActionExecuting(
            ClearMainContestCategoriesCacheAttribute attribute,
            ActionExecutingContext filterContext)
        {
        }

        public void OnActionExecuted(
            ClearMainContestCategoriesCacheAttribute attribute,
            ActionExecutedContext filterContext)
        {
            this.cache.Remove(CacheConstants.MainContestCategoriesDropDown);
            this.cache.Remove(CacheConstants.ContestCategoriesTree);
        }
    }
}