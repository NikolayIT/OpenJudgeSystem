namespace OJS.Web.Areas.Administration.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.UI;

    using OJS.Common;
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Areas.Administration.InputModels.Contests;
    using OJS.Web.Areas.Administration.ViewModels.Contest;
    using OJS.Web.Areas.Contests.Models;
    using OJS.Web.Common.Extensions;
    using OJS.Web.ViewModels.Common;

    using Resource = Resources.Areas.Administration.Contests.ContestsControllers;
    using ShortViewModelType = OJS.Web.Areas.Administration.ViewModels.Contest.ShortContestAdministrationViewModel;
    using ViewModelType = OJS.Web.Areas.Administration.ViewModels.Contest.ContestAdministrationViewModel;

    public class ContestsController : LecturerBaseGridController
    {
        private const int StartTimeDelayInSeconds = 10;
        private const int LabDurationInSeconds = 30 * 60;

        public ContestsController(IOjsData data)
            : base(data)
        {
        }

        public override IEnumerable GetData()
        {
            var allContests = this.Data.Contests.All();

            if (!this.User.IsAdmin() && this.User.IsLecturer())
            {
                allContests = allContests.Where(x =>
                    x.Lecturers.Any(y => y.LecturerId == this.UserProfile.Id) ||
                    x.Category.Lecturers.Any(cl => cl.LecturerId == this.UserProfile.Id));
            }

            return allContests.Select(ViewModelType.ViewModel);
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
            this.PrepareViewBagData();
            return this.View(new ViewModelType());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ContestAdministrationViewModel model)
        {
            if (model?.CategoryId == null || !this.CheckIfUserHasContestCategoryPermissions(model.CategoryId.Value))
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
                var contest = model.GetEntityModel();

                this.AddIpsToContest(contest, model.AllowedIps);

                this.Data.Contests.Add(contest);
                this.Data.SaveChanges();

                this.TempData.Add(GlobalConstants.InfoMessage, Resource.Contest_added);
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
                this.TempData.Add(GlobalConstants.DangerMessage, Resource.Contest_not_found);
                return this.RedirectToAction(GlobalConstants.Index);
            }

            this.PrepareViewBagData();
            return this.View(contest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ContestAdministrationViewModel model)
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
                    this.TempData.Add(GlobalConstants.DangerMessage, Resource.Contest_not_found);
                    return this.RedirectToAction(GlobalConstants.Index);
                }

                contest = model.GetEntityModel(contest);

                contest.AllowedIps.Clear();
                this.AddIpsToContest(contest, model.AllowedIps);

                this.Data.Contests.Update(contest);
                this.Data.SaveChanges();

                this.TempData.Add(GlobalConstants.InfoMessage, Resource.Contest_edited);
                return this.RedirectToAction(GlobalConstants.Index);
            }

            this.PrepareViewBagData();
            return this.View(model);
        }

        [HttpPost]
        public ActionResult Destroy([DataSourceRequest]DataSourceRequest request, ContestAdministrationViewModel model)
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
            var categories = this.Data.ContestCategories.All();

            if (!this.User.IsAdmin() && this.User.IsLecturer())
            {
                categories = categories.Where(c =>
                    c.Lecturers.Any(l => l.LecturerId == this.UserProfile.Id) ||
                    c.Contests.Any(cc => !cc.IsDeleted && cc.Lecturers.Any(l => l.LecturerId == this.UserProfile.Id)));
            }

            var dropDownData = categories
                .ToList()
                .Select(cat => new SelectListItem
                {
                    Text = cat.Name,
                    Value = cat.Id.ToString(CultureInfo.InvariantCulture)
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

        public void MoveSubmission(int categoryId)
        {
            ContestCategory category = this.Data.ContestCategories.GetById(categoryId);

            foreach (var categoryContest in category.Contests)
            {
                var competeOnlyParticipants = categoryContest
                .Participants
                .GroupBy(p => p.UserId)
                .Where(g => g.Count() == 1 && g.All(p => p.IsOfficial))
                .SelectMany(gr => gr.ToList());

                foreach (Participant participant in competeOnlyParticipants)
                {
                    foreach (var participantScore in participant.Scores)
                    {
                        participantScore.IsOfficial = false;
                    }

                    participant.IsOfficial = false;
                }

                var competeAndPracticeParticipants = categoryContest
                    .Participants
                    .GroupBy(p => p.UserId)
                    .Where(g => g.Count() == 2)
                    .ToDictionary(grp => grp.Key, grp => grp.OrderBy(p => p.IsOfficial).ToList());

                foreach (KeyValuePair<string, List<Participant>> competeAndPracticeParticipant in competeAndPracticeParticipants)
                {
                    var unofficialParticipant = competeAndPracticeParticipants[competeAndPracticeParticipant.Key][0];
                    var officialParticipant = competeAndPracticeParticipants[competeAndPracticeParticipant.Key][1];

                    foreach (var officialParticipantSubmission in officialParticipant.Submissions)
                    {
                        officialParticipantSubmission.Participant = unofficialParticipant;
                    }

                    foreach (var officialParticipantScore in officialParticipant.Scores.ToList())
                    {
                        var unofficialParticipantScore = unofficialParticipant
                            .Scores
                            .FirstOrDefault(s => s.ProblemId == officialParticipantScore.ProblemId);

                        officialParticipantScore.IsOfficial = false;

                        officialParticipantScore.Participant = unofficialParticipant;

                        if (unofficialParticipantScore != null)
                        {
                            if (unofficialParticipantScore.Points == officialParticipantScore.Points)
                            {
                                unofficialParticipantScore = unofficialParticipantScore.Id > officialParticipantScore.Id
                                                             ? unofficialParticipantScore
                                                             : officialParticipantScore;
                            }
                            else
                            {
                                unofficialParticipantScore = unofficialParticipantScore.Points >
                                                             officialParticipantScore.Points
                                                             ? unofficialParticipantScore
                                                             : officialParticipantScore;
                            }
                        }
                    }

                    this.Data.Participants.Delete(officialParticipant);
                    this.Data.SaveChanges();
                }
            }
        }

        private void PrepareViewBagData()
        {
            this.ViewBag.TypeData = DropdownViewModel.GetEnumValues<ContestType>();
            this.ViewBag.SubmissionExportTypes = DropdownViewModel.GetEnumValues<SubmissionExportType>();
        }

        private bool IsValidContest(ViewModelType model)
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
