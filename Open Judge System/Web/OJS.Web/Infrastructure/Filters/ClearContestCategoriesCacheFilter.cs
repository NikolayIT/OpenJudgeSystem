namespace OJS.Web.Infrastructure.Filters
{
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Common.Constants;
    using OJS.Services.Common.Cache;
    using OJS.Services.Data.ContestCategories;
    using OJS.Web.Areas.Administration.Controllers;
    using OJS.Web.Areas.Administration.ViewModels.ContestCategory;
    using OJS.Web.Infrastructure.Filters.Attributes;
    using OJS.Web.Infrastructure.Filters.Contracts;

    public class ClearContestCategoriesCacheFilter : IActionFilter<ClearContestCategoriesCacheAttribute>
    {
        private readonly ICacheService cache;
        private readonly IContestCategoriesDataService contestCategoriesData;

        public ClearContestCategoriesCacheFilter(
            ICacheService cache,
            IContestCategoriesDataService contestCategoriesData)
        {
            this.cache = cache;
            this.contestCategoriesData = contestCategoriesData;
        }

        public void OnActionExecuting(
            ClearContestCategoriesCacheAttribute attribute,
            ActionExecutingContext filterContext)
        {
            int? categoryId;
            if (filterContext.ActionDescriptor.ActionName != nameof(ContestCategoriesController.MoveCategory))
            {
                categoryId = filterContext.ActionParameters.Values
                    .OfType<ContestCategoryAdministrationViewModel>()
                    .FirstOrDefault()
                    ?.Id;
            }
            else
            {
                categoryId = filterContext.ActionParameters.Values.OfType<int>().FirstOrDefault();
                var moveToCategoryId = filterContext.ActionParameters.Values.OfType<int?>().FirstOrDefault();

                if (moveToCategoryId.HasValue)
                {
                    this.ClearCacheFromCategory(moveToCategoryId.Value);
                }
            }

            if (!categoryId.HasValue)
            {
                return;
            }

            this.ClearCacheFromCategory(categoryId.Value);
        }

        public void OnActionExecuted(
            ClearContestCategoriesCacheAttribute attribute,
            ActionExecutedContext filterContext)
        {
            this.cache.Remove(CacheConstants.MainContestCategoriesDropDown);
            this.cache.Remove(CacheConstants.ContestCategoriesTree);
        }

        private void ClearCacheFromCategory(int categoryId)
        {
            var contestCategory = this.contestCategoriesData.GetById(categoryId);

            if (contestCategory == null)
            {
                return;
            }

            while (contestCategory != null)
            {
                var categoryNamecacheId = string.Format(
                    CacheConstants.ContestCategoryNameFormat,
                    contestCategory.Id);

                var subCategoriesCacheId = string.Format(
                    CacheConstants.ContestCategoriesTreeFormat,
                    contestCategory.Id);

                this.cache.Remove(categoryNamecacheId);
                this.cache.Remove(subCategoriesCacheId);

                contestCategory = contestCategory.Parent;
            }
        }
    }
}