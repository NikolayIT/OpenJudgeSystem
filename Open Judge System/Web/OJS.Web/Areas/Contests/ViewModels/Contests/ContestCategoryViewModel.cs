namespace OJS.Web.Areas.Contests.ViewModels.Contests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using OJS.Data.Models;

    public class ContestCategoryViewModel
    {
        public static Expression<Func<ContestCategory, ContestCategoryViewModel>> FromContestCategory
        {
            get
            {
                return contestCategory =>
                    new ContestCategoryViewModel
                    {
                        CategoryName = contestCategory.Name,
                        Contests = contestCategory.Contests
                            .AsQueryable()
                            .Where(x => x.IsVisible && !x.IsDeleted)
                            .OrderBy(x => x.OrderBy)
                            .ThenByDescending(x => x.EndTime)
                            .Select(ContestViewModel.FromContest),
                        SubCategories = contestCategory.Children
                            .AsQueryable()
                            .Select(ContestCategoryListViewModel.FromCategory)
                    };
            }
        }

        public string CategoryName { get; set; }

        public IEnumerable<ContestViewModel> Contests { get; set; }

        public IEnumerable<ContestCategoryListViewModel> SubCategories { get; set; }
    }
}