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

        public ActionResult Index()
        {
            return this.View();
        }


        public ActionResult Results(string searchTerm)
        {
            var results = new SearchResultGroupViewModel(searchTerm);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var problemSearchResults = this.Data.Problems.All()
                                        .Where(x => !x.IsDeleted && x.Name.Contains(searchTerm))
                                        .Select(SearchResultViewModel.FromProblem);

                results.SearchResults.Add(SearchResultType.Problem, problemSearchResults);

                var contestSearchResults = this.Data.Contests.All()
                                        .Where(x => x.IsVisible && !x.IsDeleted && x.Name.Contains(searchTerm))
                                        .Select(SearchResultViewModel.FromContest);
                
                results.SearchResults.Add(SearchResultType.Contest, contestSearchResults);

                var userSearchResults = this.Data.Users.All()
                                        .Where(x => !x.IsDeleted && x.UserName.Contains(searchTerm))
                                        .Select(SearchResultViewModel.FromUser);

                results.SearchResults.Add(SearchResultType.User, userSearchResults);
            }

            return this.View(results);
        }
    }
}