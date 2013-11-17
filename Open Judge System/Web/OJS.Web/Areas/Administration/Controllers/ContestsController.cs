namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Data;
    using OJS.Web.Areas.Administration.ViewModels.Contest;
    using OJS.Web.Controllers;

    using ModelType = OJS.Web.Areas.Administration.ViewModels.Contest.ContestAdministrationViewModel;

    public class ContestsController : KendoGridAdministrationController
    {
        private const string NoActiveContests = "Няма активни състезания";
        private const string NoFutureContests = "Няма бъдещи състезания";
        private const string NoLatestContests = "Нямa последни състезaния";

        public ContestsController(IOjsData data)
            : base(data)
        {
        }

        public override IEnumerable GetData()
        {
            return this.Data.Contests
                .All()
                .Where(x => !x.IsDeleted)
                .Select(ContestAdministrationViewModel.ViewModel);
        }

        public ActionResult Index()
        {
            this.GenerateDropDownData();
            return this.View();
        }

        [HttpPost]
        public ActionResult Create([DataSourceRequest]DataSourceRequest request, ModelType model)
        {
            return this.BaseCreate(request, model.ToEntity);
        }

        [HttpPost]
        public ActionResult Update([DataSourceRequest]DataSourceRequest request, ModelType model)
        {
            return this.BaseUpdate(request, model.ToEntity);
        }

        [HttpPost]
        public ActionResult Destroy([DataSourceRequest]DataSourceRequest request, ModelType model)
        {
            return this.BaseDestroy(request, model.ToEntity);
        }

        public ActionResult GetFutureContests([DataSourceRequest]DataSourceRequest request)
        {
            var futureContests = this.Data.Contests
                .AllFuture()
                .OrderBy(contest => contest.StartTime)
                .Take(3)
                .Select(ShortContestAdministrationViewModel.FromContest);

            if (futureContests.Count() > 0)
            {
                return this.PartialView("_QuickContestsGrid", futureContests);
            }
            else
            {
                return this.Content(NoFutureContests);
            }
        }

        public ActionResult GetActiveContests([DataSourceRequest]DataSourceRequest request)
        {
            var activeContests = this.Data.Contests
                .AllActive()
                .OrderBy(contest => contest.EndTime)
                .Take(3)
                .Select(ShortContestAdministrationViewModel.FromContest);

            if (activeContests.Count() > 0)
            {
                return this.PartialView("_QuickContestsGrid", activeContests);
            }
            else
            {
                return this.Content(NoActiveContests);
            }
        }

        public ActionResult GetLatestContests([DataSourceRequest]DataSourceRequest request)
        {
            var latestContests = this.Data.Contests
                .AllVisible()
                .OrderByDescending(contest => contest.CreatedOn)
                .Take(3)
                .Select(ShortContestAdministrationViewModel.FromContest);

            if (latestContests.Count() > 0)
            {
                return this.PartialView("_QuickContestsGrid", latestContests);
            }
            else
            {
                return this.Content(NoLatestContests);
            }
        }

        private void GenerateDropDownData()
        {
            var dropDownData = this.Data.ContestCategories
                .All()
                .ToList()
                .Select(cat => new SelectListItem
                {
                    Text = cat.Name,
                    Value = cat.Id.ToString(),
                });

            // TODO: Improve not to use ViewData
            this.ViewData["DropDownData"] = dropDownData;
        }
    }
}