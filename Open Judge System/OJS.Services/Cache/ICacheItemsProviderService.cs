namespace OJS.Services.Cache
{
    using System.Collections.Generic;

    using OJS.Services.Cache.Models;
    using OJS.Services.Common;

    public interface ICacheItemsProviderService : IService
    {
        IEnumerable<ContestCategoryListViewModel> GetContestSubCategoriesList(
            int? categoryId,
            int? cacheSeconds = null);

        IEnumerable<CategoryMenuItemViewModel> GetMainContestCategoeries(int? cacheSeconds = null);

        string GetContestCategoryName(int categoryId, int? cacheSeconds = null);
    }
}