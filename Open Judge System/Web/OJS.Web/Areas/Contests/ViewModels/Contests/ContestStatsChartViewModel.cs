namespace OJS.Web.Areas.Contests.ViewModels.Contests
{
    public class ContestStatsChartViewModel
    {
        public double AverageResult { get; set; }

        public int Hour { get; set; }

        public int Minute { get; set; }

        public string DisplayValue
        {
            get
            {
                return string.Format("{0:00}:{1:00}", this.Hour, this.Minute);
            }
        }
    }
}