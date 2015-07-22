namespace OJS.Web.Tests.Administration.TestsControllerTests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Script.Serialization;

    using NUnit.Framework;

    using OJS.Web.Areas.Administration.ViewModels.Test;
    using OJS.Web.Areas.Administration.ViewModels.TestRun;

    [TestFixture]
    public class AjaxOperationsTests : TestsControllerBaseTestsClass
    {
        [Test]
        public void FullInputActionShouldReturnFullInputData()
        {
            var contentResult = this.TestsController.FullInput(1) as ContentResult;
            Assert.IsNotNull(contentResult);

            Assert.AreEqual("Sample test input", contentResult.Content);
        }

        [Test]
        public void FullOutputActionShouldReturnFullOutputData()
        {
            var contentResult = this.TestsController.FullOutput(1) as ContentResult;
            Assert.IsNotNull(contentResult);

            Assert.AreEqual("Sample test output", contentResult.Content);
        }

        [Test]
        public void GetTestRunsActionShouldReturnProperTestCount()
        {
            var jsonResult = this.TestsController.GetTestRuns(1);
            Assert.IsNotNull(jsonResult);

            var data = jsonResult.Data as IQueryable<TestRunViewModel>;

            Assert.AreEqual(1, data.Count());
        }

        [Test]
        public void GetGetCascadeCategoriesShouldReturnProperCategoriesCount()
        {
            var jsonResult = this.TestsController.GetCascadeCategories();
            Assert.IsNotNull(jsonResult);

            var data = jsonResult.Data as IQueryable<object>;

            Assert.AreEqual(3, data.Count());
        }

        [Test]
        public void GetGetCascadeContestsShouldReturnProperContestsCount()
        {
            var jsonResult = this.TestsController.GetCascadeContests(1);
            Assert.IsNotNull(jsonResult);

            var data = jsonResult.Data as IQueryable<object>;

            Assert.AreEqual(2, data.Count());
        }

        [Test]
        public void GetGetCascadeProblemsShouldReturnProperProblemsCount()
        {
            var jsonResult = this.TestsController.GetCascadeProblems(1);
            Assert.IsNotNull(jsonResult);

            var data = jsonResult.Data as IQueryable<object>;

            Assert.AreEqual(4, data.Count());
        }

        [Test]
        public void GetProblemInformacionShouldReturnProperIds()
        {
            var jsonResult = this.TestsController.GetProblemInformation(1);
            Assert.IsNotNull(jsonResult);

            var data = jsonResult.Data;

            var contest = data.GetType().GetProperty("Contest").GetValue(data, null);
            var category = data.GetType().GetProperty("Category").GetValue(data, null);

            Assert.AreEqual(1, contest);
            Assert.AreEqual(1, category);
        }

        [Test]
        public void GetSearchedProblemsShouldReturnProperProblemsCountIfTextIsValidAndCaseInsensitive()
        {
            var jsonResult = this.TestsController.GetSearchedProblems("pro");
            Assert.IsNotNull(jsonResult);

            var data = jsonResult.Data as IQueryable<object>;

            Assert.AreEqual(2, data.Count());
        }

        [Test]
        public void GetSearchedProblemsShouldReturnProperProblemsCountIfTextIsValidAndParticular()
        {
            var jsonResult = this.TestsController.GetSearchedProblems("other");
            Assert.IsNotNull(jsonResult);

            var data = jsonResult.Data as IQueryable<object>;

            Assert.AreEqual(1, data.Count());
        }

        [Test]
        public void GetSearchedProblemsShouldReturnProperProblemsCountIfTextIsNotValid()
        {
            var jsonResult = this.TestsController.GetSearchedProblems("abv");
            Assert.IsNotNull(jsonResult);

            var data = jsonResult.Data as IQueryable<object>;

            Assert.AreEqual(0, data.Count());
        }

        [Test]
        public void ProblemTestsShouldContainProperTestsCount()
        {
            var contentResult = this.TestsController.ProblemTests(1);
            Assert.IsNotNull(contentResult);

            var serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue, RecursionLimit = 100 };

            var data = serializer.Deserialize<List<TestViewModel>>(contentResult.Content);

            Assert.AreEqual(13, data.Count);
        }
    }
}
