namespace OJS.Web.Areas.Contests.ViewModels.Contests
{
    using System.Collections.Generic;
    using System.Linq;

    using OJS.Services.Cache.Models;

    using X.PagedList;

    public class ContestCategoryViewModel
    {
        public int Id { get; set; }

        public string CategoryName { get; set; }

        public IPagedList<ContestListViewModel> Contests { get; set; }

        public IEnumerable<ContestCategoryListViewModel> SubCategories { get; set; } =
            Enumerable.Empty<ContestCategoryListViewModel>();

        public bool IsUserLecturerInContestCategory { get; set; }
    }
}