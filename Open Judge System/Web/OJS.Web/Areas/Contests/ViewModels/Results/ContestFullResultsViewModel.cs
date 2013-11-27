using System.Collections.Generic;

namespace OJS.Web.Areas.Contests.ViewModels.Results
{
    // TODO: Refactor to reuse same logic with ContestResultsViewModel
    public class ContestFullResultsViewModel
    {
        public string Name { get; set; }

        public IEnumerable<ContestProblemViewModel> Problems { get; set; }

        public IEnumerable<ParticipantFullResultViewModel> Results { get; set; }
    }
}