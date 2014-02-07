namespace OJS.Web.Tests.Administration.TestsControllerTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;
    
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Data.Contracts;
    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;
    using OJS.Web.Areas.Administration.Controllers;
    using OJS.Web.Areas.Administration.ViewModels.Test;
    
    [TestClass]
    public class TestsControllerBaseTestsClass : BaseWebTests
    {
        private readonly Mock<IOjsData> data;

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

            this.TestViewModel = new TestViewModel
            {
                Id = 1,
                InputFull = "Input test",
                OutputFull = "Output test",
                IsTrialTest = false,
                OrderBy = 1,
                ProblemId = 1,
            };

            //// TODO: get these mocks in base class for reuse

            var listsOfTests = new List<Test>(selectedProblem.Tests);

            this.data = new Mock<IOjsData>();
            this.Problems = new Mock<IDeletableEntityRepository<Problem>>();
            this.Tests = new Mock<ITestRepository>();
            this.TestsRuns = new Mock<ITestRunsRepository>();
            this.Categories = new Mock<IDeletableEntityRepository<ContestCategory>>();
            this.Contests = new Mock<IContestsRepository>();
            this.Submissions = new Mock<ISubmissionsRepository>();

            this.Problems.Setup(x => x.All()).Returns((new List<Problem>() { selectedProblem, otherProblem, problemWithOnlyTrialTests, problemWithOnlyNormalTests }).AsQueryable());

            this.Tests.Setup(x => x.All()).Returns(listsOfTests.AsQueryable());
            this.Tests.Setup(x => x.Add(It.IsAny<Test>())).Callback((Test t) => { listsOfTests.Add(t); });
            this.Tests.Setup(x => x.Delete(It.IsAny<int>())).Callback((int id) =>
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

            this.TestsRuns.Setup(x => x.All()).Returns(new List<TestRun>() { testRun }.AsQueryable());
            this.Categories.Setup(x => x.All()).Returns(new List<ContestCategory>() { firstCategory, secondCategory, thirdCategory }.AsQueryable());
            this.Contests.Setup(x => x.All()).Returns(new List<Contest>() { contest, otherContest }.AsQueryable());

            this.data.SetupGet(x => x.Problems).Returns(this.Problems.Object);
            this.data.SetupGet(x => x.Tests).Returns(this.Tests.Object);
            this.data.SetupGet(x => x.TestRuns).Returns(this.TestsRuns.Object);
            this.data.SetupGet(x => x.ContestCategories).Returns(this.Categories.Object);
            this.data.SetupGet(x => x.Contests).Returns(this.Contests.Object);
            this.data.SetupGet(x => x.Submissions).Returns(this.Submissions.Object);

            this.TestsController = new TestsController(this.data.Object);

            this.ControllerContext = new ControllerContext(this.MockHttpContestBase(), new RouteData(), this.TestsController);
        }

        protected Mock<IDeletableEntityRepository<ContestCategory>> Categories { get; set; }

        protected Mock<IDeletableEntityRepository<Problem>> Problems { get; set; }

        protected Mock<ITestRepository> Tests { get; set; }

        protected Mock<ITestRunsRepository> TestsRuns { get; set; }

        protected Mock<IContestsRepository> Contests { get; set; }

        protected Mock<ISubmissionsRepository> Submissions { get; set; }

        protected TestsController TestsController { get; set; }

        protected ControllerContext ControllerContext { get; set; }

        protected TestViewModel TestViewModel { get; set; }

        protected IOjsData Data
        {
            get
            {
                return this.data.Object;
            }
        }

        protected HttpContextBase MockHttpContestBase()
        {
            var mockHttpContext = new Mock<HttpContextBase>();
            var mockRequest = new Mock<HttpRequestBase>();
            mockHttpContext.SetupGet(x => x.Request).Returns(mockRequest.Object);

            return mockHttpContext.Object;
        }
    }
}
