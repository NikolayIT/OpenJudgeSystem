namespace OJS.Web.Controllers
{
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

        public ActionResult Results(string searchTerm)
        {
            var results = new SearchResultGroupViewModel(searchTerm);

            var problemSearchResults = this.Data.Problems.All()
                                    .Where(x => !x.IsDeleted && x.Name.Contains(searchTerm))
                                    .Select(SearchResultViewModel.FromProblem);

            results.SearchResults.Add(SearchResultType.Problem, problemSearchResults);

            var contestSearchResults = this.Data.Contests.All()
                                    .Where(x => x.IsVisible && !x.IsDeleted && x.Name.Contains(searchTerm))
                                    .Select(SearchResultViewModel.FromContest);

            results.SearchResults.Add(SearchResultType.Contest, contestSearchResults);

            return this.View(results);
        }
    }
}