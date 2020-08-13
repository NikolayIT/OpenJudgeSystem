namespace OJS.Web.Areas.Administration.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net.Mime;
    using System.Text;
    using System.Transactions;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Expressions;

    using Ionic.Zip;

    using Kendo.Mvc.UI;

    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Common.Helpers;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Services.Business.Problems;
    using OJS.Services.Business.Submissions;
    using OJS.Services.Data.Problems;
    using OJS.Services.Data.Submissions;
    using OJS.Services.Data.TestRuns;
    using OJS.Services.Data.Tests;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Areas.Administration.ViewModels.Problem;
    using OJS.Web.Areas.Administration.ViewModels.Test;
    using OJS.Web.Areas.Administration.ViewModels.TestRun;
    using OJS.Web.Common;
    using OJS.Web.Common.Extensions;
    using OJS.Web.Common.ZippedTestManipulator;
    using OJS.Workers.Common;

    using static OJS.Web.Common.WebConstants;

    using GeneralResource = Resources.Areas.Administration.AdministrationGeneral;
    using Resource = Resources.Areas.Administration.Tests.TestsControllers;

    /// <summary>
    /// Controller class for administrating problems' input and output tests, inherits Administration controller for authorisation
    /// </summary>
    public class TestsController : LecturerBaseController
    {
        private readonly IProblemsDataService problemsData;
        private readonly ISubmissionsDataService submissionsData;
        private readonly ITestRunsDataService testRunsData;
        private readonly ITestsDataService testsData;
        private readonly IProblemsBusinessService problemsBusiness;
        private readonly ISubmissionsBusinessService submissionsBusiness;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestsController"/> class.
        /// </summary>
        public TestsController(
            IOjsData data,
            IProblemsDataService problemsData,
            ISubmissionsDataService submissionsData,
            ITestRunsDataService testRunsData,
            ITestsDataService testsData,
            IProblemsBusinessService problemsBusiness,
            ISubmissionsBusinessService submissionsBusiness)
            : base(data)
        {
            this.problemsData = problemsData;
            this.submissionsData = submissionsData;
            this.testRunsData = testRunsData;
            this.testsData = testsData;
            this.problemsBusiness = problemsBusiness;
            this.submissionsBusiness = submissionsBusiness;
        }

        /// <summary>
        /// Returns view for the tests administration index page
        /// </summary>
        /// <returns>View for /Administration/Tests/</returns>
        public ActionResult Index() => this.View();

        /// <summary>
        /// Returns view for the tests administration index page and populates problem, contest, category dropdowns and tests grid if problem id is correct
        /// </summary>
        /// <param name="id">Problem id which tests are populated</param>
        /// <returns>View for /Administration/Tests/Problem/{id}</returns>
        public ActionResult Problem(int? id)
        {
            if (id == null || !this.CheckIfUserHasProblemPermissions(id.Value))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
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
            if (!id.HasValue)
            {
                this.TempData.AddDangerMessage(Resource.Invalid_problem);
                return this.RedirectToAction(c => c.Index());
            }

            if (!this.CheckIfUserHasProblemPermissions(id.Value))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            var problem = this.problemsData.GetById(id.Value);

            if (problem == null)
            {
                this.TempData.AddDangerMessage(Resource.Invalid_problem);
                return this.RedirectToAction(c => c.Index());
            }

            var test = new TestViewModel
            {
                ProblemId = problem.Id,
                ProblemName = problem.Name,
                OrderBy = this.testsData
                    .GetAllNonTrialByProblem(problem.Id)
                    .OrderByDescending(t => t.OrderBy)
                    .Select(t => t.OrderBy)
                    .FirstOrDefault() + 1
            };

            ImportAllTypesInTest(test);
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
            if (test == null || !this.ModelState.IsValid)
            {
                ImportAllTypesInTest(test);
                return this.View(test);
            }

            if (!this.problemsData.ExistsById(id))
            {
                this.TempData.AddDangerMessage(Resource.Invalid_problem);
                return this.RedirectToAction(c => c.Index());
            }

            if (!this.CheckIfUserHasProblemPermissions(id))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            this.testsData.Add(new Test
            {
                InputDataAsString = test.InputFull,
                OutputDataAsString = test.OutputFull,
                ProblemId = id,
                IsTrialTest = test.Type == TestType.Trial,
                OrderBy = test.OrderBy,
                IsOpenTest = test.Type == TestType.Open,
                HideInput = test.HideInput,
            });

            if (test.RetestProblem)
            {
                this.problemsBusiness.RetestById(id);
            }

            this.TempData.AddInfoMessage(Resource.Test_added_successfully);
            return this.RedirectToAction(c => c.Problem(id));
        }

        /// <summary>
        /// Returns view for the tests administration edit page with HTTP Get request - editing test by id
        /// </summary>
        /// <param name="id">Id for test to be edited</param>
        /// <returns>View for /Administration/Tests/Edit/{id} otherwise redirects to /Administration/Test/ with proper error message</returns>
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var test = this.testsData
                .GetByIdQuery(id)
                .Select(TestViewModel.FromTest)
                .FirstOrDefault();

            if (test == null)
            {
                this.TempData.AddDangerMessage(Resource.Invalid_test);
                return this.RedirectToAction(c => c.Index());
            }

            if (!this.CheckIfUserHasProblemPermissions(test.ProblemId))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            ImportAllTypesInTest(test);

            return this.View(test);
        }

        /// <summary>
        /// Redirects to Problem action after successful edit of the test with HTTP Post request
        /// </summary>
        /// <param name="id">Id for edited test</param>
        /// <param name="test">Edited test posted information</param>
        /// <param name="retestTask">Value indicating if the problem should be retested</param>
        /// <returns>Redirects to /Administration/Tests/Problem/{id} after succesful edit otherwise to /Administration/Test/ with proper error message</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, TestViewModel test)
        {
            if (test == null || !this.ModelState.IsValid)
            {
                ImportAllTypesInTest(test);
                return this.View(test);
            }

            var existingTest = this.testsData.GetById(id);

            if (existingTest == null)
            {
                this.TempData.AddDangerMessage(Resource.Invalid_test);
                return this.RedirectToAction(c => c.Problem(id));
            }

            if (!this.CheckIfUserHasProblemPermissions(existingTest.ProblemId))
            {
                this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            using (var scope = TransactionsHelper.CreateTransactionScope(IsolationLevel.RepeatableRead))
            {
                existingTest.InputData = test.InputData;
                existingTest.OutputData = test.OutputData;
                existingTest.OrderBy = test.OrderBy;
                existingTest.IsTrialTest = test.Type == TestType.Trial;
                existingTest.IsOpenTest = test.Type == TestType.Open;
                existingTest.HideInput = test.HideInput;

                this.testsData.Update(existingTest);

                this.submissionsData.RemoveTestRunsCacheByProblem(existingTest.ProblemId);
                this.testRunsData.DeleteByProblem(existingTest.ProblemId);

                if (test.RetestProblem)
                {
                    this.problemsBusiness.RetestById(existingTest.ProblemId);
                }

                scope.Complete();
            }

            this.TempData.AddInfoMessage(Resource.Test_edited_successfully);
            return this.RedirectToAction(c => c.Problem(existingTest.ProblemId));
        }

        /// <summary>
        /// Returns view for the tests administration delete page with HTTP Get request - deleting test by id
        /// </summary>
        /// <param name="id">Id for test to be deleted</param>
        /// <returns>View for /Administration/Tests/Delete/{id} otherwise redirects to /Administration/Test/ with proper error message</returns>
        [HttpGet]
        public ActionResult Delete(int? id)
        {
            if (!id.HasValue)
            {
                this.TempData.AddDangerMessage(Resource.Invalid_test);
                return this.RedirectToAction(c => c.Index());
            }

            var test = this.testsData
                .GetByIdQuery(id.Value)
                .Select(TestViewModel.FromTest)
                .FirstOrDefault();

            if (test == null)
            {
                this.TempData.AddDangerMessage(Resource.Invalid_test);
                return this.RedirectToAction(c => c.Index());
            }

            if (!this.CheckIfUserHasProblemPermissions(test.ProblemId))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            return this.View(test);
        }

        /// <summary>
        /// Redirects to Problem action after successful deletion of the test
        /// </summary>
        /// <param name="id">Id for test to be deleted</param>
        /// <returns>Redirects to /Administration/Tests/Problem/{id} after succesful deletion otherwise to /Administration/Test/ with proper error message</returns>
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmDelete(int? id)
        {
            if (!id.HasValue)
            {
                this.TempData.AddDangerMessage(Resource.Invalid_test);
                return this.RedirectToAction(x => x.Index());
            }

            var test = this.testsData.GetById(id.Value);

            if (test == null)
            {
                this.TempData.AddDangerMessage(Resource.Invalid_test);
                return this.RedirectToAction(x => x.Index());
            }

            if (!this.CheckIfUserHasProblemPermissions(test.ProblemId))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            using (var scope = TransactionsHelper.CreateTransactionScope())
            {
                this.testRunsData.DeleteByTest(id.Value);

                this.testsData.Delete(test);

                this.submissionsBusiness.RecalculatePointsByProblem(test.ProblemId);

                scope.Complete();
            }

            this.TempData.AddInfoMessage(Resource.Test_deleted_successfully);
            return this.RedirectToAction(c => c.Problem(test.ProblemId));
        }

        /// <summary>
        /// Returns view for the tests administration delete all page with HTTP Get request - deleting all test for problem by id
        /// </summary>
        /// <param name="id">Id for the problem which tests will be deleted</param>
        /// <returns>View for /Administration/Tests/DeleteAll/{id} otherwise redirects to /Administration/Test/ with proper error message</returns>
        [HttpGet]
        public ActionResult DeleteAll(int? id)
        {
            if (!id.HasValue)
            {
                this.TempData.AddDangerMessage(Resource.Invalid_problem);
                return this.RedirectToAction(c => c.Index());
            }

            if (!this.CheckIfUserHasProblemPermissions(id.Value))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            var problem = this.problemsData
                .GetByIdQuery(id.Value)
                .Select(pr => new ProblemViewModel
                {
                    Id = pr.Id,
                    Name = pr.Name,
                    ContestName = pr.ProblemGroup.Contest.Name
                })
                .FirstOrDefault();

            if (problem == null)
            {
                this.TempData.AddDangerMessage(Resource.Invalid_problem);
                return this.RedirectToAction(c => c.Index());
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
            if (!id.HasValue || !this.problemsData.ExistsById(id.Value))
            {
                this.TempData.AddDangerMessage(Resource.Invalid_problem);
                return this.RedirectToAction(c => c.Index());
            }

            if (!this.CheckIfUserHasProblemPermissions(id.Value))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            using (var scope = TransactionsHelper.CreateTransactionScope())
            {
                this.testRunsData.DeleteByProblem(id.Value);
                this.testsData.DeleteByProblem(id.Value);
                this.submissionsData.RemoveTestRunsCacheByProblem(id.Value);

                scope.Complete();
            }

            this.TempData.AddInfoMessage(Resource.Tests_deleted_successfully);
            return this.RedirectToAction(c => c.Problem(id.Value));
        }

        /// <summary>
        /// Returns view for the tests administration details page - showing information about test by id
        /// </summary>
        /// <param name="id">Id for test which details will be shown</param>
        /// <returns>View for /Administration/Tests/Details/{id} otherwise redirects to /Administration/Test/ with proper error message</returns>
        public ActionResult Details(int? id)
        {
            if (!id.HasValue)
            {
                this.TempData.AddDangerMessage(Resource.Invalid_test);
                return this.RedirectToAction(c => c.Index());
            }

            var test = this.testsData
                .GetByIdQuery(id.Value)
                .Select(TestViewModel.FromTest)
                .FirstOrDefault();

            if (test == null)
            {
                this.TempData.AddDangerMessage(Resource.Invalid_test);
                return this.RedirectToAction(c => c.Index());
            }

            if (!this.CheckIfUserHasProblemPermissions(test.ProblemId))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
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
            var test = this.testsData.GetById(id);
            if (test != null)
            {
                return this.Content(HttpUtility.HtmlEncode(test.InputDataAsString), "text/html");
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
            var test = this.testsData.GetById(id);

            if (test != null)
            {
                return this.Content(HttpUtility.HtmlEncode(test.OutputDataAsString), "text/html");
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
            var result = this.testRunsData
                .GetAllByTest(id)
                .OrderByDescending(tr => tr.Submission.CreatedOn)
                .Select(TestRunViewModel.FromTestRun);

            return this.Json(result, JsonRequestBehavior.AllowGet);
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
                this.TempData.AddDangerMessage(GeneralResource.No_privileges_message);
                return this.Json("No premissions");
            }

            var problem = this.problemsData.GetById(id);

            var contestId = problem.ProblemGroup.ContestId;

            var categoryId = problem.ProblemGroup.Contest.CategoryId;

            var result = new { Contest = contestId, Category = categoryId };

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
            if (!int.TryParse(problemId, out var id))
            {
                this.TempData.AddDangerMessage(Resource.Invalid_problem);
                return this.RedirectToAction(c => c.Index());
            }

            var problem = this.problemsData.GetById(id);

            if (problem == null)
            {
                this.TempData.AddDangerMessage(Resource.Invalid_problem);
                return this.RedirectToAction(c => c.Index());
            }

            if (!this.CheckIfUserHasProblemPermissions(id))
            {
                this.TempData.AddDangerMessage(GeneralResource.No_privileges_message);
                return this.Json("No premissions");
            }

            if (file == null || file.ContentLength == 0)
            {
                this.TempData.AddDangerMessage(Resource.No_empty_file);
                return this.RedirectToAction(c => c.Problem(id));
            }

            var extension = Path.GetExtension(file.FileName);

            if (extension != ".zip")
            {
                this.TempData.AddDangerMessage(Resource.Must_be_zip);
                return this.RedirectToAction(c => c.Problem(id));
            }

            TestsParseResult parsedTests;

            using (var memory = new MemoryStream())
            {
                file.InputStream.CopyTo(memory);
                memory.Position = 0;

                try
                {
                    parsedTests = ZippedTestsParser.Parse(memory);
                }
                catch
                {
                    this.TempData.AddDangerMessage(Resource.Zip_damaged);
                    return this.RedirectToAction(c => c.Problem(id));
                }
            }

            if (!ZippedTestsParser.AreTestsParsedCorrectly(parsedTests))
            {
                this.TempData.AddDangerMessage(Resource.Invalid_tests);
                return this.RedirectToAction(c => c.Problem(id));
            }

            int addedTestsCount;

            using (var scope = TransactionsHelper.CreateTransactionScope(IsolationLevel.RepeatableRead))
            {
                this.submissionsData.RemoveTestRunsCacheByProblem(problem.Id);

                if (deleteOldFiles)
                {
                    this.testRunsData.DeleteByProblem(problem.Id);
                    this.testsData.DeleteByProblem(problem.Id);
                }

                addedTestsCount = ZippedTestsParser.AddTestsToProblem(problem, parsedTests);

                this.problemsData.Update(problem);

                if (retestTask)
                {
                    this.problemsBusiness.RetestById(problem.Id);
                }

                scope.Complete();
            }

            this.TempData.AddInfoMessage(string.Format(Resource.Tests_added_to_problem, addedTestsCount));
            return this.RedirectToAction(c => c.Problem(id));
        }

        /// <summary>
        /// Creates zip files with all tests in given task
        /// </summary>
        /// <param name="id">Task id</param>
        /// <returns>Zip file containing all tests in format {task}.{testNum}[.{zeroNum}].{in|out}.txt</returns>
        public ActionResult Export(int id)
        {
            var problem = this.problemsData.GetById(id);

            if (problem == null)
            {
                this.TempData.AddDangerMessage(Resource.Problem_does_not_exist);
                return this.RedirectToAction(c => c.Index());
            }

            if (!this.CheckIfUserHasProblemPermissions(id))
            {
                this.TempData.AddDangerMessage(GeneralResource.No_privileges_message);
                return this.Json("No premissions");
            }

            var tests = problem.Tests.OrderBy(x => x.OrderBy);

            var zipFileName = $"{problem.Name}_Tests_{DateTime.Now}";
            var zipFile = new ZipFile(zipFileName);

            using (zipFile)
            {
                var trialTestCounter = 1;
                var openTestCounter = 1;
                var testCounter = 1;

                foreach (var test in tests)
                {
                    var inputTestName = $"test.{testCounter:D3}{TestInputTxtFileExtension}";
                    var outputTestName = $"test.{testCounter:D3}{TestOutputTxtFileExtension}";

                    if (test.IsTrialTest)
                    {
                        inputTestName = $"test{ZeroTestStandardSignature}{trialTestCounter:D3}{TestInputTxtFileExtension}";
                        outputTestName = $"test{ZeroTestStandardSignature}{trialTestCounter:D3}{TestOutputTxtFileExtension}";
                        trialTestCounter++;
                    }
                    else if (test.IsOpenTest)
                    {
                        inputTestName = $"test{OpenTestStandardSignature}{openTestCounter:D3}{TestInputTxtFileExtension}";
                        outputTestName = $"test{OpenTestStandardSignature}{openTestCounter:D3}{TestOutputTxtFileExtension}";
                        openTestCounter++;
                    }
                    else
                    {
                        testCounter++;
                    }

                    zipFile.AddEntry(inputTestName, test.InputDataAsString, Encoding.UTF8);
                    zipFile.AddEntry(outputTestName, test.OutputDataAsString, Encoding.UTF8);
                }
            }

            var stream = new MemoryStream();

            zipFile.Save(stream);
            stream.Position = 0;

            var exportedTests = this.File(
                stream,
                MediaTypeNames.Application.Zip,
                $"{zipFileName}{Constants.ZipFileExtension}");

            return exportedTests;
        }

        [HttpGet]
        public FileResult ExportToExcel([DataSourceRequest] DataSourceRequest request, int id) =>
            this.ExportToExcel(request, this.GetData(id));

        private static void ImportAllTypesInTest(TestViewModel test) =>
            test.AllTypes = Enum.GetValues(typeof(TestType))
                .Cast<TestType>()
                .Select(tt => new SelectListItem
                {
                    Text = tt.GetLocalizedDescription(),
                    Value = ((int)tt).ToString(CultureInfo.InvariantCulture),
                    Selected = tt == test.Type
                });

        private IEnumerable GetData(int id)
        {
            if (!this.CheckIfUserHasProblemPermissions(id))
            {
                return new List<TestViewModel>();
            }

            var result = this.testsData
                .GetAllByProblem(id)
                .OrderByDescending(test => test.IsTrialTest)
                .ThenBy(test => test.OrderBy)
                .Select(TestViewModel.FromTest);

            return result;
        }
    }
}