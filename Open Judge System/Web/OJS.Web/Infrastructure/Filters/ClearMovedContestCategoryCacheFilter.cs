namespace OJS.Web.Infrastructure.Filters
{
    using System.Web.Mvc;

    using OJS.Services.Cache;
    using OJS.Web.Infrastructure.Filters.Attributes;
    using OJS.Web.Infrastructure.Filters.Contracts;

    public class ClearMovedContestCategoryCacheFilter
        : IActionFilter<ClearMovedContestCategoryCacheAttribute>
    {
        private readonly ICacheItemsProviderService cacheItems;

        public ClearMovedContestCategoryCacheFilter(ICacheItemsProviderService cacheItems) =>
            this.cacheItems = cacheItems;

        public void OnActionExecuting(
            ClearMovedContestCategoryCacheAttribute attribute,
            ActionExecutingContext filterContext)
        {
            var categoryIdAsString = filterContext
                .HttpContext
                .Request
                .Params[attribute.MovedCategoryIdParamName];

            var movedToCategoryIdAsString = filterContext
                .HttpContext
                .Request
                .Params[attribute.MovedToCategoryIdParamName];

            if (!int.TryParse(categoryIdAsString, out var categoryId))
            {
                return;
            }

            this.cacheItems.ClearContestCategory(categoryId);

            if (!int.TryParse(movedToCategoryIdAsString, out var movedToCategoryId))
            {
                return;
            }

            if (movedToCategoryId != categoryId)
            {
                this.cacheItems.ClearContestCategory(movedToCategoryId);
            }
        }

        public void OnActionExecuted(
            ClearMovedContestCategoryCacheAttribute attribute,
            ActionExecutedContext filterContext)
        {
        }
    }
}