namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Web.Mvc;

    using Ionic.Zip;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Common.Extensions;
    using OJS.Data;
    using OJS.Web.Areas.Administration.ViewModels.Contest;
    using OJS.Web.Areas.Administration.ViewModels.ContestQuestion;
    using OJS.Web.Areas.Administration.ViewModels.SubmissionType;
    using OJS.Web.Common;
    using OJS.Web.Controllers;

    using ViewModelType = OJS.Web.Areas.Administration.ViewModels.Contest.ContestAdministrationViewModel;

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
            var newContest = new ViewModelType();
            newContest.SubmisstionTypes = this.Data.SubmissionTypes.All().Select(SubmissionTypeViewModel.ViewModel).ToList();

            return this.View(newContest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ViewModelType model)
        {
            if (model.StartTime >= model.EndTime)
            {
                ModelState.AddModelError("DateTimeError", "Началната дата на състезанието не може да бъде след крайната дата на състезанието");
                return this.View(model);
            }

            if (model.PracticeStartTime >= model.PracticeEndTime)
            {
                ModelState.AddModelError("DateTimeError", "Началната дата за упражнения не може да бъде след крайната дата за упражнения");
                return this.View(model);
            }

            if (model.SelectedSubmissionTypes == null || !model.SelectedSubmissionTypes.Any())
            {
                ModelState.AddModelError("SelectedSubmissionTypes", "Изберете поне един вид решение!");
                return this.View(model);
            }

            if (model != null && ModelState.IsValid)
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

                TempData.Add("InfoMessage", "Състезанието беше добавено успешно");
                return this.RedirectToAction("Index");
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
                TempData.Add("DangerMessage", "Състезанието не е намерено");
                return this.RedirectToAction("Index");
            }

            this.Data.SubmissionTypes.All()
                .Select(SubmissionTypeViewModel.ViewModel)
                .ForEach(SubmissionTypeViewModel.ApplySelectedTo(contest));

            return this.View(contest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ViewModelType model)
        {
            if (model.StartTime >= model.EndTime)
            {
                ModelState.AddModelError("DateTimeError", "Началната дата на състезанието не може да бъде след крайната дата на състезанието");
                return this.View(model);
            }

            if (model.PracticeStartTime >= model.PracticeEndTime)
            {
                ModelState.AddModelError("DateTimeError", "Началната дата за упражнения не може да бъде след крайната дата за упражнения");
                return this.View(model);
            }

            if (model != null && ModelState.IsValid)
            {
                var contest = this.Data.Contests.All().FirstOrDefault(c => c.Id == model.Id);

                if (contest == null)
                {
                    TempData.Add("DangerMessage", "Състезанието не е намерено");
                    return this.RedirectToAction("Index");
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

                TempData.Add("InfoMessage", "Състезанието беше променено успешно");
                return this.RedirectToAction("Index");
            }

            return this.View(model);
        }

        [HttpPost]
        public ActionResult Destroy([DataSourceRequest]DataSourceRequest request, ViewModelType model)
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
                .Select(ShortContestAdministrationViewModel.FromContest);

            if (!futureContests.Any())
            {
                return this.Content(NoFutureContests);
            }

            return this.PartialView("_QuickContestsGrid", futureContests);
        }

        public ActionResult GetActiveContests([DataSourceRequest]DataSourceRequest request)
        {
            var activeContests = this.Data.Contests
                .AllActive()
                .OrderBy(contest => contest.EndTime)
                .Take(3)
                .Select(ShortContestAdministrationViewModel.FromContest);

            if (!activeContests.Any())
            {
                return this.Content(NoActiveContests);
            }

            return this.PartialView("_QuickContestsGrid", activeContests);
        }

        public ActionResult GetLatestContests([DataSourceRequest]DataSourceRequest request)
        {
            var latestContests = this.Data.Contests
                .AllVisible()
                .OrderByDescending(contest => contest.CreatedOn)
                .Take(3)
                .Select(ShortContestAdministrationViewModel.FromContest);

            if (!latestContests.Any())
            {
                return this.Content(NoLatestContests);
            }

            return this.PartialView("_QuickContestsGrid", latestContests);
        }

        public JsonResult GetCategories()
        {
            var dropDownData = this.Data.ContestCategories
                .All()
                .ToList()
                .Select(cat => new SelectListItem
                {
                    Text = cat.Name,
                    Value = cat.Id.ToString(CultureInfo.InvariantCulture),
                });

            return this.Json(dropDownData, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult QuestionsInContest([DataSourceRequest]DataSourceRequest request, int id)
        {
            var questions = this.Data.ContestQuestions
                .All()
                .Where(q => q.ContestId == id)
                .Select(ContestQuestionViewModel.ViewModel);

            return this.Json(questions.ToDataSourceResult(request));
        }

        [HttpPost]
        public JsonResult AddQuestionToContest([DataSourceRequest]DataSourceRequest request, ContestQuestionViewModel model, int id)
        {
            var contest = this.Data.Contests.All().FirstOrDefault(c => c.Id == id);
            var question = model.GetEntityModel();

            contest.Questions.Add(question);
            this.Data.SaveChanges();

            return this.Json(new[] { model }.ToDataSourceResult(request));
        }
    }
}
