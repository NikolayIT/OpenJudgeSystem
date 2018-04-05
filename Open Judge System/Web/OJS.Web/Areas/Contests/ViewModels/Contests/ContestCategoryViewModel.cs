namespace OJS.Web.Areas.Contests.ViewModels.Contests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using OJS.Data.Models;

    public class ContestCategoryViewModel
    {
        public static Expression<Func<ContestCategory, ContestCategoryViewModel>> FromLeafContestCategory =>
            contestCategory => new ContestCategoryViewModel
            {
                Id = contestCategory.Id,
                CategoryName = contestCategory.Name,
                Contests = contestCategory.Contests.AsQueryable()
                    .Where(c => c.IsVisible && !c.IsDeleted)
                    .OrderBy(c => c.OrderBy)
                    .ThenByDescending(c => c.EndTime ?? c.PracticeEndTime ?? c.PracticeStartTime)
                    .Select(ContestViewModel.FromContest),
                SubCategories = contestCategory.Children
                    .AsQueryable()
                    .Where(cc => !cc.IsDeleted && cc.IsVisible)
                    .OrderBy(cc => cc.OrderBy)
                    .Select(ContestCategoryListViewModel.FromCategory)
            };

        public static Expression<Func<ContestCategory, ContestCategoryViewModel>> FromContestCategory =>
            contestCategory => new ContestCategoryViewModel
            {
                Id = contestCategory.Id,
                CategoryName = contestCategory.Name,
                SubCategories = contestCategory.Children
                    .AsQueryable()
                    .Where(cc => !cc.IsDeleted && cc.IsVisible)
                    .OrderBy(cc => cc.OrderBy)
                    .Select(ContestCategoryListViewModel.FromCategory)
            };

        public int Id { get; set; }

        public string CategoryName { get; set; }

        public IEnumerable<ContestViewModel> Contests { get; set; } = Enumerable.Empty<ContestViewModel>();

        public IEnumerable<ContestCategoryListViewModel> SubCategories { get; set; }

        public bool IsUserLecturerInContestCategory { get; set; }
    }
}