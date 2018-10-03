namespace OJS.Services.Cache
{
    using System.Collections.Generic;

    using OJS.Common.Constants;
    using OJS.Services.Cache.Models;
    using OJS.Services.Common;

    public interface ICacheItemsProviderService : IService
    {
        IEnumerable<ContestCategoryListViewModel> GetContestSubCategoriesList(
            int? categoryId,
            int? cacheSeconds = CacheConstants.OneDayInSeconds);

        IEnumerable<ContestCategoryListViewModel> GetContestCategoryParentsList(
            int categoryId,
            int? cacheSeconds = CacheConstants.OneDayInSeconds);

        IEnumerable<CategoryMenuItemViewModel> GetMainContestCategories(
            int? cacheSeconds = CacheConstants.OneDayInSeconds);

        string GetContestCategoryName(
            int categoryId,
            int? cacheSeconds = CacheConstants.OneDayInSeconds);

        void ClearContestCategory(int categoryId);
    }
}