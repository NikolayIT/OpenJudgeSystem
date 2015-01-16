namespace OJS.Web.Areas.Administration.Controllers
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.UI;

    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Areas.Administration.InputModels.Contests;
    using OJS.Web.Areas.Administration.ViewModels.Contest;
    using OJS.Web.Areas.Administration.ViewModels.SubmissionType;
    using OJS.Web.Common.Extensions;
    using OJS.Web.ViewModels.Common;

    using ShortViewModelType = OJS.Web.Areas.Administration.ViewModels.Contest.ShortContestAdministrationViewModel;
    using ViewModelType = OJS.Web.Areas.Administration.ViewModels.Contest.ContestAdministrationViewModel;

    public class ContestsController : LecturerBaseGridController
    {
        private const string NoActiveContests = "Няма активни състезания";
        private const string NoFutureContests = "Няма бъдещи състезания";
        private const string NoLatestContests = "Нямa последни състезaния";
        private const int StartTimeDelayInSeconds = 10;
        private const int LabDurationInSeconds = 30 * 60;

        public ContestsController(IOjsData data)
            : base(data)
        {
        }

        public override IEnumerable GetData()
        {
            var allContest = this.Data.Contests.All();

            if (!this.User.IsAdmin() && this.User.IsLecturer())
            {
                allContest = allContest.Where(x => x.Lecturers.Any(y => y.LecturerId == this.UserProfile.Id));
            }

            return allContest.Select(ViewModelType.ViewModel);
        }

        public override object GetById(object id)
        {
            return this.Data.Contests.All().FirstOrDefault(o => o.Id == (int)id);
        }

        public ActionResult Index()
        {
            this.PrepareViewBagData();
            return this.View();
        }

        [HttpGet]
        public ActionResult Create()
        {
            if (!this.User.IsAdmin())
            {
                this.TempData[GlobalConstants.DangerMessage] = "Нямате привилегиите за това действие";
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
            }

            var newContest = new ViewModelType
            {
                SubmissionTypes = this.Data.SubmissionTypes.All().Select(SubmissionTypeViewModel.ViewModel).ToList()
            };

            this.PrepareViewBagData();
            return this.View(newContest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ViewModelType model)
        {
            if (!this.User.IsAdmin())
            {
                this.TempData[GlobalConstants.DangerMessage] = "Нямате привилегиите за това действие";
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
            }

            if (!this.IsValidContest(model))
            {
                return this.View(model);
            }

            if (model != null && this.ModelState.IsValid)
            {
                var contest = model.GetEntityModel();

                model.SubmissionTypes.ForEach(s =>
                {
                    if (s.IsChecked)
                    {
                        var submission = this.Data.SubmissionTypes.All().FirstOrDefault(t => t.Id == s.Id);
                        contest.SubmissionTypes.Add(submission);
                    }
                });

                this.AddIpsToContest(contest, model.AllowedIps);

                this.Data.Contests.Add(contest);
                this.Data.SaveChanges();

                this.TempData.Add(GlobalConstants.InfoMessage, "Състезанието беше добавено успешно");
                return this.RedirectToAction(GlobalConstants.Index);
            }

            this.PrepareViewBagData();
            return this.View(model);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            if (!this.CheckIfUserHasContestPermissions(id))
            {
                this.TempData[GlobalConstants.DangerMessage] = "Нямате привилегиите за това действие";
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
            }

            var contest = this.Data.Contests
                .All()
                .Where(con => con.Id == id)
                .Select(ContestAdministrationViewModel.ViewModel)
                .FirstOrDefault();

            if (contest == null)
            {
                TempData.Add(GlobalConstants.DangerMessage, "Състезанието не е намерено");
                return this.RedirectToAction(GlobalConstants.Index);
            }

            this.Data.SubmissionTypes.All()
                .Select(SubmissionTypeViewModel.ViewModel)
                .ForEach(SubmissionTypeViewModel.ApplySelectedTo(contest));

            this.PrepareViewBagData();
            return this.View(contest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ViewModelType model)
        {
            if (model.Id == null || !this.CheckIfUserHasContestPermissions(model.Id.Value))
            {
                this.TempData[GlobalConstants.DangerMessage] = "Нямате привилегиите за това действие";
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
            }

            if (!this.IsValidContest(model))
            {
                return this.View(model);
            }

            if (this.ModelState.IsValid)
            {
                var contest = this.Data.Contests.All().FirstOrDefault(c => c.Id == model.Id);

                if (contest == null)
                {
                    this.TempData.Add(GlobalConstants.DangerMessage, "Състезанието не е намерено");
                    return this.RedirectToAction(GlobalConstants.Index);
                }

                contest = model.GetEntityModel(contest);
                contest.SubmissionTypes.Clear();

                model.SubmissionTypes.ForEach(s =>
                {
                    if (s.IsChecked)
                    {
                        var submission = this.Data.SubmissionTypes.All().FirstOrDefault(t => t.Id == s.Id);
                        contest.SubmissionTypes.Add(submission);
                    }
                });

                contest.AllowedIps.Clear();
                this.AddIpsToContest(contest, model.AllowedIps);

                this.Data.Contests.Update(contest);
                this.Data.SaveChanges();

                this.TempData.Add(GlobalConstants.InfoMessage, "Състезанието беше променено успешно");
                return this.RedirectToAction(GlobalConstants.Index);
            }

            this.PrepareViewBagData();
            return this.View(model);
        }

        [HttpPost]
        public ActionResult Destroy([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            if (model.Id == null || !this.CheckIfUserHasContestPermissions(model.Id.Value))
            {
                this.TempData[GlobalConstants.DangerMessage] = "Нямате привилегиите за това действие";
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
            }

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
                return this.Content(NoFutureContests);
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
                return this.Content(NoActiveContests);
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
                return this.Content(NoLatestContests);
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
                    Value = cat.Id.ToString(CultureInfo.InvariantCulture),
                });

            return this.Json(dropDownData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult StartAsLab(LabStartInputModel inputModel)
        {
            if (!this.CheckIfUserHasContestPermissions(inputModel.ContestCreateId))
            {
                this.TempData[GlobalConstants.DangerMessage] = "Нямате привилегиите за това действие";
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
            }

            var contest = this.Data.Contests.GetById(inputModel.ContestCreateId);

            if (contest != null)
            {
                contest.StartTime = DateTime.Now.AddSeconds(StartTimeDelayInSeconds);
                contest.EndTime = DateTime.Now.AddSeconds(StartTimeDelayInSeconds + LabDurationInSeconds);
                contest.IsVisible = true;
                this.Data.SaveChanges();
            }

            return new EmptyResult();
        }

        [HttpGet]
        public ActionResult ReadIpsContaining(string value)
        {
            var ipEntries = this.Data.Ips.All();

            if (!string.IsNullOrWhiteSpace(value))
            {
                var trimmedValue = value.Trim();
                ipEntries = ipEntries.Where(x => x.Value.Contains(trimmedValue));
            }

            var ips = ipEntries.Take(10).Select(IpAdministrationViewModel.ViewModel);
            return this.Json(ips, JsonRequestBehavior.AllowGet);
        }

        private void PrepareViewBagData()
        {
            this.ViewBag.TypeData = DropdownViewModel.GetEnumValues<ContestType>();
        }

        private bool IsValidContest(ViewModelType model)
        {
            bool isValid = true;

            if (model.StartTime >= model.EndTime)
            {
                this.ModelState.AddModelError(GlobalConstants.DateTimeError, "Началната дата на състезанието не може да бъде след крайната дата на състезанието");
                isValid = false;
            }

            if (model.PracticeStartTime >= model.PracticeEndTime)
            {
                this.ModelState.AddModelError(GlobalConstants.DateTimeError, "Началната дата за упражнения не може да бъде след крайната дата за упражнения");
                isValid = false;
            }

            if (model.SubmissionTypes == null || !model.SubmissionTypes.Any(s => s.IsChecked))
            {
                this.ModelState.AddModelError("SelectedSubmissionTypes", "Изберете поне един вид решение!");
                isValid = false;
            }

            return isValid;
        }

        private void AddIpsToContest(Contest contest, string mergedIps)
        {
            if (!string.IsNullOrWhiteSpace(mergedIps))
            {
                var ipValues = mergedIps.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var ipValue in ipValues)
                {
                    var ip = this.Data.Ips.All().FirstOrDefault(x => x.Value == ipValue);
                    if (ip == null)
                    {
                        ip = new Ip { Value = ipValue };
                        this.Data.Ips.Add(ip);
                    }

                    contest.AllowedIps.Add(new ContestIp { Ip = ip, IsOriginallyAllowed = true });
                }
            }
        }
    }
}
