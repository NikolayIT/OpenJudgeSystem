namespace OJS.Web.Areas.Administration.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Mime;
    using System.Web;
    using System.Web.Mvc;

    using Ionic.Zip;

    using Kendo.Mvc.UI;

    using OJS.Common;
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Problem;
    using OJS.Web.Areas.Administration.ViewModels.Test;
    using OJS.Web.Areas.Administration.ViewModels.TestRun;
    using OJS.Web.Common.ZippedTestManipulator;
    using OJS.Web.Controllers;

    /// <summary>
    /// Controller class for administrating problems' input and output tests, inherits Administration controller for authorisation
    /// </summary>
    public class TestsController : AdministrationController
    {
        /// <summary>
        /// Instantiates the controller with database context as data
        /// </summary>
        /// <param name="data">Open Judge System Database context for the controller to work with</param>
        public TestsController(IOjsData data)
            : base(data)
        {
        }

        /// <summary>
        /// Returns view for the tests administration index page
        /// </summary>
        /// <returns>View for /Administration/Tests/</returns>
        public ActionResult Index()
        {
            return this.View();
        }

        /// <summary>
        /// Returns view for the tests administration index page and populates problem, contest, category dropdowns and tests grid if problem id is correct
        /// </summary>
        /// <param name="id">Problem id which tests are populated</param>
        /// <returns>View for /Administration/Tests/Problem/{id}</returns>
        public ActionResult Problem(int? id)
        {
            this.ViewBag.ProblemId = id;

            return this.View(GlobalConstants.Index);
        }

        /// <summary>
        /// Returns view for the tests administration create page with HTTP Get request - creating tests for a problem selected by id
        /// </summary>
        /// <param name="id">Problem id for which a test will be created</param>
        /// <returns>View for /Administration/Tests/Create/ if problem id is correct otherwise redirects to /Administration/Tests/ with proper error message</returns>
        [HttpGet]
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                this.TempData[GlobalConstants.DangerMessage] = "Невалидна задача";
                return this.RedirectToAction(GlobalConstants.Index);
            }

            var problem = this.Data.Problems.All().FirstOrDefault(pr => pr.Id == id);

            if (problem == null)
            {
                this.TempData[GlobalConstants.DangerMessage] = "Невалидна задача";
                return this.RedirectToAction(GlobalConstants.Index);
            }

            var test = new TestViewModel
            {
                ProblemId = problem.Id,
                ProblemName = problem.Name,
            };

            return this.View(test);
        }

        /// <summary>
        /// Redirects to Problem action after successful creation of new test with HTTP Post request
        /// </summary>
        /// <param name="id">Problem id for which the posted test will be saved</param>
        /// <param name="test">Created test posted information</param>
        /// <returns>Redirects to /Administration/Tests/Problem/{id} after succesful creation otherwise to /Administration/Test/ with proper error message</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int id, TestViewModel test)
        {
            var problem = this.Data.Problems.All().FirstOrDefault(pr => pr.Id == id);

            if (problem == null)
            {
                this.TempData[GlobalConstants.DangerMessage] = "Невалидна задача";
                return this.RedirectToAction(GlobalConstants.Index);
            }

            if (test != null && this.ModelState.IsValid)
            {
                this.Data.Tests.Add(new Test
                {
                    InputDataAsString = test.InputFull,
                    OutputDataAsString = test.OutputFull,
                    ProblemId = id,
                    IsTrialTest = test.IsTrialTest,
                    OrderBy = test.OrderBy
                });

                this.Data.SaveChanges();

                this.RetestSubmissions(problem);

                this.TempData[GlobalConstants.InfoMessage] = "Тестът беше добавен успешно.";
                return this.RedirectToAction("Problem", new { id });
            }

            return this.View(test);
        }

        /// <summary>
        /// Returns view for the tests administration edit page with HTTP Get request - editing test by id
        /// </summary>
        /// <param name="id">Id for test to be edited</param>
        /// <returns>View for /Administration/Tests/Edit/{id} otherwise redirects to /Administration/Test/ with proper error message</returns>
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var test = this.Data.Tests.All()
                .Where(t => t.Id == id)
                .Select(TestViewModel.FromTest)
                .FirstOrDefault();

            if (test == null)
            {
                this.TempData[GlobalConstants.DangerMessage] = "Невалиден тест";
                return this.RedirectToAction(GlobalConstants.Index);
            }

            return this.View(test);
        }

        /// <summary>
        /// Redirects to Problem action after successful edit of the test with HTTP Post request
        /// </summary>
        /// <param name="id">Id for edited test</param>
        /// <param name="test">Edited test posted information</param>
        /// <returns>Redirects to /Administration/Tests/Problem/{id} after succesful edit otherwise to /Administration/Test/ with proper error message</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, TestViewModel test)
        {
            if (test != null && this.ModelState.IsValid)
            {
                var existingTest = this.Data.Tests
                    .All()
                    .FirstOrDefault(t => t.Id == id);

                if (existingTest == null)
                {
                    this.TempData[GlobalConstants.DangerMessage] = "Невалиден тест";
                    return this.RedirectToAction("Problem", new { id });
                }

                existingTest.InputData = test.InputData;
                existingTest.OutputData = test.OutputData;
                existingTest.OrderBy = test.OrderBy;
                existingTest.IsTrialTest = test.IsTrialTest;

                this.Data.SaveChanges();

                this.RetestSubmissions(existingTest.Problem);

                this.TempData[GlobalConstants.InfoMessage] = "Тестът беше променен успешно.";
                return this.RedirectToAction("Problem", new { id = existingTest.ProblemId });
            }

            return this.View(test);
        }

        /// <summary>
        /// Returns view for the tests administration delete page with HTTP Get request - deleting test by id
        /// </summary>
        /// <param name="id">Id for test to be deleted</param>
        /// <returns>View for /Administration/Tests/Delete/{id} otherwise redirects to /Administration/Test/ with proper error message</returns>
        [HttpGet]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                this.TempData[GlobalConstants.DangerMessage] = "Невалиден тест";
                return this.RedirectToAction(GlobalConstants.Index);
            }

            var test = this.Data.Tests.All()
                .Where(t => t.Id == id)
                .Select(TestViewModel.FromTest)
                .FirstOrDefault();

            if (test == null)
            {
                this.TempData[GlobalConstants.DangerMessage] = "Невалиден тест";
                return this.RedirectToAction(GlobalConstants.Index);
            }

            return this.View(test);
        }

        /// <summary>
        /// Redirects to Problem action after successful deletion of the test
        /// </summary>
        /// <param name="id">Id for test to be deleted</param>
        /// <returns>Redirects to /Administration/Tests/Problem/{id} after succesful deletion otherwise to /Administration/Test/ with proper error message</returns>
        public ActionResult ConfirmDelete(int? id)
        {
            if (id == null)
            {
                this.TempData[GlobalConstants.DangerMessage] = "Невалиден тест";
                return this.RedirectToAction(GlobalConstants.Index);
            }

            var test = this.Data.Tests.All()
                .FirstOrDefault(t => t.Id == id);

            if (test == null)
            {
                this.TempData[GlobalConstants.DangerMessage] = "Невалиден тест";
                return this.RedirectToAction(GlobalConstants.Index);
            }

            // delete all test runs for the current test
            var testRunsIdList = test.TestRuns.Select(t => t.Id).ToList();
            foreach (var testRun in testRunsIdList)
            {
                this.Data.TestRuns.Delete(testRun);
            }

            this.Data.SaveChanges();

            // delete the test
            this.Data.Tests.Delete(id.Value);
            this.Data.SaveChanges();

            // recalculate submissions point
            var submissionResults = this.Data.Submissions
                .All()
                .Where(s => s.ProblemId == test.ProblemId)
                .Select(s => new
                {
                    Id = s.Id,
                    CorrectTestRuns = s.TestRuns.Count(t => t.ResultType == TestRunResultType.CorrectAnswer),
                    AllTestRuns = s.TestRuns.Count(),
                    MaxPoints = s.Problem.MaximumPoints
                })
                .ToList();

            foreach (var submissionResult in submissionResults)
            {
                var submission = this.Data.Submissions.GetById(submissionResult.Id);
                int points = 0;
                if (submissionResult.AllTestRuns != 0)
                {
                    points = submissionResult.CorrectTestRuns / submissionResult.AllTestRuns * submissionResult.MaxPoints;
                }

                submission.Points = points;
            }

            this.Data.SaveChanges();

            this.TempData[GlobalConstants.InfoMessage] = "Тестът беше изтрит успешно.";
            return this.RedirectToAction("Problem", new { id = test.ProblemId });
        }

        /// <summary>
        /// Returns view for the tests administration delete all page with HTTP Get request - deleting all test for problem by id
        /// </summary>
        /// <param name="id">Id for the problem which tests will be deleted</param>
        /// <returns>View for /Administration/Tests/DeleteAll/{id} otherwise redirects to /Administration/Test/ with proper error message</returns>
        [HttpGet]
        public ActionResult DeleteAll(int? id)
        {
            if (id == null)
            {
                this.TempData[GlobalConstants.DangerMessage] = "Невалидна задача";
                return this.RedirectToAction(GlobalConstants.Index);
            }

            var problem = this.Data.Problems.All()
                .Where(pr => pr.Id == id)
                .Select(pr => new ProblemViewModel { Id = pr.Id, Name = pr.Name, ContestName = pr.Contest.Name })
                .FirstOrDefault();

            if (problem == null)
            {
                this.TempData[GlobalConstants.DangerMessage] = "Невалидна задача";
                return this.RedirectToAction(GlobalConstants.Index);
            }

            return this.View(problem);
        }

        /// <summary>
        /// Redirects to Problem action after successful deletion of all tests
        /// </summary>
        /// <param name="id">Id for the problem which tests will be deleted</param>
        /// <returns>Redirects to /Administration/Tests/Problem/{id} after succesful deletion otherwise to /Administration/Test/ with proper error message</returns>
        public ActionResult ConfirmDeleteAll(int? id)
        {
            if (id == null)
            {
                this.TempData[GlobalConstants.DangerMessage] = "Невалидна задача";
                return this.RedirectToAction(GlobalConstants.Index);
            }

            var problem = this.Data.Problems.All()
                .FirstOrDefault(pr => pr.Id == id);

            if (problem == null)
            {
                this.TempData[GlobalConstants.DangerMessage] = "Невалидна задача";
                return this.RedirectToAction(GlobalConstants.Index);
            }

            var tests = problem.Tests.Select(t => new { Id = t.Id, TestRuns = t.TestRuns.Select(tr => tr.Id) }).ToList();
            foreach (var test in tests)
            {
                var testRuns = test.TestRuns.ToList();
                foreach (var testRun in testRuns)
                {
                    this.Data.TestRuns.Delete(testRun);
                }

                this.Data.Tests.Delete(test.Id);
            }

            this.Data.SaveChanges();

            this.RetestSubmissions(problem);

            this.TempData[GlobalConstants.InfoMessage] = "Тестовете бяха изтрити успешно";
            return this.RedirectToAction("Problem", new { id = id });
        }

        /// <summary>
        /// Returns view for the tests administration details page - showing information about test by id
        /// </summary>
        /// <param name="id">Id for test which details will be shown</param>
        /// <returns>View for /Administration/Tests/Details/{id} otherwise redirects to /Administration/Test/ with proper error message</returns>
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                this.TempData[GlobalConstants.DangerMessage] = "Невалиден тест";
                return this.RedirectToAction(GlobalConstants.Index);
            }

            var test = this.Data.Tests.All()
                .Where(t => t.Id == id)
                .Select(TestViewModel.FromTest)
                .FirstOrDefault();

            if (test == null)
            {
                this.TempData[GlobalConstants.DangerMessage] = "Невалиден тест";
                return this.RedirectToAction(GlobalConstants.Index);
            }

            return this.View(test);
        }

        /// <summary>
        /// Returns full input data as string content for test by id
        /// </summary>
        /// <param name="id">Id of the test to show full input</param>
        /// <returns>Content as html of the test input</returns>
        public ActionResult FullInput(int id)
        {
            var result = this.Data.Tests.All().FirstOrDefault(t => t.Id == id).InputDataAsString;
            return this.Content(HttpUtility.HtmlEncode(result), "text/html");
        }

        /// <summary>
        /// Returns full output data as string content for test by id
        /// </summary>
        /// <param name="id">Id of the test to show full output</param>
        /// <returns>Content as html of the test output</returns>
        public ActionResult FullOutput(int id)
        {
            var result = this.Data.Tests.All().FirstOrDefault(t => t.Id == id).OutputDataAsString;
            return this.Content(HttpUtility.HtmlEncode(result), "text/html");
        }

        /// <summary>
        /// Returns test runs for test by id
        /// </summary>
        /// <param name="id">Id of the test to get test runs</param>
        /// <returns>JSON result of all test runs for the test</returns>
        public JsonResult GetTestRuns(int id)
        {
            // TODO: Add server side paging and sorting to test runs grid
            var result = this.Data.TestRuns.All()
                .Where(tr => tr.TestId == id)
                .OrderByDescending(tr => tr.Submission.CreatedOn)
                .Select(TestRunViewModel.FromTestRun);

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Returns all available contest categories
        /// </summary>
        /// <returns>JSON result of all categores as objects with Id and Name properties</returns>
        [HttpGet]
        public JsonResult GetCascadeCategories()
        {
            var result = this.Data.ContestCategories.All().Select(cat => new { Id = cat.Id, Name = cat.Name });

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Returns all available contests in category by id
        /// </summary>
        /// <param name="category">Id of category to get all contests from</param>
        /// <returns>JSON result of all contests in category as objects with Id and Name properties</returns>
        [HttpGet]
        public JsonResult GetCascadeContests(int id)
        {
            var contests = this.Data.Contests
                .All()
                .Where(con => con.CategoryId == id)
                .Select(con => new { Id = con.Id, Name = con.Name });

            return this.Json(contests, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Returns all available problems in contest by id
        /// </summary>
        /// <param name="category">Id of contest to get all problem from</param>
        /// <returns>JSON result of all problems in contest as objects with Id and Name properties</returns>
        [HttpGet]
        public JsonResult GetCascadeProblems(int id)
        {
            var problems = this.Data.Problems
                .All()
                .Where(pr => pr.ContestId == id)
                .Select(pr => new { Id = pr.Id, Name = pr.Name });

            return this.Json(problems, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Returns contest and category id for a problem
        /// </summary>
        /// <param name="id">Id of the problem to get information for</param>
        /// <returns>JSON result of contest and category id as object</returns>
        [HttpGet]
        public JsonResult GetProblemInformation(int id)
        {
            var problem = this.Data.Problems.All().FirstOrDefault(pr => pr.Id == id);

            var contestId = problem.ContestId;

            var categoryId = problem.Contest.CategoryId;

            var result = new { Contest = contestId, Category = categoryId };

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Returns all problems as pair of Id and Name if problem name contains given substring
        /// </summary>
        /// <param name="text">Substring which problems should contain in name</param>
        /// <returns>JSON result of all problems that contain the given substring</returns>
        [HttpGet]
        public JsonResult GetSearchedProblems(string text)
        {
            var result = this.Data.Problems
                .All()
                .Where(pr => pr.Name.ToLower().Contains(text.ToLower()))
                .Select(pr => new { Id = pr.Id, Name = pr.Name });

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Returns all tests for particular problem by id
        /// </summary>
        /// <param name="id">Id of the problem to get all tests</param>
        /// <returns>JSON result of all tests for the problem</returns>
        public ContentResult ProblemTests(int id)
        {
            var result = this.GetData(id);
            return this.LargeJson(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import(string problemId, HttpPostedFileBase file, bool retestTask, bool deleteOldFiles)
        {
            int id;

            if (!int.TryParse(problemId, out id))
            {
                this.TempData.Add(GlobalConstants.DangerMessage, "Невалидна задача");
                return this.RedirectToAction(GlobalConstants.Index);
            }

            var problem = this.Data.Problems.All().FirstOrDefault(x => x.Id == id);

            if (problem == null)
            {
                this.TempData.Add(GlobalConstants.DangerMessage, "Невалидна задача");
                return this.RedirectToAction(GlobalConstants.Index);
            }

            if (file == null || file.ContentLength == 0)
            {
                this.TempData.Add(GlobalConstants.DangerMessage, "Файлът не може да бъде празен");
                return this.RedirectToAction("Problem", new { id = id });
            }

            var extension = file.FileName.Substring(file.FileName.Length - 4, 4);

            if (extension != ".zip")
            {
                this.TempData.Add(GlobalConstants.DangerMessage, "Файлът трябва да бъде .ZIP файл");
                return this.RedirectToAction("Problem", new { id = id });
            }

            if (deleteOldFiles)
            {
                var tests = problem.Tests.Select(t => new { Id = t.Id, TestRuns = t.TestRuns.Select(tr => tr.Id) }).ToList();
                foreach (var test in tests)
                {
                    var testRuns = test.TestRuns.ToList();
                    foreach (var testRun in testRuns)
                    {
                        this.Data.TestRuns.Delete(testRun);
                    }

                    this.Data.Tests.Delete(test.Id);
                }

                problem.Tests = new HashSet<Test>();
            }

            using (var memory = new MemoryStream())
            {
                file.InputStream.CopyTo(memory);
                memory.Position = 0;

                TestsParseResult parsedTests;

                try
                {
                    parsedTests = ZippedTestsManipulator.Parse(memory);
                }
                catch
                {
                    this.TempData.Add(GlobalConstants.DangerMessage, "Zip файлът е повреден");
                    return this.RedirectToAction("Problem", new { id });
                }

                if (parsedTests.ZeroInputs.Count != parsedTests.ZeroOutputs.Count || parsedTests.Inputs.Count != parsedTests.Outputs.Count)
                {
                    this.TempData.Add(GlobalConstants.DangerMessage, "Невалидни тестове");
                    return this.RedirectToAction("Problem", new { id });
                }

                ZippedTestsManipulator.AddTestsToProblem(problem, parsedTests);

                this.Data.SaveChanges();
            }

            if (retestTask)
            {
                this.RetestSubmissions(problem);
            }

            this.TempData.Add(GlobalConstants.InfoMessage, "Тестовете са добавени към задачата");

            return this.RedirectToAction("Problem", new { id });
        }

        /// <summary>
        /// Creates zip files with all tests in given task
        /// </summary>
        /// <param name="id">Task id</param>
        /// <returns>Zip file containing all tests in format {task}.{testNum}[.{zeroNum}].{in|out}.txt</returns>
        public ActionResult Export(int id)
        {
            var problem = this.Data.Problems.All().Where(x => x.Id == id).FirstOrDefault();

            if (problem == null)
            {
                this.TempData[GlobalConstants.DangerMessage] = "Задачата не съществува";
                return this.RedirectToAction(GlobalConstants.Index);
            }

            var tests = problem.Tests.OrderBy(x => x.OrderBy);

            ZipFile zip = new ZipFile(string.Format("{0}_Tests_{1}", problem.Name, DateTime.Now));

            using (zip)
            {
                int trialTestCounter = 1;
                int testCounter = 1;

                foreach (var test in tests)
                {
                    if (test.IsTrialTest)
                    {
                        zip.AddEntry(string.Format("test.000.{0:D3}.in.txt", trialTestCounter), test.InputDataAsString);
                        zip.AddEntry(string.Format("test.000.{0:D3}.out.txt", trialTestCounter), test.OutputDataAsString);
                        trialTestCounter++;
                    }
                    else
                    {
                        zip.AddEntry(string.Format("test.{0:D3}.in.txt", testCounter), test.InputDataAsString);
                        zip.AddEntry(string.Format("test.{0:D3}.out.txt", testCounter), test.OutputDataAsString);
                        testCounter++;
                    }
                }
            }

            var stream = new MemoryStream();

            zip.Save(stream);
            stream.Position = 0;

            return this.File(stream, MediaTypeNames.Application.Zip, zip.Name);
        }

        [HttpGet]
        public FileResult ExportToExcel([DataSourceRequest] DataSourceRequest request, int id)
        {
            return this.ExportToExcel(request, this.GetData(id));
        }

        private IEnumerable GetData(int id)
        {
            var result = this.Data.Tests.All()
                .Where(test => test.ProblemId == id)
                .OrderByDescending(test => test.IsTrialTest)
                .ThenBy(test => test.OrderBy)
                .Select(TestViewModel.FromTest);

            return result;
        }

        private void RetestSubmissions(Problem problem)
        {
            var submissionIds = problem.Submissions.Select(s => new { Id = s.Id }).ToList();
            foreach (var submissionId in submissionIds)
            {
                var currentSubmission = this.Data.Submissions.GetById(submissionId.Id);
                currentSubmission.Processed = false;
                currentSubmission.Processing = false;
            }

            this.Data.SaveChanges();
        }
    }
}