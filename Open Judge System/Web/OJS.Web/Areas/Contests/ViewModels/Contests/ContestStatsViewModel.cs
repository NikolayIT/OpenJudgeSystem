using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OJS.Web.Areas.Contests.ViewModels.Contests
{
    public class ContestStatsViewModel
    {
        public ContestStatsViewModel()
        {
            this.StatsByProblem = new List<ContestProblemStatsViewModel>();
            this.StatsByPointsRange = new List<ContestPointsRangeViewModel>();
        }

        public int MaxResultsCount { get; set; }

        public double MaxResultsPercent { get; set; }

        public int MinResultsCount { get; set; }

        public double MinResultsPercent { get; set; }

        public double AverageResult { get; set; }

        public ICollection<ContestProblemStatsViewModel> StatsByProblem { get; set; }

        public ICollection<ContestPointsRangeViewModel> StatsByPointsRange { get; set; }
    }
}