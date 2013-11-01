namespace OJS.Web.Tests.Administration.TestsControllerTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using OJS.Data;
    using OJS.Data.Contracts;
    using OJS.Data.Models;
    using OJS.Tests.Common;
    using OJS.Web.Areas.Administration.Controllers;
    using OJS.Web.Areas.Administration.ViewModels;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;
    using OJS.Data.Repositories.Contracts;

    [TestClass]
    public class TestsControllerBaseTestsClass : BaseWebTests
    {
        protected Mock<IOjsData> data;

        protected Mock<IRepository<ContestCategory>> categories;

        protected Mock<IRepository<Problem>> problems;

        protected Mock<IRepository<Test>> tests;

        protected Mock<IRepository<TestRun>> testsRuns;

        protected Mock<IContestsRepository> contests;

        protected TestsController testsController;

        protected ControllerContext controllerContext;

        protected TestViewModel testViewModel;

        protected HttpContextBase MockHttpContestBase()
        {
            var mockHttpContext = new Mock<HttpContextBase>();
            var mockRequest = new Mock<HttpRequestBase>();
            mockHttpContext.SetupGet(x => x.Request).Returns(mockRequest.Object);

            return mockHttpContext.Object;
        }

        protected IOjsData Data
        {
            get
            {
                return this.data.Object;
            }
        }

        public TestsControllerBaseTestsClass()
        {
            var firstCategory = new ContestCategory()
            {
                Id = 1,
                Name = "Category"
            };

            var secondCategory = new ContestCategory()
            {
                Id = 2,
                Name = "Another category"
            };

            var thirdCategory = new ContestCategory()
            {
                Id = 3,
                Name = "And another category"
            };

            var contest = new Contest
            {
                Id = 1,
                Name = "Contest",
                CategoryId = 1,
                Category = firstCategory
            };

            var otherContest = new Contest
            {
                Id = 2,
                Name = "Other contest",
                CategoryId = 1,
                Category = firstCategory
            };

            firstCategory.Contests.Add(contest);
            firstCategory.Contests.Add(otherContest);

            var selectedProblem = new Problem
            {
                Id = 1,
                Name = "Problem",
                Contest = contest,
                ContestId = 1,
            };

            var otherProblem = new Problem
            {
                Id = 2,
                Name = "Other problem",
                Contest = contest,
                ContestId = 1,
            };

            var problemWithOnlyTrialTests = new Problem
            {
                Id = 3,
                Name = "OnlyTrialTests",
                Contest = contest,
                ContestId = 1,
            };

            var problemWithOnlyNormalTests = new Problem
            {
                Id = 4,
                Name = "OnlyNormalTests",
                Contest = contest,
                ContestId = 1,
            };

            var testRun = new TestRun()
            {
                Id = 1,
                TestId = 1,
                ResultType = TestRunResultType.CorrectAnswer,
                MemoryUsed = 100,
                TimeUsed = 100,
                SubmissionId = 1,
                ExecutionComment = "Comment execution",
                CheckerComment = "Comment checker",
                Submission = new Submission
                {
                    Id = 1,
                    CreatedOn = DateTime.Now,
                },
            };

            var test = new Test
            {
                Id = 1,
                InputDataAsString = "Sample test input",
                OutputDataAsString = "Sample test output",
                IsTrialTest = false,
                TestRuns = new HashSet<TestRun>()
                { 
                    testRun,
                },
                Problem = selectedProblem,
                ProblemId = 1,
                OrderBy = 5,
            };

            testRun.Test = test;

            selectedProblem.Tests.Add(test);

            selectedProblem.Tests.Add(new Test
            {
                InputDataAsString = "Trial input test 1",
                OutputDataAsString = "Trial output test 1",
                IsTrialTest = true,
                Problem = selectedProblem,
                ProblemId = 1,
            });

            selectedProblem.Tests.Add(new Test
            {
                InputDataAsString = "Trial input test 2",
                OutputDataAsString = "Trial output test 2",
                IsTrialTest = true,
                Problem = selectedProblem,
                ProblemId = 1,
            });

            problemWithOnlyTrialTests.Tests.Add(new Test
            {
                InputDataAsString = "Zero test 1\nZero test 1 second line",
                OutputDataAsString = "Zero test 1\nZero test 1 second lint output",
                IsTrialTest = true,
                Problem = selectedProblem,
                ProblemId = 1,
            });

            for (int i = 0; i < 10; i++)
            {
                selectedProblem.Tests.Add(new Test
                {
                    InputDataAsString = i.ToString(),
                    OutputDataAsString = (i + 1).ToString(),
                    IsTrialTest = false,
                    Problem = selectedProblem,
                    ProblemId = 1,
                });
            }

            otherProblem.Tests.Add(new Test
            {
                InputDataAsString = "Trial input test 1 other",
                OutputDataAsString = "Trial output test 1 other",
                IsTrialTest = true,
                Problem = selectedProblem,
                ProblemId = 1,
            });

            otherProblem.Tests.Add(new Test
            {
                InputDataAsString = "Trial input test 2 other",
                OutputDataAsString = "Trial output test 2 other",
                IsTrialTest = true,
                Problem = selectedProblem,
                ProblemId = 1,
            });

            for (int i = 0; i < 10; i++)
            {
                otherProblem.Tests.Add(new Test
                {
                    InputDataAsString = i.ToString() + "other",
                    OutputDataAsString = (i + 1).ToString() + "other",
                    IsTrialTest = false,
                    Problem = selectedProblem,
                    ProblemId = 1,
                });
            }

            for (int i = 0; i < 10; i++)
            {
                problemWithOnlyNormalTests.Tests.Add(new Test
                {
                    InputDataAsString = "Only normal tests " + i.ToString(),
                    OutputDataAsString = "Only normal tests output" + i.ToString(),
                    IsTrialTest = false,
                    Problem = selectedProblem,
                    ProblemId = 1,
                });
            }

            contest.Problems.Add(selectedProblem);
            contest.Problems.Add(otherProblem);
            contest.Problems.Add(problemWithOnlyTrialTests);
            contest.Problems.Add(problemWithOnlyNormalTests);

            this.testViewModel = new TestViewModel
            {
                Id = 1,
                InputFull = "Input test",
                OutputFull = "Output test",
                IsTrialTest = false,
                OrderBy = 1,
                ProblemId = 1,
            };

            // TODO: get these mocks in base class for reuse

            var listsOfTests = new List<Test>(selectedProblem.Tests);

            this.data = new Mock<IOjsData>();
            this.problems = new Mock<IRepository<Problem>>();
            this.tests = new Mock<IRepository<Test>>();
            this.testsRuns = new Mock<IRepository<TestRun>>();
            this.categories = new Mock<IRepository<ContestCategory>>();
            this.contests = new Mock<IContestsRepository>();

            this.problems.Setup(x => x.All()).Returns((new List<Problem>() { selectedProblem, otherProblem, problemWithOnlyTrialTests, problemWithOnlyNormalTests }).AsQueryable());

            this.tests.Setup(x => x.All()).Returns(listsOfTests.AsQueryable());
            this.tests.Setup(x => x.Add(It.IsAny<Test>())).Callback((Test t) => { listsOfTests.Add(t); });
            this.tests.Setup(x => x.Delete(It.IsAny<int>())).Callback((int id) => 
            {
                foreach (var currentTest in listsOfTests)
                {
                    if (currentTest.Id == id)
                    {
                        listsOfTests.Remove(currentTest);
                        break;
                    }
                }
            });

            this.testsRuns.Setup(x => x.All()).Returns(new List<TestRun>() { testRun }.AsQueryable());
            this.categories.Setup(x => x.All()).Returns(new List<ContestCategory>() { firstCategory, secondCategory, thirdCategory }.AsQueryable());
            this.contests.Setup(x => x.All()).Returns(new List<Contest>() { contest, otherContest }.AsQueryable());
            
            this.data.SetupGet(x => x.Problems).Returns(this.problems.Object);
            this.data.SetupGet(x => x.Tests).Returns(this.tests.Object);
            this.data.SetupGet(x => x.TestRuns).Returns(this.testsRuns.Object);
            this.data.SetupGet(x => x.ContestCategories).Returns(this.categories.Object);
            this.data.SetupGet(x => x.Contests).Returns(this.contests.Object);

            this.testsController = new TestsController(this.data.Object);

            this.controllerContext = new ControllerContext(this.MockHttpContestBase(), new RouteData(), this.testsController);
        }
    }
}
