namespace OJS.Web.Tests.Administration.TestsControllerTests
{
    using System.Web.Mvc;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    
    using OJS.Web.Areas.Administration.ViewModels;
    using OJS.Web.Areas.Administration.ViewModels.Test;

    [TestClass]
    public class DetailsActionTests : TestsControllerBaseTestsClass
    {
        [TestMethod]
        public void DetailsActionShouldReturnProperRedirectWhenIdIsNull()
        {
            var redirectResult = this.testsController.Details(null) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
        }

        [TestMethod]
        public void DetailsActionShouldReturnProperMessageWhenIdIsNull()
        {
            var redirectResult = this.testsController.Details(null) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.testsController.TempData.ContainsKey("DangerMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.testsController.TempData["DangerMessage"];
            Assert.AreEqual("Невалиден тест", tempDataMessage);
        }

        [TestMethod]
        public void DetailsActionShouldReturnProperRedirectWhenTestIsNull()
        {
            var redirectResult = this.testsController.Details(100) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
        }

        [TestMethod]
        public void DetailsActionShouldReturnProperMessageWhenTestIsNull()
        {
            var redirectResult = this.testsController.Details(100) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.testsController.TempData.ContainsKey("DangerMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.testsController.TempData["DangerMessage"];
            Assert.AreEqual("Невалиден тест", tempDataMessage);
        }

        [TestMethod]
        public void DetailsActionShouldReturnProperViewModelWhenIdIsCorrect()
        {
            var viewResult = this.testsController.Details(1) as ViewResult;
            Assert.IsNotNull(viewResult);

            var model = viewResult.Model as TestViewModel;
            Assert.IsNotNull(model);

            Assert.AreEqual("Sample test input", model.InputFull);
            Assert.AreEqual("Sample test output", model.OutputFull);
            Assert.AreEqual(false, model.IsTrialTest);
            Assert.AreEqual(1, model.ProblemId);
            Assert.AreEqual(5, model.OrderBy);
        }
    }
}
