namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Common;
    using OJS.Common.Helpers;
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Services.Business.ParticipantScores;
    using OJS.Services.Data.Participants;
    using OJS.Services.Data.ParticipantScores;
    using OJS.Services.Data.SubmissionsForProcessing;
    using OJS.Services.Data.TestRuns;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Common.Attributes;
    using OJS.Web.Common.Extensions;
    using OJS.Web.ViewModels.Common;

    using DatabaseModelType = OJS.Data.Models.Submission;
    using GridModelType = OJS.Web.Areas.Administration.ViewModels.Submission.SubmissionAdministrationGridViewModel;
    using ModelType = OJS.Web.Areas.Administration.ViewModels.Submission.SubmissionAdministrationViewModel;
    using Resource = Resources.Areas.Administration.Submissions.SubmissionsControllers;

    public class SubmissionsController : LecturerBaseGridController
    {
        protected const int RequestsPerInterval = 2;
        protected const int RestrictInterval = 180;
        protected const string TooManyRequestsErrorMessage = "Прекалено много заявки. Моля, опитайте по-късно.";

        private readonly ISubmissionsForProcessingDataService submissionsForProcessingData;
        private readonly IParticipantScoresBusinessService participantScoresBusiness;
        private readonly IParticipantScoresDataService participantScoresData;
        private readonly IParticipantsDataService participantsData;
        private readonly ITestRunsDataService testRunsData;

        private int? contestId;

        public SubmissionsController(
            IOjsData data,
            ISubmissionsForProcessingDataService submissionsForProcessingData,
            IParticipantScoresBusinessService participantScoresBusiness,
            IParticipantScoresDataService participantScoresData,
            IParticipantsDataService participantsData,
            ITestRunsDataService testRunsData)
            : base(data)
        {
            this.submissionsForProcessingData = submissionsForProcessingData;
            this.participantScoresBusiness = participantScoresBusiness;
            this.participantScoresData = participantScoresData;
            this.participantsData = participantsData;
            this.testRunsData = testRunsData;
        }

        public override IEnumerable GetData()
        {
            var submissions = this.Data.Submissions.All();

            if (!this.User.IsAdmin() && this.User.IsLecturer())
            {
                submissions = submissions.Where(s =>
                    s.Problem.ProblemGroup.Contest.Lecturers.Any(l => l.LecturerId == this.UserProfile.Id) ||
                    s.Problem.ProblemGroup.Contest.Category.Lecturers.Any(cl => cl.LecturerId == this.UserProfile.Id));
            }

            if (this.contestId != null)
            {
                submissions = submissions.Where(s => s.Problem.ProblemGroup.ContestId == this.contestId);
            }

            return submissions.Select(GridModelType.ViewModel);
        }

        public override object GetById(object id)
        {
            return this.Data.Submissions.All().FirstOrDefault(o => o.Id == (int)id);
        }

        public override string GetEntityKeyName()
        {
            return this.GetEntityKeyNameByType(typeof(DatabaseModelType));
        }

        public ActionResult Index()
        {
            this.ViewBag.SubmissionStatusData = DropdownViewModel.GetEnumValues<SubmissionStatus>();
            return this.View();
        }

        [HttpGet]
        public ActionResult Create()
        {
            this.ViewBag.SubmissionAction = "Create";
            var model = new ModelType();
            return this.View(model);
        }

        [HttpPost]
        public ActionResult ReadSubmissions([DataSourceRequest]DataSourceRequest request, int? id)
        {
            this.contestId = id;
            return this.Read(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ModelType model)
        {
            if (model != null && this.ModelState.IsValid)
            {
                if (model.ProblemId.HasValue)
                {
                    var problem = this.Data.Problems.GetById(model.ProblemId.Value);
                    if (problem != null)
                    {
                        this.ValidateParticipant(model.ParticipantId, problem.ProblemGroup.ContestId);
                    }

                    var submissionType = this.GetSubmissionType(model.SubmissionTypeId.Value);
                    if (submissionType != null)
                    {
                        this.ValidateSubmissionContentLength(model, problem);
                        this.ValidateBinarySubmission(model, problem, submissionType);
                    }
                }

                if (this.ModelState.IsValid)
                {
                    var entity = model.GetEntityModel();
                    entity.Processed = false;

                    using (var scope = TransactionsHelper.CreateTransactionScope())
                    {
                        this.BaseCreate(entity);
                        this.submissionsForProcessingData.AddOrUpdateBySubmission(model.Id.Value);

                        scope.Complete();
                    }

                    this.TempData.AddInfoMessage(Resource.Successful_creation_message);
                    return this.RedirectToAction(GlobalConstants.Index);
                }
            }

            this.ViewBag.SubmissionAction = "Create";
            return this.View(model);
        }

        [HttpGet]
        public ActionResult Update(int id)
        {
            var submission = this.Data.Submissions
                .All()
                .Where(subm => subm.Id == id)
                .Select(ModelType.ViewModel)
                .FirstOrDefault();

            if (submission == null)
            {
                this.TempData.AddDangerMessage(Resource.Invalid_submission_message);
                return this.RedirectToAction(GlobalConstants.Index);
            }

            if (!submission.ProblemId.HasValue || !this.CheckIfUserHasProblemPermissions(submission.ProblemId.Value))
            {
                this.TempData[GlobalConstants.DangerMessage] = "Нямате привилегиите за това действие";
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
            }

            this.ViewBag.SubmissionAction = "Update";
            return this.View(submission);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(ModelType model)
        {
            if (model.Id.HasValue)
            {
                if (!model.ProblemId.HasValue || !this.CheckIfUserHasProblemPermissions(model.ProblemId.Value))
                {
                    this.TempData[GlobalConstants.DangerMessage] = "Нямате привилегиите за това действие";
                    return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
                }

                var submission = this.Data.Submissions.GetById(model.Id.Value);
                if (model.SubmissionTypeId.HasValue)
                {
                    var submissionType = this.Data.SubmissionTypes.GetById(model.SubmissionTypeId.Value);
                    if (submissionType.AllowBinaryFilesUpload && model.FileSubmission == null)
                    {
                        model.Content = submission.Content;
                        model.FileExtension = submission.FileExtension;
                        if (this.ModelState.ContainsKey("Content"))
                        {
                            this.ModelState["Content"].Errors.Clear();
                        }
                    }
                }

                if (this.ModelState.IsValid)
                {
                    var problem = this.Data.Problems.GetById(model.ProblemId.Value);
                    if (problem != null)
                    {
                        this.ValidateParticipant(model.ParticipantId, problem.ProblemGroup.ContestId);
                    }

                    var submissionType = this.GetSubmissionType(model.SubmissionTypeId.Value);
                    if (submissionType != null)
                    {
                        this.ValidateSubmissionContentLength(model, problem);
                        this.ValidateBinarySubmission(model, problem, submissionType);
                    }

                    if (this.ModelState.IsValid)
                    {
                        if (this.IsSubmissionProcessing(submission))
                        {
                            this.TempData[GlobalConstants.DangerMessage] = Resource.Submission_is_processing;
                            return this.RedirectToAction(nameof(this.Update), "Submissions", new { area = "Administration" });
                        }

                        var submissionProblemId = model.ProblemId.Value;
                        var submissionParticipantId = model.ParticipantId.Value;

                        using (var scope = TransactionsHelper.CreateTransactionScope())
                        {
                            submission.Processed = false;

                            this.submissionsForProcessingData.AddOrUpdateBySubmission(submission.Id);

                            var submissionIsBestSubmission = this.IsBestSubmission(
                                submissionProblemId,
                                submissionParticipantId,
                                submission.Id);

                            if (submissionIsBestSubmission)
                            {
                                this.participantScoresBusiness.RecalculateForParticipantByProblem(
                                    submissionParticipantId,
                                    submissionProblemId);
                            }

                            this.Data.SaveChanges();

                            this.UpdateAuditInfoValues(model, submission);
                            this.BaseUpdate(model.GetEntityModel(submission));

                            scope.Complete();
                        }

                        this.TempData.AddInfoMessage(Resource.Successful_edit_message);
                        return this.RedirectToAction(GlobalConstants.Index);
                    }
                }
            }

            this.ViewBag.SubmissionAction = "Update";
            return this.View(model);
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var submission = this.Data.Submissions
                .All()
                .Where(subm => subm.Id == id)
                .Select(GridModelType.ViewModel)
                .FirstOrDefault();

            if (submission == null)
            {
                this.TempData.AddDangerMessage(Resource.Invalid_submission_message);
                return this.RedirectToAction(GlobalConstants.Index);
            }

            if (!submission.ProblemId.HasValue || !this.CheckIfUserHasProblemPermissions(submission.ProblemId.Value))
            {
                this.TempData[GlobalConstants.DangerMessage] = "Нямате привилегиите за това действие";
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
            }

            return this.View(submission);
        }

        public ActionResult ConfirmDelete(int id)
        {
            var submission = this.Data.Submissions
                .All()
                .FirstOrDefault(s => s.Id == id);

            if (submission == null)
            {
                this.TempData.AddDangerMessage(Resource.Invalid_submission_message);
                return this.RedirectToAction(GlobalConstants.Index);
            }

            if (!submission.ProblemId.HasValue || !this.CheckIfUserHasProblemPermissions(submission.ProblemId.Value))
            {
                this.TempData[GlobalConstants.DangerMessage] = "Нямате привилегиите за това действие";
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
            }

            if (!submission.ParticipantId.HasValue)
            {
                this.TempData[GlobalConstants.DangerMessage] = "Потребителя не съществува!";
                return this.RedirectToAction(nameof(ContestsController.Index), "Contests", new { area = "Administration" });
            }

            var submissionProblemId = submission.ProblemId.Value;
            var submissionParticipantId = submission.ParticipantId.Value;

            using (var scope = TransactionsHelper.CreateTransactionScope())
            {
                this.testRunsData.DeleteBySubmission(id);

                this.Data.Submissions.Delete(id);
                this.submissionsForProcessingData.RemoveBySubmission(submission.Id);

                this.Data.SaveChanges();

                var isBestSubmission = this.IsBestSubmission(
                    submissionProblemId,
                    submissionParticipantId,
                    submission.Id);

                if (isBestSubmission)
                {
                    this.participantScoresBusiness.RecalculateForParticipantByProblem(
                        submission.ParticipantId.Value,
                        submission.ProblemId.Value);
                }

                this.Data.SaveChanges();

                scope.Complete();
            }

            return this.RedirectToAction(GlobalConstants.Index);
        }

        [AuthorizeRoles(SystemRole.Administrator)]
        public ActionResult BulkDeleteSubmissions([DataSourceRequest]DataSourceRequest request)
        {
            request.PageSize = 0;

            var submissionsDataSourceResult = this.GetData().ToDataSourceResult(request);
            var submissions = submissionsDataSourceResult.Data;

            using (var scope = TransactionsHelper.CreateTransactionScope())
            {
                foreach (GridModelType submission in submissions)
                {
                    this.Data.Submissions.Delete(submission.Id);
                    this.submissionsForProcessingData.RemoveBySubmission(submission.Id);
                }

                this.Data.SaveChanges();

                foreach (GridModelType submission in submissions)
                {
                    var dbSubmission = this.Data.Submissions.GetById(submission.Id);

                    if (!dbSubmission.ParticipantId.HasValue)
                    {
                        this.TempData[GlobalConstants.DangerMessage] = "Потребителя не съществува!";
                        return this.RedirectToAction(nameof(ContestsController.Index), "Contests", new { area = "Administration" });
                    }

                    var submissionProblemId = dbSubmission.ProblemId.Value;
                    var submissionParticipantId = dbSubmission.ParticipantId.Value;

                    var isBestSubmission = this.IsBestSubmission(
                        submissionProblemId,
                        submissionParticipantId,
                        dbSubmission.Id);

                    if (isBestSubmission)
                    {
                        this.participantScoresBusiness.RecalculateForParticipantByProblem(
                            dbSubmission.ParticipantId.Value,
                            dbSubmission.ProblemId.Value);
                    }
                }

                this.Data.SaveChanges();

                scope.Complete();
            }

            this.TempData[GlobalConstants.InfoMessage] = $"Успешно изтрихте {submissionsDataSourceResult.Total} решения.";
            return this.RedirectToAction<SubmissionsController>(c => c.Index());
        }

        public JsonResult GetSubmissionTypes(int problemId, bool? allowBinaryFilesUpload)
        {
            var submissionTypesSelectListItems = this.Data.Problems.All()
                .Where(x => x.Id == problemId)
                .SelectMany(x => x.SubmissionTypes)
                .ToList()
                .Select(submissionType => new
                {
                    Text = submissionType.Name,
                    Value = submissionType.Id.ToString(),
                    submissionType.AllowBinaryFilesUpload,
                    submissionType.AllowedFileExtensions
                });

            if (allowBinaryFilesUpload.HasValue)
            {
                submissionTypesSelectListItems = submissionTypesSelectListItems
                    .Where(submissionType => submissionType.AllowBinaryFilesUpload == allowBinaryFilesUpload);
            }

            return this.Json(submissionTypesSelectListItems, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [OverrideAuthorization]
        [Authorize]
        [RestrictRequests(
            RequestsPerInterval = RequestsPerInterval,
            RestrictInterval = RestrictInterval,
            ErrorMessage = TooManyRequestsErrorMessage)]
        public ActionResult Retest(int id)
        {
            var submission = this.Data.Submissions.GetById(id);

            if (!this.ModelState.IsValid)
            {
                var modelStateErrors = this.ModelState.Values.SelectMany(m => m.Errors);
                foreach (var modelStateError in modelStateErrors)
                {
                    this.TempData.AddDangerMessage(modelStateError.ErrorMessage);
                }

                return this.RedirectToAction(nameof(ContestsController.Index), "Contests", new { area = string.Empty });
            }

            if (submission == null)
            {
                this.TempData.AddDangerMessage(Resource.Invalid_submission_message);
                return this.RedirectToAction(nameof(ContestsController.Index), "Contests", new { area = string.Empty });
            }

            var problemIdIsValid = submission.ProblemId.HasValue;
            var userOwnsSubmission = this.CheckIfUserOwnsSubmission(id);

            if (!problemIdIsValid ||
                (!this.CheckIfUserHasProblemPermissions(submission.ProblemId.Value) &&
                !userOwnsSubmission))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            if (this.CheckIfUserHasProblemPermissions(submission.ProblemId.Value) ||
                (string.IsNullOrEmpty(submission.TestRunsCache) &&
                    userOwnsSubmission &&
                    submission.Processed))
            {
                if (!submission.ParticipantId.HasValue)
                {
                    this.TempData[GlobalConstants.DangerMessage] = "Потребителя не съществува!";
                    return this.RedirectToAction(nameof(ContestsController.Index), "Contests", new { area = "Administration" });
                }

                if (this.IsSubmissionProcessing(submission))
                {
                    this.TempData[GlobalConstants.DangerMessage] = Resource.Submission_is_processing;
                    return this.RedirectToAction(nameof(ContestsController.Index), "Contests", new { area = "Administration" });
                }

                var submissionProblemId = submission.ProblemId.Value;
                var submissionParticipantId = submission.ParticipantId.Value;

                using (var scope = TransactionsHelper.CreateTransactionScope())
                {
                    submission.Processed = false;

                    this.submissionsForProcessingData.AddOrUpdateBySubmission(submission.Id);

                    var submissionIsBestSubmission = this.IsBestSubmission(
                        submissionProblemId,
                        submissionParticipantId,
                        submission.Id);

                    if (submissionIsBestSubmission)
                    {
                        this.participantScoresBusiness.RecalculateForParticipantByProblem(
                            submissionParticipantId,
                            submissionProblemId);
                    }

                    this.Data.SaveChanges();

                    scope.Complete();
                }

                this.TempData.AddInfoMessage(Resource.Retest_successful);
                return this.RedirectToAction("View", "Submissions", new { area = "Contests", id });
            }

            this.TempData[GlobalConstants.DangerMessage] = "Решението не може да бъде ретествано в момента";
            return this.RedirectToAction(nameof(ContestsController.Index), "Contests", new { area = string.Empty });
        }

        public JsonResult GetProblems(string text)
        {
            var dropDownData = this.Data.Problems.All();

            if (!string.IsNullOrEmpty(text))
            {
                dropDownData = dropDownData.Where(pr => pr.Name.ToLower().Contains(text.ToLower()));
            }

            var result = dropDownData
                .ToList()
                .Select(pr => new SelectListItem
                {
                    Text = pr.Name,
                    Value = pr.Id.ToString(),
                });

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetParticipants(string text, int problem)
        {
            var selectedProblem = this.Data.Problems.All().FirstOrDefault(pr => pr.Id == problem);

            var dropDownData = this.participantsData.GetAllByContest(selectedProblem.ProblemGroup.ContestId);

            if (!string.IsNullOrEmpty(text))
            {
                dropDownData = dropDownData.Where(part => part.User.UserName.ToLower().Contains(text.ToLower()));
            }

            var result = dropDownData
                .ToList()
                .Select(part => new SelectListItem
                {
                    Text = part.User.UserName,
                    Value = part.Id.ToString(),
                });

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RenderGrid(int? id)
        {
            this.ViewBag.SubmissionStatusData = DropdownViewModel.GetEnumValues<SubmissionStatus>();
            return this.PartialView("_SubmissionsGrid", id);
        }

        public FileResult GetSubmissionFile(int submissionId)
        {
            var submission = this.Data.Submissions.GetById(submissionId);

            return this.File(
                submission.Content,
                GlobalConstants.BinaryFileMimeType,
                $"{submission.Participant.User.UserName}_{submission.Problem.Name}.{submission.FileExtension}");
        }

        private SubmissionType GetSubmissionType(int submissionTypeId)
        {
            var submissionType = this.Data.SubmissionTypes.GetById(submissionTypeId);

            if (submissionType != null)
            {
                return submissionType;
            }

            this.ModelState.AddModelError("SubmissionTypeId", Resource.Wrong_submision_type);
            return null;
        }

        private void ValidateParticipant(int? participantId, int contestId)
        {
            if (participantId.HasValue)
            {
                if (!this.participantsData.ExistsByIdAndContest(participantId.Value, contestId))
                {
                    this.ModelState.AddModelError("ParticipantId", Resource.Invalid_task_for_participant);
                }
            }
        }

        private void ValidateSubmissionContentLength(ModelType model, Problem problem)
        {
            if (model.Content.Length > problem.SourceCodeSizeLimit)
            {
                this.ModelState.AddModelError("Content", Resource.Submission_content_length_invalid);
            }
        }

        private void ValidateBinarySubmission(ModelType model, Problem problem, SubmissionType submissionType)
        {
            if (submissionType.AllowBinaryFilesUpload && !string.IsNullOrEmpty(model.ContentAsString))
            {
                this.ModelState.AddModelError("SubmissionTypeId", Resource.Wrong_submision_type);
            }

            if (submissionType.AllowedFileExtensions != null)
            {
                if (!submissionType.AllowedFileExtensionsList.Contains(model.FileExtension))
                {
                    this.ModelState.AddModelError("Content", Resource.Invalid_file_extention);
                }
            }
        }

        private bool IsBestSubmission(int problemId, int participantId, int submissionId)
        {
            var bestScore = this.participantScoresData.GetByParticipantIdAndProblemId(participantId, problemId);

            return bestScore?.SubmissionId == submissionId;
        }

        private bool IsSubmissionProcessing(Submission submission)
        {
            if (submission.Processed)
            {
                return false;
            }

            var submissionForProcessing = this.submissionsForProcessingData.GetBySubmission(submission.Id);

            return submissionForProcessing != null && !submissionForProcessing.Processed;
        }
    }
}