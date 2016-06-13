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

    using Ionic.Zip;

    using Kendo.Mvc.UI;

    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Areas.Administration.ViewModels.Problem;
    using OJS.Web.Areas.Administration.ViewModels.Test;
    using OJS.Web.Areas.Administration.ViewModels.TestRun;
    using OJS.Web.Common;
    using OJS.Web.Common.Extensions;
    using OJS.Web.Common.ZippedTestManipulator;

    using Resource = Resources.Areas.Administration.Tests.TestsControllers;

    /// <summary>
    /// Controller class for administrating problems' input and output tests, inherits Administration controller for authorisation
    /// </summary>
    public class TestsController : LecturerBaseController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestsController"/> class.
        /// </summary>
        /// <param name="data">Open Judge System Database context for the control
        /// ler to work with</param>
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
            if (id == null || !this.CheckIfUserHasProblemPermissions(id.Value))
            {
                this.TempData[GlobalConstants.DangerMessage] = "Нямате привилегиите за това действие";
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
            }

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
                this.TempData.AddDangerMessage(Resource.Invalid_problem);
                return this.RedirectToAction(GlobalConstants.Index);
            }

            if (!this.CheckIfUserHasProblemPermissions(id.Value))
            {
                this.TempData[GlobalConstants.DangerMessage] = "Нямате привилегиите за това действие";
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
            }

            var problem = this.Data.Problems.All().FirstOrDefault(pr => pr.Id == id);

            if (problem == null)
            {
                this.TempData.AddDangerMessage(Resource.Invalid_problem);
                return this.RedirectToAction(GlobalConstants.Index);
            }

            var test = new TestViewModel
            {
                ProblemId = problem.Id,
                ProblemName = problem.Name,
                AllTypes = Enum.GetValues(typeof(TestType)).Cast<TestType>().Select(v => new SelectListItem
                {
                    Text = v.GetLocalizedDescription(),
                    Value = ((int)v).ToString(CultureInfo.InvariantCulture)
                }),
                OrderBy = this.Data.Tests
                    .All()
                    .Where(t => t.ProblemId == problem.Id && !t.IsTrialTest)
                    .OrderByDescending(t => t.OrderBy)
                    .Select(t => t.OrderBy)
                    .FirstOrDefault() + 1
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
                this.TempData.AddDangerMessage(Resource.Invalid_problem);
                return this.RedirectToAction(GlobalConstants.Index);
            }

            if (!this.CheckIfUserHasProblemPermissions(id))
            {
                this.TempData[GlobalConstants.DangerMessage] = "Нямате привилегиите за това действие";
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
            }

            if (test != null && this.ModelState.IsValid)
            {
                this.Data.Tests.Add(new Test
                {
                    InputDataAsString = test.InputFull,
                    OutputDataAsString = test.OutputFull,
                    ProblemId = id,
                    IsTrialTest = test.Type == TestType.Trial,
                    OrderBy = test.OrderBy,
                    IsOpenTest = test.Type == TestType.Open
                });

                this.Data.SaveChanges();

                this.RetestSubmissions(problem.Id);

                this.TempData.AddInfoMessage(Resource.Test_added_successfully);
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
                this.TempData.AddDangerMessage(Resource.Invalid_test);
                return this.RedirectToAction(GlobalConstants.Index);
            }

            test.AllTypes = Enum.GetValues(typeof(TestType)).Cast<TestType>().Select(v => new SelectListItem
            {
                Text = v.GetLocalizedDescription(),
                Value = ((int)v).ToString(CultureInfo.InvariantCulture)
            });

            if (!this.CheckIfUserHasProblemPermissions(test.ProblemId))
            {
                this.TempData[GlobalConstants.DangerMessage] = "Нямате привилегиите за това действие";
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
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
                var existingTest = this.Data.Tests.GetById(id);

                if (existingTest == null)
                {
                    this.TempData.AddDangerMessage(Resource.Invalid_test);
                    return this.RedirectToAction("Problem", new { id });
                }

                if (!this.CheckIfUserHasProblemPermissions(existingTest.ProblemId))
                {
                    this.TempData[GlobalConstants.DangerMessage] = "Нямате привилегиите за това действие";
                    return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
                }

                existingTest.InputData = test.InputData;
                existingTest.OutputData = test.OutputData;
                existingTest.OrderBy = test.OrderBy;
                existingTest.IsTrialTest = test.Type == TestType.Trial;
                existingTest.IsOpenTest = test.Type == TestType.Open;

                this.Data.SaveChanges();

                this.RetestSubmissions(existingTest.ProblemId);

                this.TempData.AddInfoMessage(Resource.Test_edited_successfully);
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
                this.TempData.AddDangerMessage(Resource.Invalid_test);
                return this.RedirectToAction(GlobalConstants.Index);
            }

            var test = this.Data.Tests.All()
                .Where(t => t.Id == id)
                .Select(TestViewModel.FromTest)
                .FirstOrDefault();

            if (test == null)
            {
                this.TempData.AddDangerMessage(Resource.Invalid_test);
                return this.RedirectToAction(GlobalConstants.Index);
            }

            if (!this.CheckIfUserHasProblemPermissions(test.ProblemId))
            {
                this.TempData[GlobalConstants.DangerMessage] = "Нямате привилегиите за това действие";
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
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
                this.TempData.AddDangerMessage(Resource.Invalid_test);
                return this.RedirectToAction<TestsController>(x => x.Index());
            }

            var test = this.Data.Tests.All().FirstOrDefault(t => t.Id == id);

            if (test == null)
            {
                this.TempData.AddDangerMessage(Resource.Invalid_test);
                return this.RedirectToAction<TestsController>(x => x.Index());
            }

            if (!this.CheckIfUserHasProblemPermissions(test.ProblemId))
            {
                this.TempData[GlobalConstants.DangerMessage] = "Нямате привилегиите за това действие";
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
            }

            // delete all test runs for the test
            this.Data.TestRuns.Delete(tr => tr.TestId == id.Value);
            this.Data.SaveChanges();

            // delete the test
            this.Data.Tests.Delete(test);
            this.Data.SaveChanges();

            // recalculate submissions point
            var submissionResults = this.Data.Submissions
                .All()
                .Where(s => s.ProblemId == test.ProblemId)
                .Select(s => new
                {
                    s.Id,
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

            this.TempData.AddInfoMessage(Resource.Test_deleted_successfully);
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
                this.TempData.AddDangerMessage(Resource.Invalid_problem);
                return this.RedirectToAction(GlobalConstants.Index);
            }

            if (!this.CheckIfUserHasProblemPermissions(id.Value))
            {
                this.TempData[GlobalConstants.DangerMessage] = "Нямате привилегиите за това действие";
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
            }

            var problem = this.Data.Problems
                .All()
                .Where(pr => pr.Id == id)
                .Select(pr => new ProblemViewModel { Id = pr.Id, Name = pr.Name, ContestName = pr.Contest.Name })
                .FirstOrDefault();

            if (problem == null)
            {
                this.TempData.AddDangerMessage(Resource.Invalid_problem);
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
            if (id == null || !this.Data.Problems.All().Any(p => p.Id == id))
            {
                this.TempData.AddDangerMessage(Resource.Invalid_problem);
                return this.RedirectToAction<TestsController>(x => x.Index());
            }

            if (!this.CheckIfUserHasProblemPermissions(id.Value))
            {
                this.TempData[GlobalConstants.DangerMessage] = "Нямате привилегиите за това действие";
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
            }

            var problem = this.Data.Problems.All().FirstOrDefault(pr => pr.Id == id);

            if (problem == null)
            {
                this.TempData.AddDangerMessage(Resource.Invalid_problem);
                return this.RedirectToAction(GlobalConstants.Index);
            }

            this.Data.TestRuns.Delete(testRun => testRun.Submission.ProblemId == id);
            this.Data.SaveChanges();

            this.Data.Tests.Delete(test => test.ProblemId == id);
            this.Data.SaveChanges();

            this.RetestSubmissions(problem.Id);

            this.TempData.AddInfoMessage(Resource.Tests_deleted_successfully);
            return this.RedirectToAction("Problem", new { id });
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
                this.TempData.AddDangerMessage(Resource.Invalid_test);
                return this.RedirectToAction(GlobalConstants.Index);
            }

            var test = this.Data.Tests.All()
                .Where(t => t.Id == id)
                .Select(TestViewModel.FromTest)
                .FirstOrDefault();

            if (test == null)
            {
                this.TempData.AddDangerMessage(Resource.Invalid_test);
                return this.RedirectToAction(GlobalConstants.Index);
            }

            if (!this.CheckIfUserHasProblemPermissions(test.ProblemId))
            {
                this.TempData[GlobalConstants.DangerMessage] = "Нямате привилегиите за това действие";
                return this.RedirectToAction("Index", "Contests", new { area = "Administration" });
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
            var result = this.Data.Tests.All().FirstOrDefault(t => t.Id == id);
            if (result != null)
            {
                return this.Content(HttpUtility.HtmlEncode(result.InputDataAsString), "text/html");
            }

            return new EmptyResult();
        }

        /// <summary>
        /// Returns full output data as string content for test by id
        /// </summary>
        /// <param name="id">Id of the test to show full output</param>
        /// <returns>Content as html of the test output</returns>
        public ActionResult FullOutput(int id)
        {
            var result = this.Data.Tests.All().FirstOrDefault(t => t.Id == id);

            if (result != null)
            {
                return this.Content(HttpUtility.HtmlEncode(result.OutputDataAsString), "text/html");
            }

            return new EmptyResult();
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
            var result = this.Data.ContestCategories.All().Select(cat => new { cat.Id, cat.Name });

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Returns all available contests in category by id
        /// </summary>
        /// <param name="id">Id of category to get all contests from</param>
        /// <returns>JSON result of all contests in category as objects with Id and Name properties</returns>
        [HttpGet]
        public JsonResult GetCascadeContests(int id)
        {
            var contests = this.Data.Contests
                .All()
                .Where(con => con.CategoryId == id)
                .Select(con => new { con.Id, con.Name });

            return this.Json(contests, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Returns all available problems in contest by id
        /// </summary>
        /// <param name="id">Id of contest to get all problem from</param>
        /// <returns>JSON result of all problems in contest as objects with Id and Name properties</returns>
        [HttpGet]
        public JsonResult GetCascadeProblems(int id)
        {
            var problems = this.Data.Problems
                .All()
                .Where(pr => pr.ContestId == id)
                .Select(pr => new { pr.Id, pr.Name });

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
            if (!this.CheckIfUserHasProblemPermissions(id))
            {
                this.TempData[GlobalConstants.DangerMessage] = "Нямате привилегиите за това действие";
                return this.Json("No premissions");
            }

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
                .Select(pr => new { pr.Id, pr.Name });

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
                this.TempData.AddDangerMessage(Resource.Invalid_problem);
                return this.RedirectToAction(GlobalConstants.Index);
            }

            var problem = this.Data.Problems.All().FirstOrDefault(x => x.Id == id);

            if (problem == null)
            {
                this.TempData.AddDangerMessage(Resource.Invalid_problem);
                return this.RedirectToAction(GlobalConstants.Index);
            }

            if (!this.CheckIfUserHasProblemPermissions(id))
            {
                this.TempData[GlobalConstants.DangerMessage] = "Нямате привилегиите за това действие";
                return this.Json("No premissions");
            }

            if (file == null || file.ContentLength == 0)
            {
                this.TempData.AddDangerMessage(Resource.No_empty_file);
                return this.RedirectToAction("Problem", new { id });
            }

            var extension = file.FileName.Substring(file.FileName.Length - 4, 4);

            if (extension != ".zip")
            {
                this.TempData.AddDangerMessage(Resource.Must_be_zip);
                return this.RedirectToAction("Problem", new { id });
            }

            if (deleteOldFiles)
            {
                var tests = problem.Tests.Select(t => new { t.Id, TestRuns = t.TestRuns.Select(tr => tr.Id) }).ToList();
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
                    this.TempData.AddDangerMessage(Resource.Zip_damaged);
                    return this.RedirectToAction("Problem", new { id });
                }

                if (parsedTests.ZeroInputs.Count != parsedTests.ZeroOutputs.Count || parsedTests.Inputs.Count != parsedTests.Outputs.Count)
                {
                    this.TempData.AddDangerMessage(Resource.Invalid_tests);
                    return this.RedirectToAction("Problem", new { id });
                }

                ZippedTestsManipulator.AddTestsToProblem(problem, parsedTests);

                this.Data.SaveChanges();
            }

            if (retestTask)
            {
                this.RetestSubmissions(problem.Id);
            }

            this.TempData.AddInfoMessage(Resource.Tests_addted_to_problem);

            return this.RedirectToAction("Problem", new { id });
        }

        /// <summary>
        /// Creates zip files with all tests in given task
        /// </summary>
        /// <param name="id">Task id</param>
        /// <returns>Zip file containing all tests in format {task}.{testNum}[.{zeroNum}].{in|out}.txt</returns>
        public ActionResult Export(int id)
        {
            var problem = this.Data.Problems.All().FirstOrDefault(x => x.Id == id);

            if (problem == null)
            {
                this.TempData.AddDangerMessage(Resource.Problem_does_not_exist);
                return this.RedirectToAction(GlobalConstants.Index);
            }

            if (!this.CheckIfUserHasProblemPermissions(id))
            {
                this.TempData[GlobalConstants.DangerMessage] = "Нямате привилегиите за това действие";
                return this.Json("No premissions");
            }

            var tests = problem.Tests.OrderBy(x => x.OrderBy);

            var zipFile = new ZipFile(string.Format("{0}_Tests_{1}", problem.Name, DateTime.Now));

            using (zipFile)
            {
                int trialTestCounter = 1;
                int openTestCounter = 1;
                int testCounter = 1;

                foreach (var test in tests)
                {
                    if (test.IsTrialTest)
                    {
                        zipFile.AddEntry(string.Format("test.000.{0:D3}.in.txt", trialTestCounter), test.InputDataAsString);
                        zipFile.AddEntry(string.Format("test.000.{0:D3}.out.txt", trialTestCounter), test.OutputDataAsString);
                        trialTestCounter++;
                    }
                    else if (test.IsOpenTest)
                    {
                        zipFile.AddEntry(string.Format("test.open.{0:D3}.in.txt", openTestCounter), test.InputDataAsString);
                        zipFile.AddEntry(string.Format("test.open.{0:D3}.out.txt", openTestCounter), test.OutputDataAsString);
                        openTestCounter++;
                    }
                    else
                    {
                        zipFile.AddEntry(string.Format("test.{0:D3}.in.txt", testCounter), test.InputDataAsString);
                        zipFile.AddEntry(string.Format("test.{0:D3}.out.txt", testCounter), test.OutputDataAsString);
                        testCounter++;
                    }
                }
            }

            var stream = new MemoryStream();

            zipFile.Save(stream);
            stream.Position = 0;

            return this.File(stream, MediaTypeNames.Application.Zip, zipFile.Name);
        }

        [HttpGet]
        public FileResult ExportToExcel([DataSourceRequest] DataSourceRequest request, int id)
        {
            return this.ExportToExcel(request, this.GetData(id));
        }

        private IEnumerable GetData(int id)
        {
            if (!this.CheckIfUserHasProblemPermissions(id))
            {
                return new List<TestViewModel>();
            }

            var result = this.Data.Tests
                .All()
                .Where(test => test.ProblemId == id)
                .OrderByDescending(test => test.IsTrialTest)
                .ThenBy(test => test.OrderBy)
                .Select(TestViewModel.FromTest);

            return result;
        }

        private void RetestSubmissions(int problemId)
        {
            this.Data.Submissions.Update(
                submission => submission.ProblemId == problemId,
                submission => new Submission { Processed = false, Processing = false });

            this.Data.SaveChanges();
        }
    }
}