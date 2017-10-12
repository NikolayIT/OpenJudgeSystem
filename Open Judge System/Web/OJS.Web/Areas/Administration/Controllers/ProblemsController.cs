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

    using EntityFramework.Extensions;

    using Ionic.Zip;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using MissingFeatures;

    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Areas.Administration.ViewModels.Contest;
    using OJS.Web.Areas.Administration.ViewModels.Problem;
    using OJS.Web.Areas.Administration.ViewModels.ProblemResource;
    using OJS.Web.Areas.Administration.ViewModels.Submission;
    using OJS.Web.Areas.Administration.ViewModels.SubmissionType;
    using OJS.Web.Common;
    using OJS.Web.Common.Extensions;
    using OJS.Web.Common.ZippedTestManipulator;
    using OJS.Web.Controllers;
    using OJS.Web.ViewModels.Common;

    using GlobalResource = Resources.Areas.Administration.Problems.ProblemsControllers;
    using TransactionScope = System.Transactions.TransactionScope;

    public class ProblemsController : LecturerBaseController
    {
        public ProblemsController(IOjsData data)
            : base(data)
        {
        }

        public ActionResult Index()
        {
            return this.View();
        }

        public ActionResult Contest(int? id)
        {
            if (id == null || !this.CheckIfUserHasContestPermissions(id.Value))
            {
                this.TempData.AddDangerMessage(GlobalConstants.NoPrivilegesMessage);
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
            }

            this.ViewBag.ContestId = id;

            return this.View(GlobalConstants.Index);
        }

        public ActionResult Resource(int? id)
        {
            if (id == null || !this.CheckIfUserHasProblemPermissions(id.Value))
            {
                this.TempData.AddDangerMessage(GlobalConstants.NoPrivilegesMessage);
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
            }

            var problem = this.Data.Problems
                .All()
                .FirstOrDefault(pr => pr.Id == id);

            if (problem == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_problem);
                return this.RedirectToAction(nameof(this.Index));
            }

            this.ViewBag.ContestId = problem.ContestId;
            this.ViewBag.ProblemId = problem.Id;

            return this.View(nameof(this.Index));
        }

        [HttpGet]
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_contest);
                return this.RedirectToAction(nameof(this.Index));
            }

            if (!this.CheckIfUserHasContestPermissions(id.Value))
            {
                this.TempData.AddDangerMessage(GlobalConstants.NoPrivilegesMessage);
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
            }

            var contest = this.Data.Contests.All().FirstOrDefault(x => x.Id == id);

            if (contest == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_contest);
                return this.RedirectToAction(nameof(this.Index));
            }

            var problem = this.PrepareProblemViewModelForCreate(contest);
            return this.View(problem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int id, DetailedProblemViewModel problem)
        {
            if (!this.CheckIfUserHasContestPermissions(id))
            {
                this.TempData.AddDangerMessage(GlobalConstants.NoPrivilegesMessage);
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
            }

            var contest = this.Data.Contests.All().FirstOrDefault(x => x.Id == id);
            if (contest == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_contest);
                return this.RedirectToAction(nameof(this.Index));
            }

            if (problem == null)
            {
                problem = this.PrepareProblemViewModelForCreate(contest);
                problem.AvailableCheckers = this.Data.Checkers.All()
                .Select(checker => new SelectListItem
                {
                    Text = checker.Name,
                    Value = checker.Name
                });
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
                problem.AvailableCheckers = this.Data.Checkers.All()
                    .Select(checker => new SelectListItem { Text = checker.Name, Value = checker.Name });
                return this.View(problem);
            }

            var newProblem = problem.GetEntityModel();
            newProblem.Checker = this.Data.Checkers.All().FirstOrDefault(x => x.Name == problem.Checker);

            problem.SubmissionTypes.ForEach(s =>
            {
                if (s.IsChecked)
                {
                    var submission = this.Data.SubmissionTypes.All().FirstOrDefault(t => t.Id == s.Id);
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
                                        Content = ex.Message,
                                        Type = SystemMessageType.Error,
                                        Importance = 0
                                    }
                                };
                    this.ViewBag.SystemMessages = systemMessages;
                    problem.AvailableCheckers = this.Data.Checkers.All()
                        .Select(checker => new SelectListItem
                        {
                            Text = checker.Name,
                            Value = checker.Name
                        });
                    return this.View(problem);
                }
            }

            this.Data.Problems.Add(newProblem);
            this.Data.SaveChanges();

            this.TempData.AddInfoMessage(GlobalResource.Problem_added);
            return this.RedirectToAction("Problem", "Tests", new { newProblem.Id });
        }

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_problem);
                return this.RedirectToAction(nameof(this.Index));
            }

            if (!this.CheckIfUserHasProblemPermissions(id.Value))
            {
                this.TempData.AddDangerMessage(GlobalConstants.NoPrivilegesMessage);
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
            }

            var selectedProblem = this.PrepareProblemViewModelForEdit(id.Value);

            if (selectedProblem == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_problem);
                return this.RedirectToAction(nameof(this.Index));
            }

            this.Data.SubmissionTypes.All()
                .Select(SubmissionTypeViewModel.ViewModel)
                .ForEach(SubmissionTypeViewModel.ApplySelectedTo(selectedProblem));

            var checkers = this.Data.Checkers
                .All()
                .AsQueryable()
                .Select(checker => new SelectListItem { Text = checker.Name, Value = checker.Name });

            selectedProblem.AvailableCheckers = checkers;

            return this.View(selectedProblem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, DetailedProblemViewModel problem)
        {
            if (!this.CheckIfUserHasProblemPermissions(id))
            {
                this.TempData.AddDangerMessage(GlobalConstants.NoPrivilegesMessage);
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
            }

            var existingProblem = this.Data.Problems.All().FirstOrDefault(x => x.Id == id);

            if (existingProblem == null)
            {
                this.TempData.Add(GlobalConstants.DangerMessage, GlobalResource.Problem_not_found);
                return this.RedirectToAction(nameof(this.Index));
            }

            if (problem == null)
            {
                problem = this.PrepareProblemViewModelForEdit(id);
                problem.AvailableCheckers = this.Data.Checkers.All()
                .Select(checker => new SelectListItem
                {
                    Text = checker.Name,
                    Value = checker.Name
                });
                return this.View(problem);
            }

            if (problem.AdditionalFiles != null && problem.AdditionalFiles.ContentLength != 0)
            {
                this.ValidateUploadedFile(nameof(problem.AdditionalFiles), problem.AdditionalFiles);
            }

            if (!this.ModelState.IsValid)
            {
                problem = this.PrepareProblemViewModelForEdit(id);
                problem.AvailableCheckers = this.Data.Checkers.All()
                .Select(checker => new SelectListItem
                {
                    Text = checker.Name,
                    Value = checker.Name
                });
                this.Data.SubmissionTypes.All()
                    .Select(SubmissionTypeViewModel.ViewModel)
                    .ForEach(SubmissionTypeViewModel.ApplySelectedTo(problem));
                return this.View(problem);
            }

            existingProblem = problem.GetEntityModel(existingProblem);
            existingProblem.Checker = this.Data.Checkers.All().FirstOrDefault(x => x.Name == problem.Checker);
            existingProblem.SolutionSkeleton = problem.SolutionSkeletonData;
            existingProblem.SubmissionTypes.Clear();

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
                if (s.IsChecked)
                {
                    var submission = this.Data.SubmissionTypes.All().FirstOrDefault(t => t.Id == s.Id);
                    existingProblem.SubmissionTypes.Add(submission);
                }
            });

            this.Data.Problems.Update(existingProblem);
            this.Data.SaveChanges();

            this.TempData.AddInfoMessage(GlobalResource.Problem_edited);
            return this.RedirectToAction("Contest", new { id = existingProblem.ContestId });
        }

        [HttpGet]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_problem);
                return this.RedirectToAction(nameof(this.Index));
            }

            if (!this.CheckIfUserHasProblemPermissions(id.Value))
            {
                this.TempData.AddDangerMessage(GlobalConstants.NoPrivilegesMessage);
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
            }

            var selectedProblem = this.Data.Problems.All()
                .Where(x => x.Id == id)
                .Select(DetailedProblemViewModel.FromProblem)
                .FirstOrDefault();

            if (selectedProblem == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_problem);
                return this.RedirectToAction(nameof(this.Index));
            }

            return this.View(selectedProblem);
        }

        public ActionResult ConfirmDelete(int? id)
        {
            if (id == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_problem);
                return this.RedirectToAction(nameof(this.Index));
            }

            var problem = this.Data.Problems.All().FirstOrDefault(x => x.Id == id);

            if (problem == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_problem);
                return this.RedirectToAction(nameof(this.Index));
            }

            this.Data.Resources.Delete(r => r.ProblemId == id);

            this.Data.TestRuns.Delete(tr => tr.Submission.ProblemId == id);

            this.Data.Tests.Delete(t => t.ProblemId == id);

            this.Data.Submissions.Delete(s => s.ProblemId == id);

            this.Data.Problems.Delete(id.Value);

            this.Data.SaveChanges();

            this.TempData.AddInfoMessage(GlobalResource.Problem_deleted);
            return this.RedirectToAction("Contest", new { id = problem.ContestId });
        }

        [HttpGet]
        public ActionResult DeleteAll(int? id)
        {
            if (id == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_contest);
                return this.RedirectToAction(nameof(this.Index));
            }

            if (!this.CheckIfUserHasContestPermissions(id.Value))
            {
                this.TempData.AddDangerMessage(GlobalConstants.NoPrivilegesMessage);
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
            }

            var contest = this.Data.Contests.All()
                .Where(x => x.Id == id)
                .Select(ContestAdministrationViewModel.ViewModel)
                .FirstOrDefault();

            if (contest == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_contest);
                return this.RedirectToAction(nameof(this.Index));
            }

            return this.View(contest);
        }

        public ActionResult ConfirmDeleteAll(int? id)
        {
            if (id == null || !this.Data.Contests.All().Any(x => x.Id == id))
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_contest);
                return this.RedirectToAction<ProblemsController>(x => x.Index());
            }

            if (!this.CheckIfUserHasContestPermissions(id.Value))
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_contest);
                return this.RedirectToAction(nameof(this.Index));
            }

            this.Data.Resources.Delete(r => r.Problem.ContestId == id);

            this.Data.TestRuns.Delete(tr => tr.Submission.Problem.ContestId == id);

            this.Data.Tests.Delete(t => t.Problem.ContestId == id);

            this.Data.Submissions.Delete(s => s.Problem.ContestId == id);

            this.Data.Problems.Delete(p => p.ContestId == id);

            this.Data.SaveChanges();

            this.TempData.AddInfoMessage(GlobalResource.Problems_deleted);
            return this.RedirectToAction("Contest", new { id });
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_problem);
                return this.RedirectToAction(nameof(this.Index));
            }

            var problem = this.Data.Problems.All()
                .Where(pr => pr.Id == id)
                .Select(DetailedProblemViewModel.FromProblem)
                .FirstOrDefault();

            if (problem == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_problem);
                return this.RedirectToAction(nameof(this.Index));
            }

            if (!this.CheckIfUserHasContestPermissions(problem.ContestId))
            {
                this.TempData.AddDangerMessage(GlobalConstants.NoPrivilegesMessage);
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
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
            var problem = this.Data.Problems.GetById(id);

            if (problem == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Problem_not_found);
                return this.RedirectToAction(nameof(this.Index));
            }

            if (!this.CheckIfUserHasProblemPermissions(id))
            {
                this.TempData.AddDangerMessage(GlobalConstants.NoPrivilegesMessage);
                return this.RedirectToAction<HomeController>(c => c.Index(), new { area = string.Empty });
            }

            var additionalFiles = problem.AdditionalFiles;
            var zipFileName = $"{problem.Name}_AdditionalFiles_{DateTime.Now}.{GlobalConstants.ZipFileExtension}";

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
                return this.RedirectToAction<ContestsController>(c => c.Index());
            }

            var problem = this.Data.Problems
                .All()
                .Select(ProblemRetestViewModel.FromProblem)
                .FirstOrDefault(pr => pr.Id == id);

            if (problem == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_problem);
                return this.RedirectToAction<ProblemsController>(c => c.Index());
            }

            if (!this.CheckIfUserHasContestPermissions(problem.ContestId))
            {
                this.TempData.AddDangerMessage(GlobalConstants.NoPrivilegesMessage);
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
            if (!this.CheckIfUserHasContestPermissions(model.ContestId))
            {
                this.TempData.AddDangerMessage(GlobalConstants.NoPrivilegesMessage);
                return this.RedirectToAction<ProblemsController>(c => c.Index());
            }

            var problem = this.Data.Problems.GetById(model.Id);

            if (problem == null)
            {
                this.TempData.AddDangerMessage(GlobalResource.Invalid_problem);
                return this.RedirectToAction<ProblemsController>(c => c.Index());
            }

            var submissionIds = problem
                .Submissions
                .Where(s => !s.IsDeleted)
                .Select(s => s.Id)
                .AsEnumerable();

            using (var scope = new TransactionScope())
            {
                this.Data.ParticipantScores.DeleteParticipantScores(model.Id);
                this.Data.SaveChanges();

                this.Data.Context.Submissions
                    .Where(s => !s.IsDeleted && s.ProblemId == problem.Id)
                    .Update(x => new Submission { Processed = false });

                this.Data.SubmissionsForProcessing.AddOrUpdate(submissionIds);

                scope.Complete();
            }

            this.TempData.AddInfoMessage(GlobalResource.Problem_retested);
            return this.RedirectToAction("Contest", new { id = problem.ContestId });
        }

        [HttpGet]
        public ActionResult GetSubmissions(int id)
        {
            if (!this.CheckIfUserHasProblemPermissions(id))
            {
                this.TempData.AddDangerMessage(GlobalConstants.NoPrivilegesMessage);
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
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

            var submissions = this.Data.Submissions
                .All()
                .Where(s => s.ProblemId == id)
                .Select(SubmissionAdministrationGridViewModel.ViewModel);

            return this.Json(submissions.ToDataSourceResult(request));
        }

        [HttpGet]
        public ActionResult GetResources(int id)
        {
            if (!this.CheckIfUserHasProblemPermissions(id))
            {
                this.TempData.AddDangerMessage(GlobalConstants.NoPrivilegesMessage);
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
            }

            return this.PartialView("_ResourcesGrid", id);
        }

        [HttpPost]
        public ActionResult ReadResources([DataSourceRequest]DataSourceRequest request, int id)
        {
            if (!this.CheckIfUserHasProblemPermissions(id))
            {
                this.TempData.AddDangerMessage(GlobalConstants.NoPrivilegesMessage);
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
            }

            var resources = this.Data.Resources
                .All()
                .Where(r => r.ProblemId == id)
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
        public JsonResult GetCascadeCategories()
        {
            var result = this.Data.ContestCategories.All().Select(x => new { x.Id, x.Name });

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetCascadeContests(string categories)
        {
            var contests = this.Data.Contests.All();

            int categoryId;

            if (int.TryParse(categories, out categoryId))
            {
                contests = contests.Where(x => x.CategoryId == categoryId);
            }

            var result = contests.Select(x => new { x.Id, x.Name });

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetSearchedContests()
        {
            var result = this.Data.Contests.All().Select(x => new { x.Id, x.Name });
            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetContestInformation(string id)
        {
            // TODO: Add validation for Id
            var contestIdNumber = int.Parse(id);

            if (!this.CheckIfUserHasContestPermissions(contestIdNumber))
            {
                this.TempData.AddDangerMessage(GlobalConstants.NoPrivilegesMessage);
                return this.Json("No premissions");
            }

            var contest = this.Data.Contests.All().FirstOrDefault(x => x.Id == contestIdNumber);

            var contestId = contestIdNumber;
            var categoryId = contest.CategoryId;

            var result = new { contest = contestId, category = categoryId };
            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public FileResult ExportToExcel([DataSourceRequest] DataSourceRequest request, int contestId)
        {
            if (!this.CheckIfUserHasContestPermissions(contestId))
            {
                this.TempData.AddDangerMessage(GlobalConstants.NoPrivilegesMessage);
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
                return this.Json(GlobalConstants.NoPrivilegesMessage);
            }

            var solutionSkeleton = this.Data.Problems
                .All()
                .Where(p => p.Id == id)
                .Select(p => p.SolutionSkeleton)
                .FirstOrDefault();

            return this.Content(solutionSkeleton.Decompress());
        }

        private IEnumerable GetData(int id)
        {
            if (!this.CheckIfUserHasContestPermissions(id))
            {
                return new List<DetailedProblemViewModel>();
            }

            var result = this.Data.Problems.All()
                .Where(x => x.ContestId == id)
                .OrderBy(x => x.OrderBy)
                .Select(DetailedProblemViewModel.FromProblem);

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

                if (parsedTests.ZeroInputs.Count != parsedTests.ZeroOutputs.Count ||
                    parsedTests.Inputs.Count != parsedTests.Outputs.Count)
                {
                    throw new ArgumentException(GlobalResource.Invalid_tests);
                }

                ZippedTestsParser.AddTestsToProblem(problem, parsedTests);
            }
        }

        private void ValidateUploadedFile(string propertyName, HttpPostedFileBase file)
        {
            var extension = Path.GetExtension(file.FileName);

            if (extension != GlobalConstants.ZipFileExtension)
            {
                this.ModelState.AddModelError(propertyName, GlobalResource.Must_be_zip_file);
            }
        }

        private DetailedProblemViewModel PrepareProblemViewModelForEdit(int id)
        {
            return this.Data.Problems.All()
                .Where(x => x.Id == id)
                .Select(DetailedProblemViewModel.FromProblem)
                .FirstOrDefault();
        }

        private DetailedProblemViewModel PrepareProblemViewModelForCreate(Contest contest)
        {
            var problemOrder = GlobalConstants.ProblemDefaultOrderBy;
            var lastProblem = this.Data.Problems
                .All()
                .Where(x => x.ContestId == contest.Id)
                .OrderByDescending(x => x.OrderBy)
                .FirstOrDefault();

            if (lastProblem != null)
            {
                problemOrder = lastProblem.OrderBy + 1;
            }

            var problem = new DetailedProblemViewModel();
            problem.OrderBy = problemOrder;
            problem.ContestId = contest.Id;
            problem.ContestName = contest.Name;
            problem.AvailableCheckers = this.Data.Checkers.All()
                .Select(checker => new SelectListItem
                {
                    Text = checker.Name,
                    Value = checker.Name,
                    Selected = checker.Name.Contains("Trim")
                });
            problem.SubmissionTypes = this.Data.SubmissionTypes.All().Select(SubmissionTypeViewModel.ViewModel).ToList();

            return problem;
        }

        private bool IsValidProblem(DetailedProblemViewModel model)
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