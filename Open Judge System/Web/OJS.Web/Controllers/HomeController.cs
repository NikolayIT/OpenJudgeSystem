namespace OJS.Web.Controllers
{
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Data;
    using OJS.Web.ViewModels.Home.Index;

    public class HomeController : BaseController
    {
        public HomeController(IOjsData data)
            : base(data)
        {
        }

        public ActionResult Index()
        {
            var indexViewModel = new IndexViewModel();
            indexViewModel.ActiveContests =
                this.Data.Contests.AllActive()
                    .OrderByDescending(x => x.StartTime)
                    .Select(IndexContestViewModel.FromContest)
                    .ToList();

            indexViewModel.FutureContests =
                this.Data.Contests.AllFuture()
                    .OrderByDescending(x => x.StartTime)
                    .Select(IndexContestViewModel.FromContest)
                    .ToList();

            indexViewModel.PastContests =
                this.Data.Contests.AllPast()
                    .OrderByDescending(x => x.StartTime)
                    .Select(IndexContestViewModel.FromContest)
                    .Take(5)
                    .ToList();

            return this.View(indexViewModel);
        }
    }
}