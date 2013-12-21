namespace OJS.Web.Controllers
{
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Data;
    using OJS.Web.Common;
    using OJS.Web.ViewModels.Search;

    public class SearchController : BaseController
    {
        public SearchController(IOjsData data)
            : base(data)
        {
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
                var problemSearchResults = this.Data.Problems.All().Include(x => x.Contest)
                                        .Where(x => !x.IsDeleted && x.Name.Contains(searchResult.SearchTerm))
                                        .ToList()
                                        .AsQueryable()
                                        .Where(x => x.Contest.CanBeCompeted || x.Contest.CanBePracticed)
                                        .Select(SearchResultViewModel.FromProblem);

                searchResult.SearchResults.Add(SearchResultType.Problem, problemSearchResults);

                var contestSearchResults = this.Data.Contests.All()
                                        .Where(x => x.IsVisible && !x.IsDeleted && x.Name.Contains(searchResult.SearchTerm))
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