namespace OJS.Web.Tests.Administration.TestsControllerTests
{
    using System;
    using System.Linq;
    using System.Web.Mvc;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Common;
    using OJS.Web.Areas.Administration.ViewModels;
    using OJS.Web.Areas.Administration.ViewModels.Test;

    [TestClass]
    public class EditActionTests : TestsControllerBaseTestsClass
    {
        [TestMethod]
        public void EditGetActionShouldReturnProperMessageAndRedirectWhenTestIsNull()
        {
            var redirectResult = this.TestsController.Edit(2) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(GlobalConstants.Index, redirectResult.RouteValues["action"]);

            var tempDataHasKey = this.TestsController.TempData.ContainsKey(GlobalConstants.DangerMessage);
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.TestsController.TempData[GlobalConstants.DangerMessage];
            Assert.AreEqual("Невалиден тест", tempDataMessage);
        }

        [TestMethod]
        public void EditGetActionShouldReturnProperViewModelWhenTestIsValid()
        {
            var viewResult = this.TestsController.Edit(1) as ViewResult;
            Assert.IsNotNull(viewResult);

            var model = viewResult.Model as TestViewModel;
            Assert.IsNotNull(model);

            Assert.AreEqual(1, model.Id);
            Assert.AreEqual(5, model.OrderBy);
            Assert.AreEqual("Sample test input", model.InputFull);
            Assert.AreEqual("Sample test output", model.OutputFull);
            Assert.AreEqual("Problem", model.ProblemName);
            Assert.AreEqual(1, model.TestRunsCount);
        }

        [TestMethod]
        public void EditPostActionShouldReturnProperMessageAndRedirectWhenTestDoesNotExist()
        {
            var redirectResult = this.TestsController.Edit(2, new TestViewModel()) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Problem", redirectResult.RouteValues["action"]);

            var tempDataHasKey = this.TestsController.TempData.ContainsKey(GlobalConstants.DangerMessage);
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.TestsController.TempData[GlobalConstants.DangerMessage];
            Assert.AreEqual("Невалиден тест", tempDataMessage);
        }

        [TestMethod]
        public void EditPostActionShouldReturnViewWithNullModelWhenPostedTestViewModelIsNull()
        {
            var viewResult = this.TestsController.Edit(1, null) as ViewResult;
            Assert.IsNotNull(viewResult);

            var model = viewResult.Model as TestViewModel;
            Assert.IsNull(model);
        }

        [TestMethod]
        public void EditPostActionShouldReturnProperRedirectAndMessageWhenPostedTestIsValid()
        {
            var redirectResult = this.TestsController.Edit(1, this.TestViewModel) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Problem", redirectResult.RouteValues["action"]);
            Assert.AreEqual(1, redirectResult.RouteValues["id"]);

            var tempDataHasKey = this.TestsController.TempData.ContainsKey(GlobalConstants.InfoMessage);
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.TestsController.TempData[GlobalConstants.InfoMessage];
            Assert.AreEqual("Теста беше променен успешно", tempDataMessage);
        }
    }
}
