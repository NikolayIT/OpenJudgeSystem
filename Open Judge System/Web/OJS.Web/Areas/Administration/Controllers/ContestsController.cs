namespace OJS.Web.Areas.Administration.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Web.Mvc;

    using Kendo.Mvc.UI;

    using OJS.Common;
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Services.Business.Contests;
    using OJS.Services.Business.Participants;
    using OJS.Services.Data.Contests;
    using OJS.Services.Data.Participants;
    using OJS.Services.Data.ParticipantScores;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Areas.Administration.InputModels.Contests;
    using OJS.Web.Areas.Administration.ViewModels.Contest;
    using OJS.Web.Areas.Contests.Helpers;
    using OJS.Web.Areas.Contests.Models;
    using OJS.Web.Common.Extensions;
    using OJS.Web.ViewModels.Common;

    using ChangeTimeResource = Resources.Areas.Administration.Contests.Views.ChangeTime;
    using Resource = Resources.Areas.Administration.Contests.ContestsControllers;
    using ShortViewModelType = OJS.Web.Areas.Administration.ViewModels.Contest.ShortContestAdministrationViewModel;
    using ViewModelType = OJS.Web.Areas.Administration.ViewModels.Contest.ContestAdministrationViewModel;

    public class ContestsController : LecturerBaseGridController
    {
        private const int StartTimeDelayInSeconds = 10;
        private const int LabDurationInSeconds = 30 * 60;

        private readonly IContestsDataService contestsData;
        private readonly IContestsBusinessService contestsBusiness;
        private readonly IParticipantsBusinessService participantsBusiness;

        public ContestsController(
            IOjsData data,
            IContestsDataService contestsData,
            IContestsBusinessService contestsBusiness,
            IParticipantsBusinessService participantsBusiness)
                : base(data)
        {
            this.contestsData = contestsData;
            this.contestsBusiness = contestsBusiness;
            this.participantsBusiness = participantsBusiness;
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

            this.ValidateContest(model);

            if (!this.ModelState.IsValid)
            {
                this.PrepareViewBagData();
                return this.View(model);
            }

            var contest = model.GetEntityModel();

            this.AddIpsToContest(contest, model.AllowedIps);

            this.Data.Contests.Add(contest);
            this.Data.SaveChanges();

            this.TempData.Add(GlobalConstants.InfoMessage, Resource.Contest_added);
            return this.RedirectToAction<ContestsController>(c => c.Index());
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

            if (contest?.Id == null)
            {
                this.TempData.Add(GlobalConstants.DangerMessage, Resource.Contest_not_found);
                return this.RedirectToAction<ContestsController>(c => c.Index());
            }

            this.PrepareViewBagData(contest.Id);

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

            this.ValidateContest(model);

            if (!this.ModelState.IsValid)
            {
                this.PrepareViewBagData(model.Id);
                return this.View(model);
            }

            var contest = this.Data.Contests.All().FirstOrDefault(c => c.Id == model.Id);

            if (contest == null)
            {
                this.TempData.Add(GlobalConstants.DangerMessage, Resource.Contest_not_found);
                return this.RedirectToAction<ContestsController>(c => c.Index());
            }

            contest = model.GetEntityModel(contest);

            if (contest.IsOnline &&
                contest.IsActive &&
                (contest.Duration != model.Duration ||
                 contest.NumberOfProblemGroups != model.NumberOfProblemGroups ||
                 (int)contest.Type != model.Type))
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

        [HttpPost]
        public ActionResult Destroy([DataSourceRequest]DataSourceRequest request, ContestAdministrationViewModel model)
        {
            if (model.Id == null || !this.CheckIfUserHasContestPermissions(model.Id.Value))
            {
                this.TempData.AddDangerMessage(GlobalConstants.NoPrivilegesMessage);
                this.ModelState.AddModelError(string.Empty, string.Empty);
                return this.GridOperation(request, model);
            }

            if (this.contestsData.IsActiveById(model.Id.Value))
            {
                this.TempData.AddDangerMessage(Resource.Active_contest_forbidden_for_deletion);
                this.ModelState.AddModelError(string.Empty, string.Empty);
                return this.GridOperation(request, model);
            }

            this.contestsBusiness.DeleteById(model.Id.Value);
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

            if (!string.IsNullOrWhiteSpace(contestFilter))
            {
                categories = categories.Where(c => c.Name.ToLower().Contains(contestFilter.ToLower()));
            }

            var dropDownData = categories
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
        public ActionResult ChangeActiveParticipantsEndTime(int id)
        {
            if (!this.User.IsAdmin())
            {
                this.TempData.AddDangerMessage(GlobalConstants.NoPrivilegesMessage);
                return this.RedirectToAction<ContestsController>(c => c.Index());
            }

            var contest = this.contestsData
                .GetByIdQuery(id)
                .Select(ChangeTimeForParticipantsViewModel.FromContest)
                .FirstOrDefault();

            if (contest != null)
            {
                this.ViewBag.CurrentUsername = this.UserProfile.UserName;
                return this.View(contest);
            }

            this.TempData.AddDangerMessage(Resource.Contest_not_valid);
            return this.RedirectToAction<ContestsController>(c => c.Index());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeActiveParticipantsEndTime(ChangeTimeForParticipantsViewModel model)
        {
            if (!this.User.IsAdmin())
            {
                this.TempData.AddDangerMessage(GlobalConstants.NoPrivilegesMessage);
                return this.RedirectToAction<ContestsController>(c => c.Index());
            }

            if (!this.ModelState.IsValid)
            {
                this.ViewBag.CurrentUsername = this.UserProfile.UserName;
                return this.View(model);
            }

            if (model.ParticipantsCreatedBeforeDateTime < model.ParticipantsCreatedAfterDateTime)
            {
                this.ModelState.AddModelError(
                    nameof(model.ParticipantsCreatedAfterDateTime),
                    ChangeTimeResource.Participants_created_after_must_be_before_Participants_created_before);
                this.ViewBag.CurrentUsername = this.UserProfile.UserName;
                return this.View(model);
            }

            if (!this.contestsData.GetAllActive().Any(c => c.Id == model.ContesId))
            {
                this.TempData.AddDangerMessage(Resource.Contest_not_valid);
                return this.RedirectToAction<ContestsController>(c => c.Index());
            }

            var notUpdatedUsersUsernames = this.participantsBusiness
                .GetAllParticipantsWhoWouldBeReducedBelowDefaultContestDuration(
                    model.ContesId,
                    model.TimeInMinutes,
                    model.ParticipantsCreatedAfterDateTime.Value,
                    model.ParticipantsCreatedBeforeDateTime.Value)
                .Select(u => u.User.UserName)
                .ToList();

            this.participantsBusiness
                .UpdateContestEndTimeForAllParticipantsByContestByParticipantContestStartTimeRangeAndTimeIntervalInMinutes(
                    model.ContesId,
                    model.TimeInMinutes,
                    model.ParticipantsCreatedAfterDateTime.Value,
                    model.ParticipantsCreatedBeforeDateTime.Value);

            var minutesForDisplay = model.TimeInMinutes.ToString();

            var sb = new StringBuilder();

            var successMessage = model.TimeInMinutes >= 0
                ? string.Format(Resource.Added_time_to_participants_online, minutesForDisplay, model.ContestName)
                : string.Format(
                    Resource.Subtracted_time_from_participants_online,
                    minutesForDisplay.Substring(1, minutesForDisplay.Length - 1),
                    model.ContestName);
            sb.AppendLine(successMessage);

            if (notUpdatedUsersUsernames.Any())
            {
                var warningMessage = string.Format(
                    Resource.Failed_to_update_participants_duration,
                    string.Join(", ", notUpdatedUsersUsernames));
              sb.AppendLine(warningMessage);
            }

            this.TempData.AddInfoMessage(sb.ToString());

            return this.RedirectToAction("Details", "Contests", new { id = model.ContesId, area = "Contests" });
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
                this.TempData.AddDangerMessage(GlobalConstants.NoPrivilegesMessage);
                return this.RedirectToAction<ContestsController>(c => c.Index());
            }

            var result = this.contestsBusiness.TransferParticipantsToPracticeById(model.Id);

            if (result.IsError)
            {
                this.TempData.AddDangerMessage(result.Error);
                return this.RedirectToAction<ContestsController>(c => c.Index());
            }

            this.TempData.AddInfoMessage(Resource.Participants_transferred);

            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return this.RedirectToAction<ContestsController>(c => c.Index());
            }

            return this.Redirect(returnUrl);
        }

        private void PrepareViewBagData(int? contestId = null)
        {
            this.ViewBag.TypeData = DropdownViewModel.GetEnumValues<ContestType>();
            this.ViewBag.SubmissionExportTypes = DropdownViewModel.GetEnumValues<SubmissionExportType>();

            if (contestId.HasValue)
            {
                // TODO: find a better solution for determining whether a Contest is active or not
                this.ViewBag.IsActive = this.contestsData.IsActiveById(contestId.Value);
            }
        }

        private void ValidateContest(ViewModelType model)
        {
            if (model.StartTime >= model.EndTime)
            {
                this.ModelState.AddModelError(GlobalConstants.DateTimeError, Resource.Contest_start_date_before_end);
            }

            if (model.PracticeStartTime >= model.PracticeEndTime)
            {
                this.ModelState.AddModelError(GlobalConstants.DateTimeError, Resource.Practice_start_date_before_end);
            }

            if (model.IsOnline)
            {
                if (!model.Duration.HasValue)
                {
                    this.ModelState.AddModelError(nameof(model.Duration), Resource.Required_field_for_online);
                }
                else if (model.Duration.Value.TotalHours >= 24)
                {
                    this.ModelState.AddModelError(nameof(model.Duration), Resource.Duration_invalid_format);
                }

                if (model.NumberOfProblemGroups <= 0)
                {
                    this.ModelState.AddModelError(nameof(model.NumberOfProblemGroups), Resource.Required_field_for_online);
                }
            }
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