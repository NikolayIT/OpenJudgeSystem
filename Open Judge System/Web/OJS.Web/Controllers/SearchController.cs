namespace OJS.Web.Controllers
{
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Data;
    using OJS.Services.Data.Contests;
    using OJS.Web.Common;
    using OJS.Web.ViewModels.Search;

    public class SearchController : BaseController
    {
        private readonly IContestsDataService contestsData;

        public SearchController(
            IOjsData data,
            IContestsDataService contestsData)
            : base(data)
        {
            this.contestsData = contestsData;
        }

        public ActionResult Index()
        {
            return this.View();
        }

        public ActionResult Results(string searchTerm)
        {
            var searchResult = new SearchResultGroupViewModel(searchTerm);

            if (searchResult.IsSearchTermValid)
            {
                var problemSearchResults = this.Data.Problems
                    .All()
                    .Include(p => p.ProblemGroup.Contest)
                    .Where(p => !p.IsDeleted && p.Name.Contains(searchResult.SearchTerm))
                    .ToList()
                    .AsQueryable()
                    .Where(p => p.ProblemGroup.Contest.CanBeCompeted || p.ProblemGroup.Contest.CanBePracticed)
                    .Select(SearchResultViewModel.FromProblem);

                searchResult.SearchResults.Add(SearchResultType.Problem, problemSearchResults);

                var contestSearchResults = this.contestsData
                    .GetAllVisible()
                    .Where(x => x.Name.Contains(searchResult.SearchTerm))
                    .ToList()
                    .AsQueryable()
                    .Where(x => x.CanBeCompeted || x.CanBePracticed)
                    .Select(SearchResultViewModel.FromContest);

                searchResult.SearchResults.Add(SearchResultType.Contest, contestSearchResults);

                var userSearchResults = this.Data.Users.All()
                                        .Where(x => !x.IsDeleted && x.UserName.Contains(searchResult.SearchTerm))
                                        .Select(SearchResultViewModel.FromUser);

                searchResult.SearchResults.Add(SearchResultType.User, userSearchResults);
            }

            return this.View(searchResult);
        }
    }
}