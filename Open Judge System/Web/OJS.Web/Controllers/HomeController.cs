namespace OJS.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Web.Mvc;

    using OJS.Data;
    using OJS.Services.Data.Contests;
    using OJS.Web.Common.Extensions;
    using OJS.Web.ViewModels.Home.Index;

    public class HomeController : BaseController
    {
        private readonly IContestsDataService contestsData;

        public HomeController(IOjsData data, IContestsDataService contestsData)
            : base(data)
        {
            this.contestsData = contestsData;
        }

        public ActionResult Index()
        {
            var isAdmin = this.User.IsAdmin();
            var userId = this.UserProfile?.Id;

            var indexViewModel = new IndexViewModel
            {
                ActiveContests = this.contestsData
                    .GetAllCompetable()
                    .OrderByDescending(ac => ac.StartTime)
                    .Select(HomeContestViewModel.FromContest)
                    .ToList(),
                FutureContests = this.Data.Contests.All()
                    .Where(fc => fc.StartTime > DateTime.Now &&
                        (fc.IsVisible ||
                            isAdmin ||
                            fc.Lecturers.Any(l => l.LecturerId == userId) ||
                            fc.Category.Lecturers.Any(cl => cl.LecturerId == userId)))
                    .OrderBy(fc => fc.StartTime)
                    .Select(HomeContestViewModel.FromContest)
                    .ToList(),
                PastContests = this.contestsData.GetAllPast()
                    .OrderByDescending(pc => pc.StartTime)
                    .Select(HomeContestViewModel.FromContest)
                    .Take(5)
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