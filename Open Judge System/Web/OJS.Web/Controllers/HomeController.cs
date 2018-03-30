namespace OJS.Web.Controllers
{
    using System.Linq;
    using System.Text;
    using System.Web.Mvc;

    using OJS.Data;
    using OJS.Services.Data.Contests;
    using OJS.Web.ViewModels.Home.Index;

    public class HomeController : BaseController
    {
        private const int DefaultPastContestsToTake = 15;

        private readonly IContestsDataService contestsData;

        public HomeController(IOjsData data, IContestsDataService contestsData)
            : base(data) =>
                this.contestsData = contestsData;

        public ActionResult Index()
        {
            var indexViewModel = new IndexViewModel
            {
                ActiveContests = this.contestsData
                    .GetAllCompetable()
                    .OrderBy(ac => ac.EndTime)
                    .Select(HomeContestViewModel.FromContest)
                    .ToList(),
                PastContests = this.contestsData
                    .GetAllPast()
                    .OrderByDescending(pc => pc.EndTime)
                    .Select(HomeContestViewModel.FromContest)
                    .Take(DefaultPastContestsToTake)
                    .ToList()
            };

            return this.View(indexViewModel);
        }

        /// <summary>
        /// Gets the robots.txt file.
        /// </summary>
        /// <returns>Returns a robots.txt file.</returns>
        [HttpGet]
        [OutputCache(Duration = 3600)]
        public FileResult RobotsTxt()
        {
            var robotsTxtContent = new StringBuilder();
            robotsTxtContent.AppendLine("User-Agent: *");
            robotsTxtContent.AppendLine("Allow: /");

            return this.File(Encoding.ASCII.GetBytes(robotsTxtContent.ToString()), "text/plain");
        }
    }
}