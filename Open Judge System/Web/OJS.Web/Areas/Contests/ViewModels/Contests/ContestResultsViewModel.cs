namespace OJS.Web.Areas.Contests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using OJS.Data.Models;

    public class ContestResultsViewModel
    {
        public string Name { get; set; }

        public IEnumerable<ContestProblemViewModel> Problems { get; set; }

        public IEnumerable<ParticipantResultViewModel> Results { get; set; }
    }
}