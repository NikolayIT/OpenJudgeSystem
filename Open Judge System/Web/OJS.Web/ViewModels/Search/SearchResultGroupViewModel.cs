namespace OJS.Web.ViewModels.Search
{
    using System.Collections.Generic;

    using OJS.Web.Common;

    public class SearchResultGroupViewModel
    {
        public SearchResultGroupViewModel(string searchTerm)
        {
            this.SearchTerm = searchTerm;
            this.SearchResults = new Dictionary<SearchResultType, IEnumerable<SearchResultViewModel>>();
        }

        public string SearchTerm { get; set; }

        public IDictionary<SearchResultType, IEnumerable<SearchResultViewModel>> SearchResults { get; set; }
    }
}