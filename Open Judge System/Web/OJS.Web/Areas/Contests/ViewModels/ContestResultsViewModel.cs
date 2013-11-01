namespace OJS.Web.Areas.Contests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web;

    using OJS.Data.Models;

    public class ContestResultsViewModel
    {
        public ContestResultsViewModel()
        {
        }

        public ContestResultsViewModel(Contest contest, bool official)
        {
            Name = contest.Name;
            Results = contest.Participants
                                        .Where(x => x.IsOfficial == official)
                                        .AsQueryable()
                                        .Select(ParticipantResultsViewModel.FromParticipant)
                                        .OrderByDescending(pr => pr.ProblemResults.Sum(res => res.Result));
            Problems = contest.Problems
                                    .AsQueryable()
                                    .Select(ContestProblemViewModel.FromProblem)
                                    .OrderBy(x => x.Name);
        }

        public string Name { get; set; }

        public IEnumerable<ContestProblemViewModel> Problems { get; set; }

        public IEnumerable<ParticipantResultsViewModel> Results;
    }
}