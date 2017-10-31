﻿namespace OJS.Web.Areas.Contests.ViewModels.Results
{
    using System.Collections.Generic;

    using OJS.Web.Areas.Contests.ViewModels.Contests;

    // TODO: Refactor to reuse same logic with ContestResultsViewModel
    public class ContestFullResultsViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int CurrentPage { get; set; }

        public IEnumerable<ContestProblemSimpleViewModel> Problems { get; set; }

        public IEnumerable<ParticipantFullResultViewModel> Results { get; set; }
    }
}