namespace OJS.Web.Infrastructure.Filters
{
    using System.Web.Mvc;

    using OJS.Services.Cache;
    using OJS.Web.Infrastructure.Filters.Attributes;
    using OJS.Web.Infrastructure.Filters.Contracts;

    public class PopulateMainContestCategoriesIntoViewBagFilter
        : IActionFilter<PopulateMainContestCategoriesIntoViewBagAttribute>
    {
        private readonly ICacheItemsProviderService cacheItems;

        public PopulateMainContestCategoriesIntoViewBagFilter(ICacheItemsProviderService cacheItems) =>
            this.cacheItems = cacheItems;

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

            var mainContestCategories = this.cacheItems.GetMainContestCategories();

            viewResult.ViewBag.MainCategories = mainContestCategories;
        }
    }
}