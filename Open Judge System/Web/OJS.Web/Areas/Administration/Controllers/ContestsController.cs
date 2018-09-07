namespace OJS.Web.Areas.Administration.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity;
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
    using OJS.Services.Data.ContestCategories;
    using OJS.Services.Data.Contests;
    using OJS.Services.Data.Ips;
    using OJS.Services.Data.Participants;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Areas.Administration.ViewModels.Contest;
    using OJS.Web.Areas.Contests.Models;
    using OJS.Web.Common.Extensions;
    using OJS.Web.ViewModels.Common;

    using ChangeTimeResource = Resources.Areas.Administration.Contests.Views.ChangeTime;
    using GeneralResource = Resources.Areas.Administration.AdministrationGeneral;
    using Resource = Resources.Areas.Administration.Contests.ContestsControllers;
    using ShortViewModelType = OJS.Web.Areas.Administration.ViewModels.Contest.ShortContestAdministrationViewModel;
    using ViewModelType = OJS.Web.Areas.Administration.ViewModels.Contest.ContestAdministrationViewModel;

    public class ContestsController : LecturerBaseGridController
    {
        private const int ProblemGroupsCountLimit = 40;

        private readonly IContestsDataService contestsData;
        private readonly IContestCategoriesDataService contestCategoriesData;
        private readonly IParticipantsDataService participantsData;
        private readonly IIpsDataService ipsData;
        private readonly IContestsBusinessService contestsBusiness;
        private readonly IParticipantsBusinessService participantsBusiness;

        public ContestsController(
            IOjsData data,
            IContestsDataService contestsData,
            IContestCategoriesDataService contestCategoriesData,
            IParticipantsDataService participantsData,
            IIpsDataService ipsData,
            IContestsBusinessService contestsBusiness,
            IParticipantsBusinessService participantsBusiness)
                : base(data)
        {
            this.contestsData = contestsData;
            this.contestCategoriesData = contestCategoriesData;
            this.participantsData = participantsData;
            this.ipsData = ipsData;
            this.contestsBusiness = contestsBusiness;
            this.participantsBusiness = participantsBusiness;
        }

        public override IEnumerable GetData()
        {
            var allContests = this.contestsData.GetAll();

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
            return this.contestsData
                .GetAll()
                .AsNoTracking()
                .FirstOrDefault(c => c.Id == (int)id);
        }

        public ActionResult Index()
        {
            this.PrepareViewBagData();
            return this.View();
        }

        [HttpGet]
        public ActionResult Create(int? categoryId)
        {
            this.PrepareViewBagData();

            var viewModel = new ViewModelType();

            if (categoryId.HasValue)
            {
                var categoryName = this.contestCategoriesData.GetNameById(categoryId.Value);

                if (categoryName != null)
                {
                    viewModel.CategoryId = categoryId;
                    viewModel.CategoryName = categoryName;
                }
            }

            return this.View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ContestAdministrationViewModel model)
        {
            if (model?.CategoryId == null || !this.CheckIfUserHasContestCategoryPermissions(model.CategoryId.Value))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            this.ValidateContest(model);

            if (!this.ModelState.IsValid)
            {
                this.PrepareViewBagData();
                return this.View(model);
            }

            var contest = model.GetEntityModel();

            this.AddProblemGroupsToContest(contest, model.ProblemGroupsCount);

            this.AddIpsToContest(contest, model.AllowedIps);

            this.contestsData.Add(contest);

            this.TempData.Add(GlobalConstants.InfoMessage, Resource.Contest_added);
            return this.RedirectToAction<ContestsController>(c => c.Index());
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            if (!this.CheckIfUserHasContestPermissions(id))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            var contest = this.contestsData
                .GetByIdQuery(id)
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
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            this.ValidateContest(model);

            if (!this.ModelState.IsValid)
            {
                this.PrepareViewBagData(model.Id);
                return this.View(model);
            }

            var contest = this.contestsData.GetById(model.Id.Value);

            if (contest == null)
            {
                this.TempData.Add(GlobalConstants.DangerMessage, Resource.Contest_not_found);
                return this.RedirectToAction<ContestsController>(c => c.Index());
            }

            var originalContestPassword = contest.ContestPassword;
            var originalPracticePassword = contest.PracticePassword;

            if (contest.IsOnline &&
                contest.IsActive &&
                (contest.Duration != model.Duration ||
                    (int)contest.Type != model.Type))
            {
                this.TempData.AddDangerMessage(Resource.Active_contest_cannot_edit_duration_type);
                this.RedirectToAction<ContestsController>(c => c.Index());
            }

            contest = model.GetEntityModel(contest);

            if (contest.IsOnline && contest.ProblemGroups.Count == 0)
            {
                this.AddProblemGroupsToContest(contest, model.ProblemGroupsCount);
            }

            if (!contest.IsOnline && contest.Duration != null)
            {
                contest.Duration = null;
            }

            contest.AllowedIps.Clear();
            this.AddIpsToContest(contest, model.AllowedIps);

            this.contestsData.Update(contest);

            this.InvalidateParticipants(originalContestPassword, originalPracticePassword, contest);

            this.TempData.Add(GlobalConstants.InfoMessage, Resource.Contest_edited);
            return this.RedirectToAction<ContestsController>(c => c.Index());
        }

        [HttpPost]
        public ActionResult Destroy([DataSourceRequest]DataSourceRequest request, ContestAdministrationViewModel model)
        {
            if (model.Id == null || !this.CheckIfUserHasContestPermissions(model.Id.Value))
            {
                this.ModelState.AddModelError(string.Empty, GeneralResource.No_privileges_message);
                return this.GridOperation(request, model);
            }

            if (this.contestsData.IsActiveById(model.Id.Value))
            {
                this.ModelState.AddModelError(string.Empty, Resource.Active_contest_forbidden_for_deletion);
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

        [HttpGet]
        public ActionResult ChangeActiveParticipantsEndTime(int id)
        {
            if (!this.User.IsAdmin())
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            var contest = this.contestsData
                .GetByIdQuery(id)
                .Select(ChangeParticipationEndTimeViewModel.FromContest)
                .FirstOrDefault();

            if (contest == null)
            {
                this.TempData.AddDangerMessage(Resource.Contest_not_valid);
                return this.RedirectToAction<ContestsController>(c => c.Index());
            }

            return this.View(contest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeParticipationEndTimeByTimeInterval(
            ChangeParticipationEndTimeByTimeIntervalViewModel model)
        {
            if (!this.User.IsAdmin())
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            if (!this.ModelState.IsValid)
            {
                return this.View("ChangeActiveParticipantsEndTime", new ChangeParticipationEndTimeViewModel(model));
            }

            if (!this.contestsData.IsActiveById(model.ContesId))
            {
                this.TempData.AddDangerMessage(Resource.Contest_not_valid);
                return this.RedirectToAction<ContestsController>(c => c.Index());
            }

            if (model.ParticipantsCreatedBeforeDateTime < model.ParticipantsCreatedAfterDateTime)
            {
                this.ModelState.AddModelError(
                    nameof(model.ParticipantsCreatedAfterDateTime),
                    ChangeTimeResource.Participants_created_after_must_be_before_Participants_created_before);
                return this.View("ChangeActiveParticipantsEndTime", new ChangeParticipationEndTimeViewModel(model));
            }

            var result = this.participantsBusiness
                .UpdateParticipationsEndTimeByContestByParticipationStartTimeRangeAndTimeInMinutes(
                    model.ContesId,
                    model.TimeInMinutes,
                    model.ParticipantsCreatedAfterDateTime.Value,
                    model.ParticipantsCreatedBeforeDateTime.Value);

            if (!result.IsError)
            {
                var systemMessage = this.GetMessageForChangedParticipantsEndTimeByTimeInterval(
                    model.TimeInMinutes,
                    model.ContestName,
                    result.Data);

                this.TempData.AddInfoMessage(systemMessage);
            }
            else
            {
                this.TempData.AddDangerMessage(result.Error);
            }

            return this.RedirectToAction("Details", "Contests", new { id = model.ContesId, area = "Contests" });
        }

        [HttpPost]
        public ActionResult ChangeParticipationEndTimeByUser(
            ChangeParticipationEndTimeByUserViewModel model)
        {
            if (!this.User.IsAdmin())
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            if (!this.ModelState.IsValid)
            {
                return this.View("ChangeActiveParticipantsEndTime", new ChangeParticipationEndTimeViewModel(model));
            }

            if (!this.contestsData.IsActiveById(model.ContesId))
            {
                this.TempData.AddDangerMessage(Resource.Contest_not_valid);
                return this.RedirectToAction<ContestsController>(c => c.Index());
            }

            var participant = this.participantsData
                .GetByContestByUserAndByIsOfficial(model.ContesId, model.UserId, isOfficial: true);

            if (participant == null)
            {
                this.ModelState.AddModelError(nameof(model.UserId), Resource.Participant_not_in_contest);
                return this.View("ChangeActiveParticipantsEndTime", new ChangeParticipationEndTimeViewModel(model));
            }

            var result = this.participantsBusiness.UpdateParticipationEndTimeByIdAndTimeInMinutes(
                participant.Id,
                model.TimeInMinutes);

            if (!result.IsError)
            {
                var systemMessage = this.GetMessageForChangedParticipantsEndTimeByUser(
                    model.TimeInMinutes,
                    model.ContestName,
                    result.Data);

                this.TempData.AddInfoMessage(systemMessage);
            }
            else
            {
                this.TempData.AddDangerMessage(result.Error);
            }

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
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
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
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
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

                if (model.ProblemGroupsCount <= 0)
                {
                    this.ModelState.AddModelError(nameof(model.ProblemGroupsCount), Resource.Required_field_for_online);
                }
                else if (model.ProblemGroupsCount > ProblemGroupsCountLimit)
                {
                    this.ModelState.AddModelError(
                        nameof(model.ProblemGroupsCount),
                        string.Format(Resource.Problem_groups_count_limit, ProblemGroupsCountLimit));
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
                    var ip = this.ipsData.GetByValue(ipValue) ?? new Ip { Value = ipValue };

                    contest.AllowedIps.Add(new ContestIp { Ip = ip, IsOriginallyAllowed = true });
                }
            }
        }

        private void AddProblemGroupsToContest(Contest contest, int problemGroupsCount)
        {
            for (var i = 1; i <= problemGroupsCount; i++)
            {
                contest.ProblemGroups.Add(new ProblemGroup
                {
                    OrderBy = i
                });
            }
        }

        private void InvalidateParticipants(
            string orginalContestPassword,
            string originalPracticePassword,
            Contest contest)
        {
            if (orginalContestPassword != contest.ContestPassword &&
                !string.IsNullOrWhiteSpace(contest.ContestPassword))
            {
                this.participantsData.InvalidateByContestAndIsOfficial(contest.Id, isOfficial: true);
            }

            if (originalPracticePassword != contest.PracticePassword &&
                !string.IsNullOrWhiteSpace(contest.PracticePassword))
            {
                this.participantsData.InvalidateByContestAndIsOfficial(contest.Id, isOfficial: false);
            }
        }

        private string GetMessageForChangedParticipantsEndTimeByTimeInterval(
            int timeInMinutes,
            string contestName,
            ICollection<string> notUpdatedParticipantUsernames)
        {
            var minutesForDisplay = timeInMinutes.ToString();
            var formatString = Resource.Added_time_to_participants_online;

            if (timeInMinutes < 0)
            {
                formatString = Resource.Subtracted_time_from_participants_online;
                minutesForDisplay = minutesForDisplay.Substring(1, minutesForDisplay.Length - 1);
            }

            var message = string.Format(formatString, minutesForDisplay, contestName);

            if (notUpdatedParticipantUsernames.Any())
            {
                var warningMessage = string.Format(
                    Resource.Failed_to_update_participants_duration,
                    string.Join(", ", notUpdatedParticipantUsernames));

                message += Environment.NewLine + warningMessage;
            }

            return message;
        }

        private string GetMessageForChangedParticipantsEndTimeByUser(
            int timeInMinutes,
            string contestName,
            string username)
        {
            var minutesForDisplay = timeInMinutes.ToString();

            var formatString = Resource.Added_time_to_single_participant_online;

            if (timeInMinutes < 0)
            {
                formatString = Resource.Subtracted_time_from_single_participant_online;
                minutesForDisplay = minutesForDisplay.Substring(1, minutesForDisplay.Length - 1);
            }

            var message = string.Format(formatString, minutesForDisplay, username, contestName);

            return message;
        }
    }
}