namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Data;
    using OJS.Web.Areas.Administration.ViewModels;
    using OJS.Web.Controllers;

    using ModelType = OJS.Data.Models.Contest;

    public class ContestsController : KendoGridAdministrationController
    {
        private const string NO_ACTIVE_CONTESTS = "Няма активни състезания";
        private const string NO_FUTURE_CONTESTS = "Няма бъдещи състезания";
        private const string NO_LATEST_CONTESTS = "Нямa последни състезaния";

        public ContestsController(IOjsData data)
            : base(data)
        {
        }

        public override IEnumerable GetData()
        {
            return this.Data.Contests.All().Where(x => !x.IsDeleted).Select(ContestViewModel.FromContest);
        }

        public ActionResult Index()
        {
            return this.View();
        }

        [HttpPost]
        public ActionResult Create([DataSourceRequest]DataSourceRequest request, ModelType model)
        {
            return this.BaseCreate(request, model);
        }

        [HttpPost]
        public ActionResult Update([DataSourceRequest]DataSourceRequest request, ModelType model)
        {
            return this.BaseUpdate(request, model);
        }

        [HttpPost]
        public ActionResult Destroy([DataSourceRequest]DataSourceRequest request, ModelType model)
        {
            return this.BaseDestroy(request, model);
        }

        public ActionResult GetFutureContests([DataSourceRequest]DataSourceRequest request)
        {
            var futureContests = this.Data.Contests
                .AllFuture()
                .OrderBy(contest => contest.StartTime)
                .Take(3)
                .Select(ShortContestViewModel.FromContest);

            if (futureContests.Count() > 0)
            {
                return this.PartialView("_QuickContestsGrid", futureContests);
            }
            else
            {
                return this.Content(NO_FUTURE_CONTESTS);
            }
        }

        public ActionResult GetActiveContests([DataSourceRequest]DataSourceRequest request)
        {
            var activeContests = this.Data.Contests
                .AllActive()
                .OrderBy(contest => contest.EndTime)
                .Take(3)
                .Select(ShortContestViewModel.FromContest);

            if (activeContests.Count() > 0)
            {
                return this.PartialView("_QuickContestsGrid", activeContests);
            }
            else
            {
                return this.Content(NO_ACTIVE_CONTESTS);
            }
        }

        public ActionResult GetLatestContests([DataSourceRequest]DataSourceRequest request)
        {
            var latestContests = this.Data.Contests
                .AllVisible()
                .OrderByDescending(contest => contest.CreatedOn)
                .Take(3)
                .Select(ShortContestViewModel.FromContest);

            if (latestContests.Count() > 0)
            {
                return this.PartialView("_QuickContestsGrid", latestContests);
            }
            else
            {
                return this.Content(NO_LATEST_CONTESTS);
            }
        }
    }
}