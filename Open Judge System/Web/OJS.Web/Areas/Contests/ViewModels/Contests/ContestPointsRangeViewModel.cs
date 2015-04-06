namespace OJS.Web.Areas.Contests.ViewModels.Contests
{
    public class ContestPointsRangeViewModel
    {
        public int PointsFrom { get; set; }

        public int PointsTo { get; set; }

        public int Participants { get; set; }

        public double PercentOfAllParticipants { get; set; }
    }
}