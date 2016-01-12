namespace OJS.Web.Areas.Contests.ViewModels.Contests
{
    public class ContestStatsChartViewModel
    {
        public double AverageResult { get; set; }

        public int Hour { get; set; }

        public int Minute { get; set; }

        public string DisplayValue => $"{this.Hour:00}:{this.Minute:00}";
    }
}
