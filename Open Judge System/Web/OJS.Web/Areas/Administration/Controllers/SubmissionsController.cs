namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.UI;

    using OJS.Common;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Areas.Administration.ViewModels.Submission;
    
    using DatabaseModelType = OJS.Data.Models.Submission;
    using GridModelType = OJS.Web.Areas.Administration.ViewModels.Submission.SubmissionAdministrationGridViewModel;
    using ModelType = OJS.Web.Areas.Administration.ViewModels.Submission.SubmissionAdministrationViewModel;

    public class SubmissionsController : LecturerBaseGridController
    {
        private const string SuccessfulCreationMessage = "Решението беше добавено успешно!";
        private const string SuccessfulEditMessage = "Решението беше променено успешно!";
        private const string InvalidSubmissionMessage = "Невалидно решение!";
        private const string RetestSuccessful = "Решението беше успешно пуснато за ретестване!";

        private int? contestId;

        public SubmissionsController(IOjsData data)
            : base(data)
        {
        }

        public override IEnumerable GetData()
        {
            var submissions = this.Data.Submissions.All();

            if (this.contestId != null)
            {
                submissions = submissions.Where(s => s.Problem.ContestId == this.contestId);
            }

            return submissions.Select(GridModelType.ViewModel);
        }

        public override object GetById(object id)
        {
            return this.Data.Submissions
                .All()
                .FirstOrDefault(o => o.Id == (int)id);
        }

        public override string GetEntityKeyName()
        {
            return this.GetEntityKeyNameByType(typeof(DatabaseModelType));
        }

        public ActionResult Index()
        {
            return this.View();
        }

        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.SubmissionAction = "Create";
            var model = new SubmissionAdministrationViewModel();
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
                        this.ValidateParticipant(model.ParticipantId, problem.ContestId);
                    }

                    var submissionType = this.GetSubmissionType(model.SubmissionTypeId.Value);
                    if (submissionType != null)
                    {
                        this.ValidateSubmissionContentLenght(model, problem);
                        this.ValidateBinarySubmission(model, problem, submissionType);
                    }
                }

                if (this.ModelState.IsValid)
                {
                    var entity = model.GetEntityModel();
                    entity.Processed = false;
                    entity.Processing = false;
                    this.BaseCreate(entity);
                    this.TempData[GlobalConstants.InfoMessage] = SuccessfulCreationMessage;
                    return this.RedirectToAction(GlobalConstants.Index);
                }
            }

            ViewBag.SubmissionAction = "Create";
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
                this.TempData[GlobalConstants.DangerMessage] = InvalidSubmissionMessage;
                return this.RedirectToAction(GlobalConstants.Index);
            }

            if (!submission.ProblemId.HasValue || !this.CheckIfUserHasProblemPermissions(submission.ProblemId.Value))
            {
                this.TempData[GlobalConstants.DangerMessage] = "Нямате привилегиите за това действие";
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
            }

            ViewBag.SubmissionAction = "Update";
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
                        if (ModelState.ContainsKey("Content"))
                        {
                            ModelState["Content"].Errors.Clear();
                        }
                    }
                }
            }

            if (this.ModelState.IsValid)
            {
                if (model.ProblemId.HasValue)
                {
                    var problem = this.Data.Problems.GetById(model.ProblemId.Value);
                    if (problem != null)
                    {
                        this.ValidateParticipant(model.ParticipantId, problem.ContestId);
                    }

                    var submissionType = this.GetSubmissionType(model.SubmissionTypeId.Value);
                    if (submissionType != null)
                    {
                        this.ValidateSubmissionContentLenght(model, problem);
                        this.ValidateBinarySubmission(model, problem, submissionType);
                    }
                }

                if (this.ModelState.IsValid)
                {
                    var entity = this.GetById(model.Id) as DatabaseModelType;
                    entity.Processed = false;
                    entity.Processing = false;

                    this.UpdateAuditInfoValues(model, entity);
                    this.BaseUpdate(model.GetEntityModel(entity));
                    this.TempData[GlobalConstants.InfoMessage] = SuccessfulEditMessage;
                    return this.RedirectToAction(GlobalConstants.Index);
                }
            }

            ViewBag.SubmissionAction = "Update";
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
                this.TempData[GlobalConstants.DangerMessage] = InvalidSubmissionMessage;
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
                .FirstOrDefault(subm => subm.Id == id);

            if (submission == null)
            {
                this.TempData[GlobalConstants.DangerMessage] = InvalidSubmissionMessage;
                return this.RedirectToAction(GlobalConstants.Index);
            }

            if (!submission.ProblemId.HasValue || !this.CheckIfUserHasProblemPermissions(submission.ProblemId.Value))
            {
                this.TempData[GlobalConstants.DangerMessage] = "Нямате привилегиите за това действие";
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
            }

            this.Data.TestRuns.Delete(tr => tr.SubmissionId == id);

            this.Data.Submissions.Delete(id);
            this.Data.SaveChanges();

            return this.RedirectToAction(GlobalConstants.Index);
        }

        public JsonResult GetSubmissionTypes(int problemId, bool? allowBinaryFilesUpload)
        {
            var selectedProblemContest = this.Data.Contests.All().FirstOrDefault(contest => contest.Problems.Any(problem => problem.Id == problemId));

            var submissionTypesSelectListItems = selectedProblemContest.SubmissionTypes
                .ToList()
                .Select(subm => new
                {
                    Text = subm.Name,
                    Value = subm.Id.ToString(),
                    AllowBinaryFilesUpload = subm.AllowBinaryFilesUpload,
                    AllowedFileExtensions = subm.AllowedFileExtensions
                });

            if (allowBinaryFilesUpload.HasValue)
            {
                submissionTypesSelectListItems = submissionTypesSelectListItems
                    .Where(submissionType => submissionType.AllowBinaryFilesUpload == allowBinaryFilesUpload);
            }

            return this.Json(submissionTypesSelectListItems, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Retest(int id)
        {
            var submission = this.Data.Submissions.GetById(id);

            if (submission == null)
            {
                this.TempData[GlobalConstants.DangerMessage] = InvalidSubmissionMessage;
            }
            else
            {
                if (!submission.ProblemId.HasValue || !this.CheckIfUserHasProblemPermissions(submission.ProblemId.Value))
                {
                    this.TempData[GlobalConstants.DangerMessage] = "Нямате привилегиите за това действие";
                    return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
                }

                submission.Processed = false;
                submission.Processing = false;
                this.Data.SaveChanges();

                this.TempData[GlobalConstants.InfoMessage] = RetestSuccessful;
            }

            return this.RedirectToAction("View", "Submissions", new { area = "Contests", id = id });
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

            var dropDownData = this.Data.Participants.All().Where(part => part.ContestId == selectedProblem.ContestId);

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
            return this.PartialView("_SubmissionsGrid", id);
        }

        public JsonResult Contests(string text)
        {
            var contests = this.Data.Contests
                .All()
                .OrderByDescending(c => c.CreatedOn)
                .Select(c => new
                    {
                        Id = c.Id,
                        Name = c.Name
                    });

            if (!string.IsNullOrEmpty(text))
            {
                contests = contests.Where(c => c.Name.ToLower().Contains(text.ToLower()));
            }

            return this.Json(contests, JsonRequestBehavior.AllowGet);
        }

        public FileResult GetSubmissionFile(int submissionId)
        {
            var submission = this.Data.Submissions.GetById(submissionId);

            return this.File(
                submission.Content,
                "application/octet-stream",
                string.Format("{0}_{1}.{2}", submission.Participant.User.UserName, submission.Problem.Name, submission.FileExtension));
        }

        private SubmissionType GetSubmissionType(int submissionTypeId)
        {
            var submissionType = this.Data.SubmissionTypes.GetById(submissionTypeId);

            if (submissionType != null)
            {
                return submissionType;
            }

            this.ModelState.AddModelError("SubmissionTypeId", "Wrong submission type!");
            return null;
        }

        private void ValidateParticipant(int? participantId, int contestId)
        {
            if (participantId.HasValue)
            {
                if (!this.Data.Participants.All().Any(participant => participant.Id == participantId.Value && participant.ContestId == contestId))
                {
                    this.ModelState.AddModelError("ParticipantId", "Задачата не е от състезанието, от което е избраният участник!");
                }
            }
        }

        private void ValidateSubmissionContentLenght(ModelType model, Problem problem)
        {
            if (model.Content.Length > problem.SourceCodeSizeLimit)
            {
                ModelState.AddModelError("Content", "Решението надвишава лимита за големина!");
            }
        }

        private void ValidateBinarySubmission(ModelType model, Problem problem, SubmissionType submissionType)
        {
            if (submissionType.AllowBinaryFilesUpload && !string.IsNullOrEmpty(model.ContentAsString))
            {
                ModelState.AddModelError("SubmissionTypeId", "Невалиден тип на решението!");
            }

            if (submissionType.AllowedFileExtensions != null)
            {
                if (!submissionType.AllowedFileExtensionsList.Contains(model.FileExtension))
                {
                    ModelState.AddModelError("Content", "Невалидно разширение на файл!");
                }
            }
        }
    }
}