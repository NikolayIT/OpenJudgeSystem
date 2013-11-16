namespace OJS.Web.Areas.Administration.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using OJS.Common.Extensions;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels;
    using OJS.Web.Common.ZippedTestManipulator;
    using OJS.Web.Controllers;
    using OJS.Web.Areas.Administration.ViewModels.Contest;

    public class ProblemsController : AdministrationController
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
            this.ViewBag.ContestId = id;

            return this.View("Index");
        }

        public ActionResult Resource(int? id)
        {
            var problem = this.Data.Problems
                .All()
                .FirstOrDefault(pr => pr.Id == id);

            if (problem == null)
            {
                this.TempData["DangerMessage"] = "Невалидна задача";
                return this.RedirectToAction("Index");
            }

            this.ViewBag.ContestId = problem.ContestId;
            this.ViewBag.ProblemId = problem.Id;

            return this.View("Index");
        }

        [HttpGet]
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                this.TempData["DangerMessage"] = "Невалидно състезание";
                return this.RedirectToAction("Index");
            }

            var contest = this.Data.Contests.All().FirstOrDefault(x => x.Id == id);

            if (contest == null)
            {
                this.TempData["DangerMessage"] = "Невалидно състезание";
                return this.RedirectToAction("Index");
            }

            var checkers = this.Data.Checkers.All()
                .Select(x => x.Name);

            var lastOrderBy = -1;
            var lastProblem = this.Data.Problems.All().Where(x => x.ContestId == id);

            if (lastProblem.Count() > 0)
            {
                lastOrderBy = lastProblem.Max(x => x.OrderBy);
            }

            var problem = new DetailedProblemViewModel
            {
                Name = "Име",
                MaximumPoints = 100,
                TimeLimit = 1000,
                MemoryLimit = 16777216,
                AvailableCheckers = this.Data.Checkers.All().Select(checker => new SelectListItem { Text = checker.Name, Value = checker.Name }),
                OrderBy = lastOrderBy + 1,
                ContestId = contest.Id,
                ContestName = contest.Name,
            };

            return this.View(problem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int id, HttpPostedFileBase testArchive, DetailedProblemViewModel problem)
        {
            if (problem != null && ModelState.IsValid)
            {
                var newProblem = new Problem
                {
                    Name = problem.Name,
                    ContestId = id,
                    MaximumPoints = problem.MaximumPoints,
                    MemoryLimit = problem.MemoryLimit,
                    TimeLimit = problem.TimeLimit,
                    SourceCodeSizeLimit = problem.SourceCodeSizeLimit,
                    OrderBy = problem.OrderBy,
                    Checker = this.Data.Checkers.All().Where(x => x.Name == problem.Checker).FirstOrDefault()
                };

                if (problem.Resources != null && problem.Resources.Count() > 0)
                {
                    this.AddResourcesToProblem(newProblem, problem.Resources);
                }

                if (testArchive != null && testArchive.ContentLength != 0)
                {
                    try
                    {
                        this.AddTestsToProblem(newProblem, testArchive);
                    }
                    catch (Exception ex)
                    {
                        TempData.Add("DangerMessage", ex.Message);
                        problem.AvailableCheckers = this.Data.Checkers.All().Select(checker => new SelectListItem { Text = checker.Name, Value = checker.Name });
                        return this.View(problem);
                    }
                }

                this.Data.Problems.Add(newProblem);
                this.Data.SaveChanges();

                TempData.Add("InfoMessage", "Задачата беше добавена успешно");
                return this.RedirectToAction("Contest", new { id = id });
            }

            problem.AvailableCheckers = this.Data.Checkers.All().Select(checker => new SelectListItem { Text = checker.Name, Value = checker.Name });
            return this.View(problem);
        }

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                this.TempData["DangerMessage"] = "Невалидна задача";
                return this.RedirectToAction("Index");
            }

            // TODO: Fix this query to use the static method from DetailedProblemViewModel

            var selectedProblem = this.Data.Problems.All()
                .Where(x => x.Id == id)
                .Select(problem => new DetailedProblemViewModel
                {
                    Id = problem.Id,
                    Name = problem.Name,
                    ContestId = problem.ContestId,
                    ContestName = problem.Contest.Name,
                    TrialTests = problem.Tests.AsQueryable().Where(x => x.IsTrialTest).Count(),
                    CompeteTests = problem.Tests.AsQueryable().Where(x => !x.IsTrialTest).Count(),
                    MaximumPoints = problem.MaximumPoints,
                    TimeLimit = problem.TimeLimit,
                    MemoryLimit = problem.MemoryLimit,
                    SourceCodeSizeLimit = problem.SourceCodeSizeLimit,
                    Checker = problem.Checker.Name,
                    OrderBy = problem.OrderBy
                })
                .FirstOrDefault();

            var checkers = this.Data.Checkers
                .All()
                .AsQueryable()
                .Select(checker => new SelectListItem { Text = checker.Name, Value = checker.Name });

            selectedProblem.AvailableCheckers = checkers;

            if (selectedProblem == null)
            {
                this.TempData["DangerMessage"] = "Невалидна задача";
                return this.RedirectToAction("Index");
            }

            return this.View(selectedProblem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, DetailedProblemViewModel problem)
        {
            // TODO: Add validation with ModelState.IsValid

            var existingProblem = this.Data.Problems.All()
                .FirstOrDefault(x => x.Id == id);

            existingProblem.Name = problem.Name;
            existingProblem.MaximumPoints = problem.MaximumPoints;
            existingProblem.TimeLimit = problem.TimeLimit;
            existingProblem.MemoryLimit = problem.MemoryLimit;
            existingProblem.SourceCodeSizeLimit = problem.SourceCodeSizeLimit;
            existingProblem.Checker = this.Data.Checkers.All().FirstOrDefault(x => x.Name == problem.Checker);
            existingProblem.OrderBy = problem.OrderBy;

            this.Data.SaveChanges();

            this.TempData["InfoMessage"] = "Задачата беше променена успешно";
            return this.RedirectToAction("Contest", new { id = existingProblem.ContestId });
        }

        [HttpGet]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                this.TempData["DangerMessage"] = "Невалидна задача";
                return this.RedirectToAction("Index");
            }

            var selectedProblem = this.Data.Problems.All()
                .Where(x => x.Id == id)
                .Select(problem => new DetailedProblemViewModel
                {
                    Id = problem.Id,
                    Name = problem.Name,
                    ContestId = problem.ContestId,
                    ContestName = problem.Contest.Name,
                    TrialTests = problem.Tests.AsQueryable().Where(x => x.IsTrialTest).Count(),
                    CompeteTests = problem.Tests.AsQueryable().Where(x => !x.IsTrialTest).Count(),
                    MaximumPoints = problem.MaximumPoints,
                    TimeLimit = problem.TimeLimit,
                    MemoryLimit = problem.MemoryLimit,
                    SourceCodeSizeLimit = problem.SourceCodeSizeLimit,
                    Checker = problem.Checker.Name,
                    OrderBy = problem.OrderBy
                })
                .FirstOrDefault();

            if (selectedProblem == null)
            {
                this.TempData["DangerMessage"] = "Невалидна задача";
                return this.RedirectToAction("Index");
            }

            return this.View(selectedProblem);
        }

        public ActionResult ConfirmDelete(int? id)
        {
            if (id == null)
            {
                this.TempData["DangerMessage"] = "Невалидна задача";
                return this.RedirectToAction("Index");
            }

            var problem = this.Data.Problems.All()
                .FirstOrDefault(x => x.Id == id);

            if (problem == null)
            {
                this.TempData["DangerMessage"] = "Невалидна задача";
                return this.RedirectToAction("Index");
            }

            // TODO: Add cascading deletion of submissions, tests, resources

            this.Data.Problems.Delete(id.Value);
            this.Data.SaveChanges();

            this.TempData["InfoMessage"] = "Задачата беше изтрита успешно";
            return this.RedirectToAction("Contest", new { id = problem.ContestId });
        }

        [HttpGet]
        public ActionResult DeleteAll(int? id)
        {
            if (id == null)
            {
                this.TempData["DangerMessage"] = "Невалидно състезание";
                return this.RedirectToAction("Index");
            }

            var contest = this.Data.Contests.All()
                .Where(x => x.Id == id)
                .Select(ContestAdministrationViewModel.ViewModel)
                .FirstOrDefault();

            if (contest == null)
            {
                this.TempData["DangerMessage"] = "Невалидно състезание";
                return this.RedirectToAction("Index");
            }

            return this.View(contest);
        }

        public ActionResult ConfirmDeleteAll(int? id)
        {
            if (id == null)
            {
                this.TempData["DangerMessage"] = "Невалидно състезание";
                return this.RedirectToAction("Index");
            }

            var contest = this.Data.Contests.All()
                .FirstOrDefault(x => x.Id == id);

            if (contest == null)
            {
                this.TempData["DangerMessage"] = "Невалидно състезание";
                return this.RedirectToAction("Index");
            }

            foreach (var problem in contest.Problems.ToList())
            {
                // TODO: Add cascading deletion of submissions, tests, resources

                this.Data.Problems.Delete(problem.Id);
            }

            this.Data.SaveChanges();

            this.TempData["InfoMessage"] = "Задачите бяха изтрити успешно";
            return this.RedirectToAction("Contest", new { id = id });
        }

        [HttpGet]
        public JsonResult ByContest(int id)
        {
            // TODO: Select should use the static method from DetailedProblemViewModel

            var result = this.Data.Problems.All()
                .Where(x => x.ContestId == id)
                .OrderBy(x => x.OrderBy)
                .Select(problem => new DetailedProblemViewModel
                {
                    Id = problem.Id,
                    Name = problem.Name,
                    ContestName = problem.Contest.Name,
                    TrialTests = problem.Tests.AsQueryable().Where(x => x.IsTrialTest).Count(),
                    CompeteTests = problem.Tests.AsQueryable().Where(x => !x.IsTrialTest).Count(),
                    MaximumPoints = problem.MaximumPoints,
                    TimeLimit = problem.TimeLimit,
                    MemoryLimit = problem.MemoryLimit,
                    Checker = problem.Checker.Name,
                    OrderBy = problem.OrderBy,
                });

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetCascadeCategories()
        {
            var result = this.Data.ContestCategories.All().Select(x => new { Id = x.Id, Name = x.Name });

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

            var result = contests.Select(x => new { Id = x.Id, Name = x.Name });

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetSearchedContests()
        {
            var result = this.Data.Contests.All().Select(x => new { Id = x.Id, Name = x.Name });
            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetContestInformation(string id)
        {
            // TODO: Add validation for Id

            var contestIdNumber = int.Parse(id);

            var contest = this.Data.Contests.All().FirstOrDefault(x => x.Id == contestIdNumber);

            var contestId = contestIdNumber;

            var categoryId = contest.CategoryId;

            var result = new { contest = contestId, category = categoryId };

            return this.Json(result, JsonRequestBehavior.AllowGet);
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
                    Value = ((int)v).ToString()
                })
            };

            return this.PartialView("_ProblemResourceForm", resourceViewModel);
        }

        private void AddResourcesToProblem(Problem problem, IEnumerable<ProblemResourceViewModel> resources)
        {
            var orderCount = 0;

            foreach (var resource in resources)
            {
                if (!string.IsNullOrEmpty(resource.Name) && resource.Type == ProblemResourceType.Video && resource.Link != null)
                {
                    problem.Resources.Add(new ProblemResource
                    {
                        Name = resource.Name,
                        Type = resource.Type,
                        OrderBy = orderCount,
                        Link = resource.Link,
                    });

                    orderCount++;
                    continue;
                }
                else if (!string.IsNullOrEmpty(resource.Name) && resource.Type != ProblemResourceType.Video && resource.File != null)
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
                    continue;
                }
            }
        }

        private void AddTestsToProblem(Problem problem, HttpPostedFileBase testArchive)
        {
            var extension = testArchive.FileName.Substring(testArchive.FileName.Length - 4, 4);

            if (extension != ".zip")
            {
                throw new ArgumentException("Тестовете трябва да бъдат в .ZIP файл");
            }

            using (var memory = new MemoryStream())
            {
                testArchive.InputStream.CopyTo(memory);
                memory.Position = 0;

                var parsedTests = new TestsParseResult();

                parsedTests = ZippedTestsManipulator.Parse(memory);

                if (parsedTests.ZeroInputs.Count != parsedTests.ZeroOutputs.Count || parsedTests.Inputs.Count != parsedTests.Outputs.Count)
                {
                    throw new ArgumentException("Невалидни тестове");
                }

                ZippedTestsManipulator.AddTestsToProblem(problem, parsedTests);
            }
        }
    }
}