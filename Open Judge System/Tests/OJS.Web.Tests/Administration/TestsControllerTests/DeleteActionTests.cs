namespace OJS.Web.Tests.Administration.TestsControllerTests
{
    using System.Web.Mvc;
    
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Web.Areas.Administration.ViewModels;

    [TestClass]
    public class DeleteActionTests : TestsControllerBaseTestsClass
    {
        [TestMethod]
        public void DeleteActionShouldReturnProperRedirectWhenIdIsNull()
        {
            var redirectResult = this.testsController.Delete(null) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
        }

        [TestMethod]
        public void DeleteActionShouldReturnProperMessageWhenIdIsNull()
        {
            var redirectResult = this.testsController.Delete(null) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.testsController.TempData.ContainsKey("DangerMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.testsController.TempData["DangerMessage"];
            Assert.AreEqual("Невалиден тест", tempDataMessage);
        }

        [TestMethod]
        public void DeleteActionShouldReturnProperRedirectWhenTestIsNull()
        {
            var redirectResult = this.testsController.Delete(100) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
        }

        [TestMethod]
        public void DeleteActionShouldReturnProperMessageWhenTestIsNull()
        {
            var redirectResult = this.testsController.Delete(100) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.testsController.TempData.ContainsKey("DangerMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.testsController.TempData["DangerMessage"];
            Assert.AreEqual("Невалиден тест", tempDataMessage);
        }

        [TestMethod]
        public void DeleteActionShouldReturnProperViewModelWhenIdIsCorrect()
        {
            var viewResult = this.testsController.Delete(1) as ViewResult;
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
            var redirectResult = this.testsController.ConfirmDelete(null) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
        }

        [TestMethod]
        public void ConfirmDeleteActionShouldReturnProperMessageWhenIdIsNull()
        {
            var redirectResult = this.testsController.ConfirmDelete(null) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.testsController.TempData.ContainsKey("DangerMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.testsController.TempData["DangerMessage"];
            Assert.AreEqual("Невалиден тест", tempDataMessage);
        }

        [TestMethod]
        public void ConfirmDeleteActionShouldReturnProperRedirectWhenTestIsNull()
        {
            var redirectResult = this.testsController.ConfirmDelete(100) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
        }

        [TestMethod]
        public void ConfirmDeleteActionShouldReturnProperMessageWhenTestIsNull()
        {
            var redirectResult = this.testsController.ConfirmDelete(100) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.testsController.TempData.ContainsKey("DangerMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.testsController.TempData["DangerMessage"];
            Assert.AreEqual("Невалиден тест", tempDataMessage);
        }

        [TestMethod]
        public void ConfirmDeleteActionShouldReturnProperRedirectWhenTestIsCorrect()
        {
            var redirectResult = this.testsController.ConfirmDelete(1) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Problem", redirectResult.RouteValues["action"]);

            var tempDataHasKey = this.testsController.TempData.ContainsKey("InfoMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.testsController.TempData["InfoMessage"];
            Assert.AreEqual("Теста беше изтрит успешно", tempDataMessage);
        }

        [TestMethod]
        public void DeleteAllActionShouldReturnProperRedirectWhenIdIsNull()
        {
            var redirectResult = this.testsController.DeleteAll(null) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
        }

        [TestMethod]
        public void DeleteAllActionShouldReturnProperMessageWhenIdIsNull()
        {
            var redirectResult = this.testsController.DeleteAll(null) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.testsController.TempData.ContainsKey("DangerMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.testsController.TempData["DangerMessage"];
            Assert.AreEqual("Невалидна задача", tempDataMessage);
        }

        [TestMethod]
        public void DeleteAllActionShouldReturnProperRedirectWhenTestIsNull()
        {
            var redirectResult = this.testsController.DeleteAll(100) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
        }

        [TestMethod]
        public void DeleteAllActionShouldReturnProperMessageWhenTestIsNull()
        {
            var redirectResult = this.testsController.DeleteAll(100) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.testsController.TempData.ContainsKey("DangerMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.testsController.TempData["DangerMessage"];
            Assert.AreEqual("Невалидна задача", tempDataMessage);
        }

        [TestMethod]
        public void DeleteAllActionShouldReturnProperViewModelWhenIdIsCorrect()
        {
            var viewResult = this.testsController.DeleteAll(1) as ViewResult;
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
            var redirectResult = this.testsController.ConfirmDeleteAll(null) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
        }

        [TestMethod]
        public void ConfirmDeleteAllActionShouldReturnProperMessageWhenIdIsNull()
        {
            var redirectResult = this.testsController.ConfirmDeleteAll(null) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.testsController.TempData.ContainsKey("DangerMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.testsController.TempData["DangerMessage"];
            Assert.AreEqual("Невалидна задача", tempDataMessage);
        }

        [TestMethod]
        public void ConfirmDeleteAllActionShouldReturnProperRedirectWhenTestIsNull()
        {
            var redirectResult = this.testsController.ConfirmDeleteAll(100) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
        }

        [TestMethod]
        public void ConfirmDeleteAllActionShouldReturnProperMessageWhenTestIsNull()
        {
            var redirectResult = this.testsController.ConfirmDeleteAll(100) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.testsController.TempData.ContainsKey("DangerMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.testsController.TempData["DangerMessage"];
            Assert.AreEqual("Невалидна задача", tempDataMessage);
        }

        [TestMethod]
        public void ConfirmDeleteAllActionShouldReturnProperRedirectWhenTestIsCorrect()
        {
            var redirectResult = this.testsController.ConfirmDeleteAll(1) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Problem", redirectResult.RouteValues["action"]);

            var tempDataHasKey = this.testsController.TempData.ContainsKey("InfoMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.testsController.TempData["InfoMessage"];
            Assert.AreEqual("Тестовете бяха изтрити успешно", tempDataMessage);
        }
    }
}
