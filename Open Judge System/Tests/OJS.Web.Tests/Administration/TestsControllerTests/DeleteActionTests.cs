namespace OJS.Web.Tests.Administration.TestsControllerTests
{
    using System.Web.Mvc;
    
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Web.Areas.Administration.ViewModels;
    using OJS.Web.Areas.Administration.ViewModels.Problem;
    using OJS.Web.Areas.Administration.ViewModels.Test;

    [TestClass]
    public class DeleteActionTests : TestsControllerBaseTestsClass
    {
        [TestMethod]
        public void DeleteActionShouldReturnProperRedirectWhenIdIsNull()
        {
            var redirectResult = this.TestsController.Delete(null) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
        }

        [TestMethod]
        public void DeleteActionShouldReturnProperMessageWhenIdIsNull()
        {
            var redirectResult = this.TestsController.Delete(null) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.TestsController.TempData.ContainsKey("DangerMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.TestsController.TempData["DangerMessage"];
            Assert.AreEqual("Невалиден тест", tempDataMessage);
        }

        [TestMethod]
        public void DeleteActionShouldReturnProperRedirectWhenTestIsNull()
        {
            var redirectResult = this.TestsController.Delete(100) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
        }

        [TestMethod]
        public void DeleteActionShouldReturnProperMessageWhenTestIsNull()
        {
            var redirectResult = this.TestsController.Delete(100) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.TestsController.TempData.ContainsKey("DangerMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.TestsController.TempData["DangerMessage"];
            Assert.AreEqual("Невалиден тест", tempDataMessage);
        }

        [TestMethod]
        public void DeleteActionShouldReturnProperViewModelWhenIdIsCorrect()
        {
            var viewResult = this.TestsController.Delete(1) as ViewResult;
            Assert.IsNotNull(viewResult);

            var model = viewResult.Model as TestViewModel;
            Assert.IsNotNull(model);

            Assert.AreEqual("Sample test input", model.InputFull);
            Assert.AreEqual("Sample test output", model.OutputFull);
            Assert.AreEqual(false, model.IsTrialTest);
            Assert.AreEqual(1, model.ProblemId);
            Assert.AreEqual(5, model.OrderBy);
        }

        [TestMethod]
        public void ConfirmDeleteActionShouldReturnProperRedirectWhenIdIsNull()
        {
            var redirectResult = this.TestsController.ConfirmDelete(null) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
        }

        [TestMethod]
        public void ConfirmDeleteActionShouldReturnProperMessageWhenIdIsNull()
        {
            var redirectResult = this.TestsController.ConfirmDelete(null) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.TestsController.TempData.ContainsKey("DangerMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.TestsController.TempData["DangerMessage"];
            Assert.AreEqual("Невалиден тест", tempDataMessage);
        }

        [TestMethod]
        public void ConfirmDeleteActionShouldReturnProperRedirectWhenTestIsNull()
        {
            var redirectResult = this.TestsController.ConfirmDelete(100) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
        }

        [TestMethod]
        public void ConfirmDeleteActionShouldReturnProperMessageWhenTestIsNull()
        {
            var redirectResult = this.TestsController.ConfirmDelete(100) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.TestsController.TempData.ContainsKey("DangerMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.TestsController.TempData["DangerMessage"];
            Assert.AreEqual("Невалиден тест", tempDataMessage);
        }

        [TestMethod]
        public void ConfirmDeleteActionShouldReturnProperRedirectWhenTestIsCorrect()
        {
            var redirectResult = this.TestsController.ConfirmDelete(1) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Problem", redirectResult.RouteValues["action"]);

            var tempDataHasKey = this.TestsController.TempData.ContainsKey("InfoMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.TestsController.TempData["InfoMessage"];
            Assert.AreEqual("Теста беше изтрит успешно", tempDataMessage);
        }

        [TestMethod]
        public void DeleteAllActionShouldReturnProperRedirectWhenIdIsNull()
        {
            var redirectResult = this.TestsController.DeleteAll(null) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
        }

        [TestMethod]
        public void DeleteAllActionShouldReturnProperMessageWhenIdIsNull()
        {
            var redirectResult = this.TestsController.DeleteAll(null) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.TestsController.TempData.ContainsKey("DangerMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.TestsController.TempData["DangerMessage"];
            Assert.AreEqual("Невалидна задача", tempDataMessage);
        }

        [TestMethod]
        public void DeleteAllActionShouldReturnProperRedirectWhenTestIsNull()
        {
            var redirectResult = this.TestsController.DeleteAll(100) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
        }

        [TestMethod]
        public void DeleteAllActionShouldReturnProperMessageWhenTestIsNull()
        {
            var redirectResult = this.TestsController.DeleteAll(100) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.TestsController.TempData.ContainsKey("DangerMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.TestsController.TempData["DangerMessage"];
            Assert.AreEqual("Невалидна задача", tempDataMessage);
        }

        [TestMethod]
        public void DeleteAllActionShouldReturnProperViewModelWhenIdIsCorrect()
        {
            var viewResult = this.TestsController.DeleteAll(1) as ViewResult;
            Assert.IsNotNull(viewResult);

            var model = viewResult.Model as ProblemViewModel;
            Assert.IsNotNull(model);

            Assert.AreEqual("Problem", model.Name);
            Assert.AreEqual("Contest", model.ContestName);
            Assert.AreEqual(1, model.Id);
        }

        [TestMethod]
        public void ConfirmDeleteAllActionShouldReturnProperRedirectWhenIdIsNull()
        {
            var redirectResult = this.TestsController.ConfirmDeleteAll(null) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
        }

        [TestMethod]
        public void ConfirmDeleteAllActionShouldReturnProperMessageWhenIdIsNull()
        {
            var redirectResult = this.TestsController.ConfirmDeleteAll(null) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.TestsController.TempData.ContainsKey("DangerMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.TestsController.TempData["DangerMessage"];
            Assert.AreEqual("Невалидна задача", tempDataMessage);
        }

        [TestMethod]
        public void ConfirmDeleteAllActionShouldReturnProperRedirectWhenTestIsNull()
        {
            var redirectResult = this.TestsController.ConfirmDeleteAll(100) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
        }

        [TestMethod]
        public void ConfirmDeleteAllActionShouldReturnProperMessageWhenTestIsNull()
        {
            var redirectResult = this.TestsController.ConfirmDeleteAll(100) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.TestsController.TempData.ContainsKey("DangerMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.TestsController.TempData["DangerMessage"];
            Assert.AreEqual("Невалидна задача", tempDataMessage);
        }

        [TestMethod]
        public void ConfirmDeleteAllActionShouldReturnProperRedirectWhenTestIsCorrect()
        {
            var redirectResult = this.TestsController.ConfirmDeleteAll(1) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Problem", redirectResult.RouteValues["action"]);

            var tempDataHasKey = this.TestsController.TempData.ContainsKey("InfoMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.TestsController.TempData["InfoMessage"];
            Assert.AreEqual("Тестовете бяха изтрити успешно", tempDataMessage);
        }
    }
}
