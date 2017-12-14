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
    using OJS.Services.Data.Contests;
    using OJS.Services.Data.ParticipantScores;
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

        private readonly IParticipantScoresDataService participantScoresData;
        private readonly IContestsDataService contestsData;

        public ContestsController(
            IOjsData data,
            IParticipantScoresDataService participantScoresData,
            IContestsDataService contestsData)
                : base(data)
        {
            this.participantScoresData = participantScoresData;
            this.contestsData = contestsData;
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
                this.TempData[GlobalConstants.DangerMessage] = GlobalConstants.NoPrivilegesMessage;
                return this.RedirectToAction<ContestsController>(c => c.Index());
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
                return this.RedirectToAction<ContestsController>(c => c.Index());
            }

            this.PrepareViewBagData();
            return this.View(model);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            if (!this.CheckIfUserHasContestPermissions(id))
            {
                this.TempData[GlobalConstants.DangerMessage] = GlobalConstants.NoPrivilegesMessage;
                return this.RedirectToAction<ContestsController>(c => c.Index());
            }

            var contest = this.Data.Contests
                .All()
                .Where(con => con.Id == id)
                .Select(ContestAdministrationViewModel.ViewModel)
                .FirstOrDefault();

            if (contest == null)
            {
                this.TempData.Add(GlobalConstants.DangerMessage, Resource.Contest_not_found);
                return this.RedirectToAction<ContestsController>(c => c.Index());
            }

            this.PrepareViewBagData(nameof(this.Edit));

            // TODO: replace CanBeCompeted with IsActive
            this.ViewBag.IsActive = this.contestsData.CanBeCompetedById(contest.Id.Value);
            return this.View(contest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ContestAdministrationViewModel model)
        {
            if (model.Id == null || !this.CheckIfUserHasContestPermissions(model.Id.Value))
            {
                this.TempData[GlobalConstants.DangerMessage] = GlobalConstants.NoPrivilegesMessage;
                return this.RedirectToAction<ContestsController>(c => c.Index());
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
                    return this.RedirectToAction<ContestsController>(c => c.Index());
                }

                contest = model.GetEntityModel(contest);

                // TODO: replace CanBeCompeted with IsActive
                if (contest.Type == ContestType.OnlinePractialExam &&
                    !contest.CanBeCompeted &&
                    (contest.Duration != model.Duration ||
                     contest.Type != (ContestType)model.Type))
                {
                    this.TempData.AddDangerMessage(Resource.Active_contest_cannot_edit_duration_type_problem_groups);
                    this.RedirectToAction<ContestsController>(c => c.Index());
                }

                contest.AllowedIps.Clear();
                this.AddIpsToContest(contest, model.AllowedIps);

                this.Data.Contests.Update(contest);
                this.Data.SaveChanges();

                this.TempData.Add(GlobalConstants.InfoMessage, Resource.Contest_edited);
                return this.RedirectToAction<ContestsController>(c => c.Index());
            }

            this.PrepareViewBagData();
            return this.View(model);
        }

        [HttpPost]
        public ActionResult Destroy([DataSourceRequest]DataSourceRequest request, ContestAdministrationViewModel model)
        {
            if (model.Id == null || !this.CheckIfUserHasContestPermissions(model.Id.Value))
            {
                this.TempData.AddDangerMessage(GlobalConstants.NoPrivilegesMessage);
                this.ModelState.AddModelError(string.Empty, string.Empty);
                return this.GridOperation(request, model);
            }

            if (this.contestsData.CanBeCompetedById(model.Id.Value))
            {
                this.TempData.AddDangerMessage(Resource.Active_contest_forbidden_for_deletion);
                this.ModelState.AddModelError(string.Empty, string.Empty);
                return this.GridOperation(request, model);
            }

            this.contestsData.DeleteById(model.Id.Value);
            return this.GridOperation(request, model);
        }

        public ActionResult GetFutureContests([DataSourceRequest]DataSourceRequest request)
        {
            var futureContests = this.contestsData
                .GetAllUpcoming()
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
            var activeContests = this.contestsData
                .GetAllActive()
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
            var latestContests = this.contestsData
                .GetAllVisible()
                .OrderByDescending(contest => contest.CreatedOn)
                .Take(3)
                .Select(ShortViewModelType.FromContest);

            if (!latestContests.Any())
            {
                return this.Content(Resource.No_latest_contests);
            }

            return this.PartialView(GlobalConstants.QuickContestsGrid, latestContests);
        }

        public JsonResult GetCategories(string contestFilter)
        {
            var categories = this.Data.ContestCategories.All();

            if (!this.User.IsAdmin() && this.User.IsLecturer())
            {
                categories = categories.Where(c =>
                    c.Lecturers.Any(l => l.LecturerId == this.UserProfile.Id) ||
                    c.Contests.Any(cc => !cc.IsDeleted && cc.Lecturers.Any(l => l.LecturerId == this.UserProfile.Id)));
            }

            var dropDownData = categories
                .Where(c => c.Name.ToLower().Contains(contestFilter.ToLower()))
                .ToList()
                .Select(cat =>
                    new
                    {
                        Parent = cat.Parent?.Name,
                        Name = cat.Name,
                        Value = cat.Id.ToString(CultureInfo.InvariantCulture)
                    })
                .OrderBy(a => string.IsNullOrEmpty(a.Parent))
                .ThenBy(a => a.Parent)
                .ThenBy(a => a.Name);

            return this.Json(dropDownData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult StartAsLab(LabStartInputModel inputModel)
        {
            if (!this.CheckIfUserHasContestPermissions(inputModel.ContestCreateId))
            {
                this.TempData[GlobalConstants.DangerMessage] = GlobalConstants.NoPrivilegesMessage;
                return this.RedirectToAction<ContestsController>(c => c.Index());
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

        [HttpGet]
        public ActionResult TransferParticipants(int id, string returnUrl)
        {
            returnUrl = string.IsNullOrWhiteSpace(returnUrl) ?
                this.Request.UrlReferrer?.AbsolutePath :
                UrlHelpers.ExtractFullContestsTreeUrlFromPath(returnUrl);

            if (!this.User.IsAdmin())
            {
                this.TempData[GlobalConstants.DangerMessage] = GlobalConstants.NoPrivilegesMessage;
                return this.RedirectToAction<ContestsController>(c => c.Index());
            }

            var contest = this.contestsData
                .GetAllInactive()
                .Where(c => c.Id == id)
                .Select(TransferParticipantsViewModel.FromContest)
                .FirstOrDefault();

            if (contest == null || contest.OfficialParticipantsCount == 0)
            {
                this.TempData[GlobalConstants.DangerMessage] = Resource.Contest_not_valid;
                return this.RedirectToAction<ContestsController>(c => c.Index());
            }

            this.ViewBag.ReturnUrl = returnUrl;
            
            return this.View(contest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TransferParticipants(ShortContestAdministrationViewModel model, string returnUrl)
        {
            if (!this.User.IsAdmin())
            {
                this.TempData[GlobalConstants.DangerMessage] = GlobalConstants.NoPrivilegesMessage;
                return this.RedirectToAction<ContestsController>(c => c.Index());
            }

            var categoryContest = this.Data.Contests.GetById(model.Id);

            if (categoryContest.CanBeCompeted)
            {
                this.TempData[GlobalConstants.DangerMessage] = Resource.Active_contest_forbidden_for_transfer;
                return this.RedirectToAction<ContestsController>(c => c.Index());
            }

            var competeOnlyParticipants = categoryContest
                .Participants
                .GroupBy(p => p.UserId)
                .Where(g => g.Count() == 1 && g.All(p => p.IsOfficial))
                .Select(gr => gr.FirstOrDefault());

            foreach (var participant in competeOnlyParticipants)
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
                .ToDictionary(grp => grp.Key, grp => grp.OrderBy(p => p.IsOfficial));

            foreach (var competeAndPracticeParticipant in competeAndPracticeParticipants)
            {
                var unofficialParticipant = competeAndPracticeParticipants[competeAndPracticeParticipant.Key].First();
                var officialParticipant = competeAndPracticeParticipants[competeAndPracticeParticipant.Key].Last();

                foreach (var officialParticipantSubmission in officialParticipant.Submissions)
                {
                    officialParticipantSubmission.Participant = unofficialParticipant;
                }

                var scoresForDeletion = new List<ParticipantScore>();

                foreach (var officialParticipantScore in officialParticipant.Scores)
                {
                    var unofficialParticipantScore = unofficialParticipant
                        .Scores
                        .FirstOrDefault(s => s.ProblemId == officialParticipantScore.ProblemId);

                    if (unofficialParticipantScore != null)
                    {
                        if (unofficialParticipantScore.Points < officialParticipantScore.Points ||
                            (unofficialParticipantScore.Points == officialParticipantScore.Points &&
                             unofficialParticipantScore.Id < officialParticipantScore.Id))
                        {
                            unofficialParticipantScore = officialParticipantScore;
                            unofficialParticipantScore.IsOfficial = false;
                            unofficialParticipantScore.Participant = unofficialParticipant;
                        }

                        scoresForDeletion.Add(officialParticipantScore);
                    }
                    else
                    {
                        officialParticipantScore.IsOfficial = false;
                        officialParticipantScore.Participant = unofficialParticipant;
                    }
                }

                this.participantScoresData.Delete(scoresForDeletion);

                this.Data.Participants.Delete(officialParticipant);
                this.Data.SaveChanges();
            }

            this.TempData[GlobalConstants.InfoMessage] = Resource.Participants_transferred;

            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return this.RedirectToAction<ContestsController>(c => c.Index());
            }

            return this.Redirect(returnUrl);
        }

        private void PrepareViewBagData(string callerActionName = null)
        {
            this.ViewBag.TypeData = DropdownViewModel.GetEnumValues<ContestType>();
            this.ViewBag.SubmissionExportTypes = DropdownViewModel.GetEnumValues<SubmissionExportType>();
            this.ViewBag.CallerAction = callerActionName;
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