namespace OJS.Web.Areas.Contests.ViewModels.Results
{
    using System.Collections.Generic;

    // TODO: Refactor to reuse same logic with ContestResultsViewModel
    public class ContestFullResultsViewModel
    {
        public string Name { get; set; }

        public IEnumerable<ContestProblemViewModel> Problems { get; set; }

        public IEnumerable<ParticipantFullResultViewModel> Results { get; set; }
    }
}