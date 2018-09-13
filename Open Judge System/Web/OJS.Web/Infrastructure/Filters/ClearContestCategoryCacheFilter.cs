namespace OJS.Web.Infrastructure.Filters
{
    using System.Web.Mvc;

    using OJS.Services.Cache;
    using OJS.Web.Infrastructure.Filters.Attributes;
    using OJS.Web.Infrastructure.Filters.Contracts;

    public class ClearContestCategoryCacheFilter : IActionFilter<ClearContestCategoryCacheAttribute>
    {
        private readonly ICacheItemsProviderService cacheItems;

        public ClearContestCategoryCacheFilter(ICacheItemsProviderService cacheItems) =>
            this.cacheItems = cacheItems;

        public void OnActionExecuted(
            ClearContestCategoryCacheAttribute attribute,
            ActionExecutedContext filterContext)
        {
        }

        public void OnActionExecuting(
            ClearContestCategoryCacheAttribute attribute,
            ActionExecutingContext filterContext)
        {
            var categoryIdAsString = filterContext
                .HttpContext
                .Request
                .Params[attribute.QuerKeyForCategoryId];

            if (int.TryParse(categoryIdAsString, out var categoryId))
            {
                this.cacheItems.ClearContestCategory(categoryId);
            }
        }
    }
}