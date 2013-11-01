namespace OJS.Web.Tests.Administration.TestsControllerTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Script.Serialization;
    using System.ComponentModel.DataAnnotations;
    using System.Collections.Generic;
    using OJS.Web.Areas.Administration.ViewModels;

    [TestClass]
    public class AjaxOperationsTests : TestsControllerBaseTestsClass
    {
        [TestMethod]
        public void FullInputActionShouldReturnFullInputData()
        {
            var contentResult = this.testsController.FullInput(1) as ContentResult;
            Assert.IsNotNull(contentResult);

            Assert.AreEqual("Sample test input", contentResult.Content);
        }

        [TestMethod]
        public void FullOutputActionShouldReturnFullOutputData()
        {
            var contentResult = this.testsController.FullOutput(1) as ContentResult;
            Assert.IsNotNull(contentResult);

            Assert.AreEqual("Sample test output", contentResult.Content);
        }

        [TestMethod]
        public void GetTestRunsActionShouldReturnProperTestCount()
        {
            var jsonResult = this.testsController.GetTestRuns(1) as JsonResult;
            Assert.IsNotNull(jsonResult);

            var data = jsonResult.Data as IQueryable<TestRunViewModel>;

            Assert.AreEqual(1, data.Count());
        }

        [TestMethod]
        public void GetGetCascadeCategoriesShouldReturnProperCategoriesCount()
        {
            var jsonResult = this.testsController.GetCascadeCategories() as JsonResult;
            Assert.IsNotNull(jsonResult);

            var data = jsonResult.Data as IQueryable<object>;

            Assert.AreEqual(3, data.Count());
        }

        [TestMethod]
        public void GetGetCascadeContestsShouldReturnProperContestsCount()
        {
            var jsonResult = this.testsController.GetCascadeContests(1) as JsonResult;
            Assert.IsNotNull(jsonResult);

            var data = jsonResult.Data as IQueryable<object>;

            Assert.AreEqual(2, data.Count());
        }

        [TestMethod]
        public void GetGetCascadeProblemsShouldReturnProperProblemsCount()
        {
            var jsonResult = this.testsController.GetCascadeProblems(1) as JsonResult;
            Assert.IsNotNull(jsonResult);

            var data = jsonResult.Data as IQueryable<object>;

            Assert.AreEqual(4, data.Count());
        }

        [TestMethod]
        public void GetProblemInformacionShouldReturnProperIds()
        {
            var jsonResult = this.testsController.GetProblemInformation(1) as JsonResult;
            Assert.IsNotNull(jsonResult);

            var data = jsonResult.Data;

            var contest =  data.GetType().GetProperty("Contest").GetValue(data, null);
            var category = data.GetType().GetProperty("Category").GetValue(data, null);

            Assert.AreEqual(1, contest);
            Assert.AreEqual(1, category);
        }

        [TestMethod]
        public void GetSearchedProblemsShouldReturnProperProblemsCountIfTextIsValidAndCaseInsensitive()
        {
            var jsonResult = this.testsController.GetSearchedProblems("pro") as JsonResult;
            Assert.IsNotNull(jsonResult);

            var data = jsonResult.Data as IQueryable<object>;

            Assert.AreEqual(2, data.Count());
        }

        [TestMethod]
        public void GetSearchedProblemsShouldReturnProperProblemsCountIfTextIsValidAndParticular()
        {
            var jsonResult = this.testsController.GetSearchedProblems("other") as JsonResult;
            Assert.IsNotNull(jsonResult);

            var data = jsonResult.Data as IQueryable<object>;

            Assert.AreEqual(1, data.Count());
        }

        [TestMethod]
        public void GetSearchedProblemsShouldReturnProperProblemsCountIfTextIsNotValid()
        {
            var jsonResult = this.testsController.GetSearchedProblems("abv") as JsonResult;
            Assert.IsNotNull(jsonResult);

            var data = jsonResult.Data as IQueryable<object>;

            Assert.AreEqual(0, data.Count());
        }

        [TestMethod]
        public void ProblemTestsShouldContainProperTestsCount()
        {
            var contentResult = this.testsController.ProblemTests(1) as ContentResult;
            Assert.IsNotNull(contentResult);

            var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue, RecursionLimit = 100 };

            var data = serializer.Deserialize<List<TestViewModel>>(contentResult.Content);

            Assert.AreEqual(13, data.Count);
        }
    }
}
