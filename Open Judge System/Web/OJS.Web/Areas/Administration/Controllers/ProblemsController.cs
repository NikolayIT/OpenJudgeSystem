namespace OJS.Web.Areas.Administration.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net.Mime;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Expressions;

    using Ionic.Zip;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using MissingFeatures;

    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Services.Business.Problems;
    using OJS.Services.Data.Checkers;
    using OJS.Services.Data.Contests;
    using OJS.Services.Data.ProblemGroups;
    using OJS.Services.Data.ProblemResources;
    using OJS.Services.Data.Problems;
    using OJS.Services.Data.Submissions;
    using OJS.Services.Data.SubmissionTypes;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Areas.Administration.ViewModels.Contest;
    using OJS.Web.Areas.Administration.ViewModels.Problem;
    using OJS.Web.Areas.Administration.ViewModels.ProblemResource;
    using OJS.Web.Areas.Administration.ViewModels.Submission;
    using OJS.Web.Areas.Administration.ViewModels.SubmissionType;
    using OJS.Web.Common;
    using OJS.Web.Common.Attributes;
    using OJS.Web.Common.Extensions;
    using OJS.Web.Common.ZippedTestManipulator;
    using OJS.Web.ViewModels.Common;
    using OJS.Workers.Common;
    using OJS.Workers.Common.Extensions;

    using GeneralResource = Resources.Areas.Administration.AdministrationGeneral;
    using GlobalResource = Resources.Areas.Administration.Problems.ProblemsControllers;
    using ViewModelType = OJS.Web.Areas.Administration.ViewModels.Problem.ProblemAdministrationViewModel;

    [RouteArea(GlobalConstants.AdministrationAreaName, AreaPrefix = GlobalConstants.AdministrationAreaName)]
    [RoutePrefix("Problems")]
    public class ProblemsController : LecturerBaseController
    {
        private readonly IContestsDataService contestsData;
        private readonly ICheckersDataService checkersData;
        private readonly IProblemsDataService problemsData;
        private readonly IProblemGroupsDataService problemGroupsData;
        private readonly IProblemResourcesDataService problemResourcesData;
        private readonly ISubmissionsDataService submissionsData;
        private readonly ISubmissionTypesDataService submissionTypesData;
        private readonly IProblemsBusinessService problemsBusiness;

        public ProblemsController(
            IOjsData data,
            IContestsDataService contestsData,
            ICheckersDataService checkersData,
            IProblemsDataService problemsData,
            IProblemGroupsDataService problemGroupsData,
            IProblemResourcesDataService problemResourcesData,
            ISubmissionsDataService submissionsData,
            ISubmissionTypesDataService submissionTypesData,
            IProblemsBusinessService problemsBusiness)
            : base(data)
        {
            this.contestsData = contestsData;
            this.checkersData = checkersData;
            this.problemsData = problemsData;
            this.problemGroupsData = problemGroupsData;
            this.problemResourcesData = problemResourcesData;
            this.submissionsData = submissionsData;
            this.submissionTypesData = submissionTypesData;
            this.problemsBusiness = problemsBusiness;
        }

        public ActionResult Index() => this.View();

        [Route("Contest/{contestId:int}")]
        public ActionResult Index(int contestId)
        {
            if (!this.CheckIfUserHasContestPermissions(contestId))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            this.ViewBag.ContestId = contestId;

            return this.View();
        }

        public ActionResult Resource(int? id)
        {
            if (id == null || !this.CheckIfUserHasProblemPermissions(id.Value))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            var problem = this.problemsData.GetWithProblemGroupById(id.Value);

            if (problem == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_problem);
                return this.RedirectToAction(c => c.Index());
            }

            this.ViewBag.ContestId = problem.ProblemGroup.ContestId;
            this.ViewBag.ProblemId = problem.Id;

            return this.View(nameof(this.Index));
        }

        [HttpGet]
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_contest);
                return this.RedirectToAction(c => c.Index());
            }

            if (!this.CheckIfUserHasContestPermissions(id.Value))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            var contest = this.contestsData.GetById(id.Value);

            if (contest == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_contest);
                return this.RedirectToAction(c => c.Index());
            }

            var problem = this.PrepareProblemViewModelForCreate(contest);
            return this.View(problem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int id, ViewModelType problem)
        {
            if (!this.CheckIfUserHasContestPermissions(id))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            var contest = this.contestsData.GetById(id);
            if (contest == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_contest);
                return this.RedirectToAction(c => c.Index());
            }

            if (problem == null)
            {
                problem = this.PrepareProblemViewModelForCreate(contest);

                this.AddCheckersToProblemViewModel(problem);

                return this.View(problem);
            }

            if (problem.Resources != null && problem.Resources.Any())
            {
                var validResources = problem.Resources
                    .All(res => !string.IsNullOrEmpty(res.Name) &&
                        ((res.Type == ProblemResourceType.AuthorsSolution && res.File != null && res.File.ContentLength > 0) ||
                        (res.Type == ProblemResourceType.ProblemDescription && res.File != null && res.File.ContentLength > 0) ||
                        (res.Type == ProblemResourceType.Link && !string.IsNullOrEmpty(res.Link))));

                if (!validResources)
                {
                    this.ModelState.AddModelError("Resources", GlobalResource.Resources_not_complete);
                }
            }

            if (problem.AdditionalFiles != null && problem.AdditionalFiles.ContentLength != 0)
            {
                this.ValidateUploadedFile(nameof(problem.AdditionalFiles), problem.AdditionalFiles);
            }

            if (problem.Tests != null && problem.Tests.ContentLength != 0)
            {
                this.ValidateUploadedFile(nameof(problem.Tests), problem.Tests);
            }

            if (!this.IsValidProblem(problem) || !this.ModelState.IsValid)
            {
                this.AddCheckersToProblemViewModel(problem);
                return this.View(problem);
            }

            var newProblem = problem.GetEntityModel();
            newProblem.Checker = this.checkersData.GetByName(problem.Checker);

            problem.SubmissionTypes.ForEach(s =>
            {
                if (s.IsChecked && s.Id.HasValue)
                {
                    var submission = this.submissionTypesData.GetById(s.Id.Value);
                    newProblem.SubmissionTypes.Add(submission);
                }
            });

            if (problem.SolutionSkeletonData != null && problem.SolutionSkeletonData.Any())
            {
                newProblem.SolutionSkeleton = problem.SolutionSkeletonData;
            }

            if (problem.Resources != null && problem.Resources.Any())
            {
                this.AddResourcesToProblem(newProblem, problem.Resources);
            }

            if (problem.AdditionalFiles != null && problem.AdditionalFiles.ContentLength != 0)
            {
                newProblem.AdditionalFiles = problem.AdditionalFiles.ToByteArray();
            }

            if (problem.Tests != null && problem.Tests.ContentLength != 0)
            {
                try
                {
                    this.AddTestsToProblem(newProblem, problem.Tests);
                }
                catch (Exception ex)
                {
                    // TempData is not working with return this.View
                    var systemMessages = new SystemMessageCollection
                    {
                        new SystemMessage
                        {
                            Content = string.Format(GlobalResource.Tests_cannot_be_improrted, ex.Message),
                            Type = SystemMessageType.Error,
                            Importance = 0
                        }
                    };

                    this.ViewBag.SystemMessages = systemMessages;

                    this.AddCheckersToProblemViewModel(problem);

                    return this.View(problem);
                }
            }

            if (newProblem.ProblemGroupId == default(int))
            {
                newProblem.ProblemGroup = new ProblemGroup
                {
                    ContestId = contest.Id,
                    OrderBy = newProblem.OrderBy,
                    Type = ((ProblemGroupType?)problem.ProblemGroupType).GetValidTypeOrNull()
                };
            }

            this.problemsData.Add(newProblem);

            this.TempData.AddInfoMessage(GlobalResource.Problem_added);
            return this.RedirectToAction("Problem", "Tests", new { newProblem.Id });
        }

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_problem);
                return this.RedirectToAction(c => c.Index());
            }

            if (!this.CheckIfUserHasProblemPermissions(id.Value))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            var selectedProblem = this.PrepareProblemViewModelForEdit(id.Value);

            if (selectedProblem == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_problem);
                return this.RedirectToAction(c => c.Index());
            }

            this.submissionTypesData
                .GetAll()
                .Select(SubmissionTypeViewModel.ViewModel)
                .ForEach(SubmissionTypeViewModel.ApplySelectedTo(selectedProblem));

            return this.View(selectedProblem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, ViewModelType problem)
        {
            if (!this.CheckIfUserHasProblemPermissions(id))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            var existingProblem = this.problemsData.GetWithProblemGroupById(id);

            if (existingProblem == null)
            {
                this.TempData.Add(GlobalConstants.DangerMessage, GlobalResource.Problem_not_found);
                return this.RedirectToAction(c => c.Index());
            }

            if (problem == null)
            {
                problem = this.PrepareProblemViewModelForEdit(id);

                this.AddCheckersToProblemViewModel(problem);

                return this.View(problem);
            }

            if (problem.AdditionalFiles != null && problem.AdditionalFiles.ContentLength != 0)
            {
                this.ValidateUploadedFile(nameof(problem.AdditionalFiles), problem.AdditionalFiles);
            }

            if (!this.ModelState.IsValid)
            {
                problem = this.PrepareProblemViewModelForEdit(id);

                this.AddCheckersToProblemViewModel(problem);

                this.submissionTypesData
                    .GetAll()
                    .Select(SubmissionTypeViewModel.ViewModel)
                    .ForEach(SubmissionTypeViewModel.ApplySelectedTo(problem));

                return this.View(problem);
            }

            existingProblem = problem.GetEntityModel(existingProblem);
            existingProblem.Checker = this.checkersData.GetByName(problem.Checker);
            existingProblem.SolutionSkeleton = problem.SolutionSkeletonData;
            existingProblem.SubmissionTypes.Clear();
            existingProblem.ProblemGroup.Type = ((ProblemGroupType?)problem.ProblemGroupType).GetValidTypeOrNull();

            if (!existingProblem.ProblemGroup.Contest.IsOnline)
            {
                existingProblem.ProblemGroup.OrderBy = problem.OrderBy;
            }

            if (problem.AdditionalFiles != null && problem.AdditionalFiles.ContentLength != 0)
            {
                using (var archiveStream = new MemoryStream())
                {
                    problem.AdditionalFiles.InputStream.CopyTo(archiveStream);
                    existingProblem.AdditionalFiles = archiveStream.ToArray();
                }
            }

            problem.SubmissionTypes.ForEach(s =>
            {
                if (s.IsChecked && s.Id.HasValue)
                {
                    var submission = this.submissionTypesData.GetById(s.Id.Value);
                    existingProblem.SubmissionTypes.Add(submission);
                }
            });

            this.problemsData.Update(existingProblem);

            this.TempData.AddInfoMessage(GlobalResource.Problem_edited);
            return this.RedirectToAction(c => c.Index(existingProblem.ProblemGroup.ContestId));
        }

        [HttpGet]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_problem);
                return this.RedirectToAction(c => c.Index());
            }

            if (!this.CheckIfUserHasProblemPermissions(id.Value))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            var selectedProblem = this.problemsData
                .GetByIdQuery(id.Value)
                .Select(DeleteProblemViewModel.FromProblem)
                .FirstOrDefault();

            if (selectedProblem == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_problem);
                return this.RedirectToAction(c => c.Index());
            }

            if (this.contestsData.IsActiveById(selectedProblem.ContestId))
            {
                this.TempData.AddDangerMessage(GlobalResource.Active_contest_problems_permitted_for_deletion);
                return this.RedirectToAction(c => c.Index(selectedProblem.ContestId));
            }

            return this.View(selectedProblem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmDelete(int problemId)
        {
            if (!this.CheckIfUserHasProblemPermissions(problemId))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            var problem = this.problemsData.GetWithContestById(problemId);

            if (problem == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_problem);
                return this.RedirectToAction(c => c.Index());
            }

            if (problem.ProblemGroup.Contest.CanBeCompeted)
            {
                this.TempData.AddDangerMessage(GlobalResource.Active_contest_problems_permitted_for_deletion);
                return this.RedirectToAction(c => c.Index(problem.ProblemGroup.ContestId));
            }

            this.problemsBusiness.DeleteById(problemId);

            this.TempData.AddInfoMessage(GlobalResource.Problem_deleted);
            return this.RedirectToAction(c => c.Index(problem.ProblemGroup.ContestId));
        }

        [HttpGet]
        public ActionResult DeleteAll(int? id)
        {
            if (id == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_contest);
                return this.RedirectToAction(c => c.Index());
            }

            if (!this.CheckIfUserHasContestPermissions(id.Value))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            var contest = this.contestsData.GetById(id.Value);

            if (contest == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_contest);
                return this.RedirectToAction(c => c.Index());
            }

            if (contest.IsActive)
            {
                this.TempData.AddDangerMessage(GlobalResource.Active_contest_problems_permitted_for_deletion);
                return this.RedirectToAction(c => c.Index(id.Value));
            }

            var contestModel = new DeleteProblemsFromContestViewModel
            {
                Id = contest.Id,
                Name = contest.Name
            };

            return this.View(contestModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmDeleteAll(int contestId)
        {
            if (!this.CheckIfUserHasContestPermissions(contestId))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            var contest = this.contestsData.GetById(contestId);

            if (contest == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_contest);
                return this.RedirectToAction(c => c.Index());
            }

            if (contest.IsActive)
            {
                this.TempData.AddDangerMessage(GlobalResource.Active_contest_problems_permitted_for_deletion);
                return this.RedirectToAction(c => c.Index(contest.Id));
            }

            this.problemsBusiness.DeleteByContest(contestId);

            this.TempData.AddInfoMessage(GlobalResource.Problems_deleted);
            return this.RedirectToAction(c => c.Index(contestId));
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_problem);
                return this.RedirectToAction(c => c.Index());
            }

            var problem = this.problemsData
                .GetByIdQuery(id.Value)
                .Select(ViewModelType.FromProblem)
                .FirstOrDefault();

            if (problem == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_problem);
                return this.RedirectToAction(c => c.Index());
            }

            if (!this.CheckIfUserHasContestPermissions(problem.ContestId))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            return this.View(problem);
        }

        /// <summary>
        /// Creates a zip file with all additional files in the problem
        /// </summary>
        /// <param name="id">Problem id</param>
        /// <returns>Zip file containing all additional files</returns>
        public ActionResult DownloadAdditionalFiles(int id)
        {
            var problem = this.problemsData.GetById(id);

            if (problem == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Problem_not_found);
                return this.RedirectToAction(c => c.Index());
            }

            if (!this.CheckIfUserHasProblemPermissions(id))
            {
                this.TempData.AddDangerMessage(GeneralResource.No_privileges_message);
                return this.RedirectToHome();
            }

            var additionalFiles = problem.AdditionalFiles;
            var zipFileName = $"{problem.Name}_AdditionalFiles_{DateTime.Now}.{Constants.ZipFileExtension}";

            var stream = new MemoryStream();
            using (var additionalFilesStream = new MemoryStream(additionalFiles))
            {
                using (var zipFile = ZipFile.Read(additionalFilesStream))
                {
                    zipFile.Name = zipFileName;
                    zipFile.Save(stream);
                    stream.Position = 0;
                }
            }

            return this.File(stream, MediaTypeNames.Application.Zip, zipFileName);
        }

        [HttpGet]
        public ActionResult Retest(int? id)
        {
            if (id == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_problem);
                return this.RedirectToAction<ProblemsController>(c => c.Index());
            }

            var problem = this.problemsData
                .GetByIdQuery(id.Value)
                .Select(ProblemRetestViewModel.FromProblem)
                .FirstOrDefault();

            if (problem == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_problem);
                return this.RedirectToAction<ProblemsController>(c => c.Index());
            }

            if (!this.CheckIfUserHasContestPermissions(problem.ContestId))
            {
                this.TempData.AddDangerMessage(GeneralResource.No_privileges_message);
                return this.RedirectToAction<ProblemsController>(c => c.Index());
            }

            if (this.HttpContext.Request.UrlReferrer != null)
            {
                this.ViewBag.ReturnUrl = this.HttpContext.Request.UrlReferrer.AbsolutePath;
            }

            return this.View("RetestConfirmation", problem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Retest(ProblemRetestViewModel model)
        {
            if (model == null || !this.problemsData.ExistsById(model.Id))
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_problem);
                return this.RedirectToAction<ProblemsController>(c => c.Index());
            }

            if (!this.CheckIfUserHasContestPermissions(model.ContestId))
            {
                this.TempData.AddDangerMessage(GeneralResource.No_privileges_message);
                return this.RedirectToAction<ProblemsController>(c => c.Index());
            }

            this.problemsBusiness.RetestById(model.Id);

            this.TempData.AddInfoMessage(GlobalResource.Problem_retested);
            return this.RedirectToAction(c => c.Index(model.ContestId));
        }

        [HttpGet]
        public ActionResult GetSubmissions(int id)
        {
            if (!this.CheckIfUserHasProblemPermissions(id))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            this.ViewBag.SubmissionStatusData = DropdownViewModel.GetEnumValues<SubmissionStatus>();
            return this.PartialView("_SubmissionsGrid", id);
        }

        [HttpPost]
        public JsonResult ReadSubmissions([DataSourceRequest]DataSourceRequest request, int id)
        {
            if (!this.CheckIfUserHasProblemPermissions(id))
            {
                return this.Json("No premissions");
            }

            var submissions = this.submissionsData
                .GetAllByProblem(id)
                .Select(SubmissionAdministrationGridViewModel.ViewModel);

            return this.Json(submissions.ToDataSourceResult(request));
        }

        [HttpGet]
        public ActionResult GetResources(int id)
        {
            if (!this.CheckIfUserHasProblemPermissions(id))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            return this.PartialView("_ResourcesGrid", id);
        }

        [HttpPost]
        public ActionResult ReadResources([DataSourceRequest]DataSourceRequest request, int id)
        {
            if (!this.CheckIfUserHasProblemPermissions(id))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            var resources = this.problemResourcesData
                .GetByProblemQuery(id)
                .Select(ProblemResourceGridViewModel.FromResource);

            return this.Json(resources.ToDataSourceResult(request));
        }

        [HttpGet]
        public ActionResult ByContest(int id)
        {
            var result = this.GetData(id);
            return this.LargeJson(result);
        }

        [HttpGet]
        public FileResult ExportToExcel([DataSourceRequest] DataSourceRequest request, int contestId)
        {
            if (!this.CheckIfUserHasContestPermissions(contestId))
            {
                this.TempData.AddDangerMessage(GeneralResource.No_privileges_message);
                throw new UnauthorizedAccessException("No premissions");
            }

            return this.ExportToExcel(request, this.GetData(contestId));
        }

        // TODO: Transfer to ResourcesController
        public ActionResult AddResourceForm(int id)
        {
            // TODO: Add validation for Id
            var resourceViewModel = new ProblemResourceViewModel
            {
                Id = id,
                AllTypes = Enum.GetValues(typeof(ProblemResourceType)).Cast<ProblemResourceType>().Select(v => new SelectListItem
                {
                    Text = v.GetDescription(),
                    Value = ((int)v).ToString(CultureInfo.InvariantCulture)
                })
            };

            return this.PartialView("_ProblemResourceForm", resourceViewModel);
        }

        public ActionResult FullSolutionSkeleton(int id)
        {
            if (!this.CheckIfUserHasProblemPermissions(id))
            {
                return this.Json(GeneralResource.No_privileges_message);
            }

            var solutionSkeleton = this.problemsData
                .GetByIdQuery(id)
                .Select(p => p.SolutionSkeleton)
                .FirstOrDefault();

            return this.Content(solutionSkeleton.Decompress());
        }

        [AjaxOnly]
        public ActionResult CopyPartial(int id) =>
            this.PartialView("_CopyProblemToAnotherContest", new CopyProblemViewModel(id));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Copy(CopyProblemViewModel problem)
        {
            var contestId = problem.ContestId;
            var problemGroupId = problem.ProblemGroupId;

            if (!this.ModelState.IsValid)
            {
                return this.JsonValidation();
            }

            if (!this.problemsData.ExistsById(problem.Id))
            {
                return this.JsonError(GlobalResource.Invalid_problem);
            }

            if (!contestId.HasValue || !this.contestsData.ExistsById(contestId.Value))
            {
                return this.JsonError(GlobalResource.Invalid_contest);
            }

            if (!this.CheckIfUserHasContestPermissions(contestId.Value))
            {
                return this.JsonError(GeneralResource.No_privileges_message);
            }

            if (problemGroupId.HasValue &&
                !this.problemGroupsData.IsFromContestByIdAndContest(problemGroupId.Value, contestId.Value))
            {
                return this.JsonError(GlobalResource.Invalid_problem_group);
            }

            var result = this.problemsBusiness.CopyToContestByIdByContestAndProblemGroup(
                problem.Id,
                contestId.Value,
                problemGroupId);

            if (result.IsError)
            {
                return this.JsonError(result.Error);
            }

            return this.JsonSuccess(string.Format(
                GlobalResource.Copy_problem_success_message,
                this.problemsData.GetNameById(problem.Id),
                this.contestsData.GetNameById(contestId.Value)));
        }

        private IEnumerable GetData(int id)
        {
            if (!this.CheckIfUserHasContestPermissions(id))
            {
                return new List<ViewModelType>();
            }

            var result = this.problemsData
                .GetAllByContest(id)
                .OrderBy(x => x.OrderBy)
                .Select(ViewModelType.FromProblem);

            return result;
        }

        private void AddResourcesToProblem(Problem problem, IEnumerable<ProblemResourceViewModel> resources)
        {
            var orderCount = 0;

            foreach (var resource in resources)
            {
                if (!string.IsNullOrEmpty(resource.Name) && resource.Type == ProblemResourceType.Link && resource.Link != null)
                {
                    problem.Resources.Add(new ProblemResource
                    {
                        Name = resource.Name,
                        Type = resource.Type,
                        OrderBy = orderCount,
                        Link = resource.Link,
                    });

                    orderCount++;
                }
                else if (!string.IsNullOrEmpty(resource.Name) && resource.Type != ProblemResourceType.Link && resource.File != null)
                {
                    problem.Resources.Add(new ProblemResource
                    {
                        Name = resource.Name,
                        Type = resource.Type,
                        OrderBy = orderCount,
                        File = resource.File.InputStream.ToByteArray(),
                        FileExtension = resource.FileExtension
                    });

                    orderCount++;
                }
            }
        }

        private void AddTestsToProblem(Problem problem, HttpPostedFileBase testArchive)
        {
            using (var memory = new MemoryStream())
            {
                testArchive.InputStream.CopyTo(memory);
                memory.Position = 0;

                var parsedTests = ZippedTestsParser.Parse(memory);

                if (!ZippedTestsParser.AreTestsParsedCorrectly(parsedTests))
                {
                    throw new ArgumentException(GlobalResource.Invalid_tests);
                }

                ZippedTestsParser.AddTestsToProblem(problem, parsedTests);
            }
        }

        private void ValidateUploadedFile(string propertyName, HttpPostedFileBase file)
        {
            var extension = Path.GetExtension(file.FileName);

            if (extension != Constants.ZipFileExtension)
            {
                this.ModelState.AddModelError(propertyName, GlobalResource.Must_be_zip_file);
            }
        }

        private ViewModelType PrepareProblemViewModelForEdit(int id)
        {
            var problemEntity = this.problemsData.GetByIdQuery(id);

            var problem = problemEntity
                .Select(ViewModelType.FromProblem)
                .FirstOrDefault();

            var contest = problemEntity.FirstOrDefault()?.ProblemGroup.Contest;

            if (problem == null || contest == null)
            {
                return null;
            }

            this.AddCheckersAndProblemGroupsToProblemViewModel(
                problem,
                contest.ProblemGroups.Count,
                contest.IsOnline);

            return problem;
        }

        private ViewModelType PrepareProblemViewModelForCreate(Contest contest)
        {
            var problem = new ViewModelType
            {
                ContestId = contest.Id,
                ContestName = contest.Name,
                OrderBy = this.problemsData.GetNewOrderByContest(contest.Id)
            };

            this.AddCheckersAndProblemGroupsToProblemViewModel(
                problem,
                contest.ProblemGroups.Count,
                contest.IsOnline);

            problem.SubmissionTypes = this.submissionTypesData
                .GetAll()
                .Select(SubmissionTypeViewModel.ViewModel)
                .ToList();

            return problem;
        }

        private void AddCheckersAndProblemGroupsToProblemViewModel(
            ViewModelType problem,
            int numberOfProblemGroups,
            bool isOnlineContest)
        {
            problem.AvailableCheckers = this.checkersData
                .GetAll()
                .Select(checker => new SelectListItem
                {
                    Text = checker.Name,
                    Value = checker.Name,
                    Selected = checker.Name.Contains("Trim")
                });

            if (isOnlineContest && numberOfProblemGroups > 0)
            {
                this.ViewBag.ProblemGroupIdData = this.problemGroupsData
                    .GetAllByContest(problem.ContestId)
                    .OrderBy(pg => pg.OrderBy)
                    .Select(DropdownViewModel.FromProblemGroup);
            }

            this.ViewBag.ProblemGroupTypeData = DropdownViewModel.GetEnumValues<ProblemGroupType>();
        }

        private void AddCheckersToProblemViewModel(ViewModelType problem) =>
            problem.AvailableCheckers = this.checkersData
                .GetAll()
                .Select(checker => new SelectListItem
                {
                    Text = checker.Name,
                    Value = checker.Name
                });

        private bool IsValidProblem(ViewModelType model)
        {
            var isValid = true;

            if (model.SubmissionTypes == null || !model.SubmissionTypes.Any(s => s.IsChecked))
            {
                this.ModelState.AddModelError(nameof(model.SelectedSubmissionTypes), GlobalResource.Select_one_submission_type);
                isValid = false;
            }

            return isValid;
        }
    }
}