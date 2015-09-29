namespace OJS.Web.ViewModels.Search
{
    using System.Collections.Generic;

    using OJS.Common;
    using OJS.Web.Common;

    public class SearchResultGroupViewModel
    {
        private const int MinimumTermLength = GlobalConstants.MinimumSearchTermLength;

        private string searchTerm;

        public SearchResultGroupViewModel(string searchTerm)
        {
            this.SearchTerm = searchTerm;
            this.SearchResults = new Dictionary<SearchResultType, IEnumerable<SearchResultViewModel>>();
        }

        public string SearchTerm
        {
            get
            {
                return this.searchTerm;
            }

            set
            {
                if (value != null)
                {
                    value = value.Trim();
                }

                this.searchTerm = value;
            }
        }

        public bool IsSearchTermValid => !string.IsNullOrWhiteSpace(this.searchTerm) && this.searchTerm.Length >= MinimumTermLength;

        public int MinimumSearchTermLength => MinimumTermLength;

        public IDictionary<SearchResultType, IEnumerable<SearchResultViewModel>> SearchResults { get; set; }
    }
}