namespace OJS.Web.Areas.Contests.ViewModels
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
                        Contests = contestCategory.Contests.AsQueryable().OrderBy(x => x.OrderBy).Select(ContestViewModel.FromContest),
                    };
            }
        }

        public string CategoryName { get; set; }

        public IEnumerable<ContestViewModel> Contests { get; set; }
    }
}