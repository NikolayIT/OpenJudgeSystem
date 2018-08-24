namespace OJS.Web.Areas.Contests.ViewModels.Contests
{
    using System.Collections.Generic;

    public class ContestStatsViewModel
    {
        public int MaxResultsCount { get; set; }

        public double MaxResultsPercent { get; set; }

        public int MinResultsCount { get; set; }

        public double MinResultsPercent { get; set; }

        public double AverageResult { get; set; }

        public bool IsGroupedByProblemGroup { get; set; }

        public ICollection<ContestProblemStatsViewModel> StatsByProblem { get; set; } =
            new List<ContestProblemStatsViewModel>();

        public ICollection<ContestPointsRangeViewModel> StatsByPointsRange { get; set; } =
            new List<ContestPointsRangeViewModel>();
    }
}