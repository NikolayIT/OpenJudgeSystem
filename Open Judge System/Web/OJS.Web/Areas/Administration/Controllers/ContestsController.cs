namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.UI;

    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Data;
    using OJS.Web.Areas.Administration.ViewModels.Contest;
    using OJS.Web.Areas.Administration.ViewModels.SubmissionType;
    using OJS.Web.Controllers;

    using Resource = Resources.Areas.Administration.Contests.ContestsControllers;
    using ShortViewModelType = OJS.Web.Areas.Administration.ViewModels.Contest.ShortContestAdministrationViewModel;
    using ViewModelType = OJS.Web.Areas.Administration.ViewModels.Contest.ContestAdministrationViewModel;

    public class ContestsController : KendoGridAdministrationController
    {
        public ContestsController(IOjsData data)
            : base(data)
        {
        }

        public override IEnumerable GetData()
        {
            return this.Data.Contests
                .All()
                .Where(x => !x.IsDeleted)
                .Select(ViewModelType.ViewModel);
        }

        public override object GetById(object id)
        {
            return this.Data.Contests
                .All()
                .FirstOrDefault(o => o.Id == (int)id);
        }

        public ActionResult Index()
        {
            return this.View();
        }

        [HttpGet]
        public ActionResult Create()
        {
            var newContest = new ViewModelType
            {
                SubmisstionTypes = this.Data.SubmissionTypes.All().Select(SubmissionTypeViewModel.ViewModel).ToList()
            };

            return this.View(newContest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ContestAdministrationViewModel model)
        {
            if (!this.IsValidContest(model))
            {
                return this.View(model);
            }

            if (model != null && this.ModelState.IsValid)
            {
                var contest = model.GetEntityModel();

                model.SubmisstionTypes.ForEach(s =>
                    {
                        if (s.IsChecked)
                        {
                            var submission = this.Data.SubmissionTypes.All().FirstOrDefault(t => t.Id == s.Id);
                            contest.SubmissionTypes.Add(submission);
                        }
                    });

                this.Data.Contests.Add(contest);
                this.Data.SaveChanges();

                this.TempData.Add(GlobalConstants.InfoMessage, Resource.Contest_added);
                return this.RedirectToAction(GlobalConstants.Index);
            }

            return this.View(model);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var contest = this.Data.Contests
                .All()
                .Where(con => con.Id == id)
                .Select(ContestAdministrationViewModel.ViewModel)
                .FirstOrDefault();

            if (contest == null)
            {
                this.TempData.Add(GlobalConstants.DangerMessage, Resource.Contest_not_found);
                return this.RedirectToAction(GlobalConstants.Index);
            }

            this.Data.SubmissionTypes.All()
                .Select(SubmissionTypeViewModel.ViewModel)
                .ForEach(SubmissionTypeViewModel.ApplySelectedTo(contest));

            return this.View(contest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ContestAdministrationViewModel model)
        {
            if (!this.IsValidContest(model))
            {
                return this.View(model);
            }

            if (model != null && this.ModelState.IsValid)
            {
                var contest = this.Data.Contests.All().FirstOrDefault(c => c.Id == model.Id);

                if (contest == null)
                {
                    this.TempData.Add(GlobalConstants.DangerMessage, Resource.Contest_not_found);
                    return this.RedirectToAction(GlobalConstants.Index);
                }

                contest = model.GetEntityModel(contest);
                contest.SubmissionTypes.Clear();

                model.SubmisstionTypes.ForEach(s =>
                {
                    if (s.IsChecked)
                    {
                        var submission = this.Data.SubmissionTypes.All().FirstOrDefault(t => t.Id == s.Id);
                        contest.SubmissionTypes.Add(submission);
                    }
                });

                this.Data.Contests.Update(contest);
                this.Data.SaveChanges();

                this.TempData.Add(GlobalConstants.InfoMessage, Resource.Contest_edited);
                return this.RedirectToAction(GlobalConstants.Index);
            }

            return this.View(model);
        }

        [HttpPost]
        public ActionResult Destroy([DataSourceRequest]DataSourceRequest request, ContestAdministrationViewModel model)
        {
            this.BaseDestroy(model.Id);
            return this.GridOperation(request, model);
        }

        public ActionResult GetFutureContests([DataSourceRequest]DataSourceRequest request)
        {
            var futureContests = this.Data.Contests
                .AllFuture()
                .OrderBy(contest => contest.StartTime)
                .Take(3)
                .Select(ShortViewModelType.FromContest);

            if (!futureContests.Any())
            {
                return this.Content(Resource.No_future_contests);
            }

            return this.PartialView(GlobalConstants.QuickContestsGrid, futureContests);
        }

        public ActionResult GetActiveContests([DataSourceRequest]DataSourceRequest request)
        {
            var activeContests = this.Data.Contests
                .AllActive()
                .OrderBy(contest => contest.EndTime)
                .Take(3)
                .Select(ShortViewModelType.FromContest);

            if (!activeContests.Any())
            {
                return this.Content(Resource.No_active_contests);
            }

            return this.PartialView(GlobalConstants.QuickContestsGrid, activeContests);
        }

        public ActionResult GetLatestContests([DataSourceRequest]DataSourceRequest request)
        {
            var latestContests = this.Data.Contests
                .AllVisible()
                .OrderByDescending(contest => contest.CreatedOn)
                .Take(3)
                .Select(ShortViewModelType.FromContest);

            if (!latestContests.Any())
            {
                return this.Content(Resource.No_latest_contests);
            }

            return this.PartialView(GlobalConstants.QuickContestsGrid, latestContests);
        }

        public JsonResult GetCategories()
        {
            var dropDownData = this.Data.ContestCategories
                .All()
                .ToList()
                .Select(cat => new SelectListItem
                {
                    Text = cat.Name,
                    Value = cat.Id.ToString(CultureInfo.InvariantCulture)
                });

            return this.Json(dropDownData, JsonRequestBehavior.AllowGet);
        }

        private bool IsValidContest(ContestAdministrationViewModel model)
        {
            bool isValid = true;

            if (model.StartTime >= model.EndTime)
            {
                this.ModelState.AddModelError(GlobalConstants.DateTimeError, Resource.Contest_start_date_before_end);
                isValid = false;
            }

            if (model.PracticeStartTime >= model.PracticeEndTime)
            {
                this.ModelState.AddModelError(GlobalConstants.DateTimeError, Resource.Practice_start_date_before_end);
                isValid = false;
            }

            if (model.SubmisstionTypes == null || !model.SubmisstionTypes.Any(s => s.IsChecked))
            {
                this.ModelState.AddModelError("SelectedSubmissionTypes", Resource.Select_one_submission_type);
                isValid = false;
            }

            return isValid;
        }
    }
}
