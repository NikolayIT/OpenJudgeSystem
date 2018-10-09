namespace OJS.Services.Cache
{
    using System.Collections.Generic;
    using System.Linq;

    using OJS.Common.Constants;
    using OJS.Services.Cache.Models;
    using OJS.Services.Common.Cache;
    using OJS.Services.Data.ContestCategories;

    public class CacheItemsProviderService : ICacheItemsProviderService
    {
        private readonly ICacheService cache;
        private readonly IContestCategoriesDataService contestCategoriesData;

        public CacheItemsProviderService(
            ICacheService cache,
            IContestCategoriesDataService contestCategoriesData)
        {
            this.cache = cache;
            this.contestCategoriesData = contestCategoriesData;
        }

        public IEnumerable<ContestCategoryListViewModel> GetContestSubCategoriesList(
            int? categoryId,
            int? cacheSeconds)
        {
            var cacheId = categoryId.HasValue
                ? string.Format(CacheConstants.ContestSubCategoriesFormat, categoryId.Value)
                : CacheConstants.ContestCategoriesTree;

            return this.cache.Get(cacheId, GetSubCategories, cacheSeconds);

            IEnumerable<ContestCategoryListViewModel> GetSubCategories() =>
                this.contestCategoriesData
                    .GetAllVisible()
                    .Where(cc => categoryId.HasValue ? cc.ParentId == categoryId : cc.ParentId == null)
                    .OrderBy(cc => cc.OrderBy)
                    .Select(ContestCategoryListViewModel.FromCategory)
                    .ToList();
        }

        public IEnumerable<ContestCategoryListViewModel> GetContestCategoryParentsList(
            int categoryId,
            int? cacheSeconds = CacheConstants.OneDayInSeconds)
        {
            var cacheId = string.Format(CacheConstants.ContestParentCategoriesFormat, categoryId);

            var contestCategories = this.cache.Get(cacheId, GetParentCategories, cacheSeconds);

            IEnumerable<ContestCategoryListViewModel> GetParentCategories()
            {
                var categories = new List<ContestCategoryListViewModel>();
                var category = this.contestCategoriesData.GetById(categoryId);

                while (category != null)
                {
                    categories.Add(new ContestCategoryListViewModel
                    {
                        Id = category.Id,
                        Name = category.Name
                    });

                    category = category.Parent;
                }

                categories.Reverse();

                return categories;
            }

            return contestCategories;
        }

        public IEnumerable<CategoryMenuItemViewModel> GetMainContestCategories(int? cacheSeconds) =>
            this.cache.Get(
                CacheConstants.MainContestCategoriesDropDown,
                () =>
                     this.contestCategoriesData
                        .GetAllVisible()
                        .Where(x => !x.ParentId.HasValue)
                        .OrderBy(x => x.OrderBy)
                        .Select(CategoryMenuItemViewModel.FromCategory)
                        .ToList(),
                cacheSeconds);
            

        public string GetContestCategoryName(int categoryId, int? cacheSeconds) =>
            this.cache.Get(
                string.Format(CacheConstants.ContestCategoryNameFormat, categoryId),
                () => this.contestCategoriesData.GetNameById(categoryId),
                cacheSeconds);

        public void ClearContestCategory(int categoryId)
        {
            var contestCategory = this.contestCategoriesData.GetById(categoryId);

            if (contestCategory == null)
            {
                return;
            }

            contestCategory.Children
                .Select(cc => cc.Id)
                .ToList()
                .ForEach(RemoveCacheFromCategory);

            while (contestCategory != null)
            {
                RemoveCacheFromCategory(contestCategory.Id);

                contestCategory = contestCategory.Parent;
            }

            void RemoveCacheFromCategory(int contestCategoryId)
            {
                var categoryNameCacheId = string.Format(
                    CacheConstants.ContestCategoryNameFormat,
                    contestCategoryId);

                var subCategoriesCacheId = string.Format(
                    CacheConstants.ContestSubCategoriesFormat,
                    contestCategoryId);

                var parentCategoriesCacheId = string.Format(
                    CacheConstants.ContestParentCategoriesFormat,
                    contestCategoryId);

                this.cache.Remove(categoryNameCacheId);
                this.cache.Remove(subCategoriesCacheId);
                this.cache.Remove(parentCategoriesCacheId);
            }
        }
    }
}