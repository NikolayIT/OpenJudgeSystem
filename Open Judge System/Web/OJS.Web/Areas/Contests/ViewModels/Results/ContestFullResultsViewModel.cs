namespace OJS.Web.Areas.Contests.ViewModels.Results
{
    using System.Collections.Generic;

    using X.PagedList;

    using OJS.Web.Areas.Contests.ViewModels.Contests;

    public class ContestFullResultsViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int CurrentPage { get; set; }

        public IEnumerable<ContestProblemSimpleViewModel> Problems { get; set; }

        public IPagedList<ParticipantFullResultViewModel> Results { get; set; }
    }
}