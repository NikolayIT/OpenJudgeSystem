namespace OJS.Web.Tests.Administration.TestsControllerTests
{
    using System.Web.Mvc;

    using NUnit.Framework;

    using OJS.Common;
    using OJS.Web.Areas.Administration.ViewModels.Problem;
    using OJS.Web.Areas.Administration.ViewModels.Test;

    [TestFixture]
    public class DeleteActionTests : TestsControllerBaseTestsClass
    {
        [Test]
        public void DeleteActionShouldReturnProperRedirectWhenIdIsNull()
        {
            var redirectResult = this.TestsController.Delete(null) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(GlobalConstants.Index, redirectResult.RouteValues["action"]);
        }

        [Test]
        public void DeleteActionShouldReturnProperMessageWhenIdIsNull()
        {
            var redirectResult = this.TestsController.Delete(null) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.TestsController.TempData.ContainsKey(GlobalConstants.DangerMessage);
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.TestsController.TempData[GlobalConstants.DangerMessage];
            Assert.AreEqual("Невалиден тест", tempDataMessage);
        }

        [Test]
        public void DeleteActionShouldReturnProperRedirectWhenTestIsNull()
        {
            var redirectResult = this.TestsController.Delete(100) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(GlobalConstants.Index, redirectResult.RouteValues["action"]);
        }

        [Test]
        public void DeleteActionShouldReturnProperMessageWhenTestIsNull()
        {
            var redirectResult = this.TestsController.Delete(100) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.TestsController.TempData.ContainsKey(GlobalConstants.DangerMessage);
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.TestsController.TempData[GlobalConstants.DangerMessage];
            Assert.AreEqual("Невалиден тест", tempDataMessage);
        }

        [Test]
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

        [Test]
        public void ConfirmDeleteActionShouldReturnProperRedirectWhenIdIsNull()
        {
            var redirectResult = this.TestsController.ConfirmDelete(null) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(GlobalConstants.Index, redirectResult.RouteValues["action"]);
        }

        [Test]
        public void ConfirmDeleteActionShouldReturnProperMessageWhenIdIsNull()
        {
            var redirectResult = this.TestsController.ConfirmDelete(null) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.TestsController.TempData.ContainsKey(GlobalConstants.DangerMessage);
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.TestsController.TempData[GlobalConstants.DangerMessage];
            Assert.AreEqual("Невалиден тест", tempDataMessage);
        }

        [Test]
        public void ConfirmDeleteActionShouldReturnProperRedirectWhenTestIsNull()
        {
            var redirectResult = this.TestsController.ConfirmDelete(100) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(GlobalConstants.Index, redirectResult.RouteValues["action"]);
        }

        [Test]
        public void ConfirmDeleteActionShouldReturnProperMessageWhenTestIsNull()
        {
            var redirectResult = this.TestsController.ConfirmDelete(100) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.TestsController.TempData.ContainsKey(GlobalConstants.DangerMessage);
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.TestsController.TempData[GlobalConstants.DangerMessage];
            Assert.AreEqual("Невалиден тест", tempDataMessage);
        }

        [Test]
        public void ConfirmDeleteActionShouldReturnProperRedirectWhenTestIsCorrect()
        {
            var redirectResult = this.TestsController.ConfirmDelete(1) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Problem", redirectResult.RouteValues["action"]);

            var tempDataHasKey = this.TestsController.TempData.ContainsKey(GlobalConstants.InfoMessage);
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.TestsController.TempData[GlobalConstants.InfoMessage];
            Assert.AreEqual("Теста беше изтрит успешно", tempDataMessage);
        }

        [Test]
        public void DeleteAllActionShouldReturnProperRedirectWhenIdIsNull()
        {
            var redirectResult = this.TestsController.DeleteAll(null) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(GlobalConstants.Index, redirectResult.RouteValues["action"]);
        }

        [Test]
        public void DeleteAllActionShouldReturnProperMessageWhenIdIsNull()
        {
            var redirectResult = this.TestsController.DeleteAll(null) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.TestsController.TempData.ContainsKey(GlobalConstants.DangerMessage);
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.TestsController.TempData[GlobalConstants.DangerMessage];
            Assert.AreEqual("Невалидна задача", tempDataMessage);
        }

        [Test]
        public void DeleteAllActionShouldReturnProperRedirectWhenTestIsNull()
        {
            var redirectResult = this.TestsController.DeleteAll(100) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(GlobalConstants.Index, redirectResult.RouteValues["action"]);
        }

        [Test]
        public void DeleteAllActionShouldReturnProperMessageWhenTestIsNull()
        {
            var redirectResult = this.TestsController.DeleteAll(100) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.TestsController.TempData.ContainsKey(GlobalConstants.DangerMessage);
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.TestsController.TempData[GlobalConstants.DangerMessage];
            Assert.AreEqual("Невалидна задача", tempDataMessage);
        }

        [Test]
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

        [Test]
        public void ConfirmDeleteAllActionShouldReturnProperRedirectWhenIdIsNull()
        {
            var redirectResult = this.TestsController.ConfirmDeleteAll(null) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(GlobalConstants.Index, redirectResult.RouteValues["action"]);
        }

        [Test]
        public void ConfirmDeleteAllActionShouldReturnProperMessageWhenIdIsNull()
        {
            var redirectResult = this.TestsController.ConfirmDeleteAll(null) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.TestsController.TempData.ContainsKey(GlobalConstants.DangerMessage);
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.TestsController.TempData[GlobalConstants.DangerMessage];
            Assert.AreEqual("Невалидна задача", tempDataMessage);
        }

        [Test]
        public void ConfirmDeleteAllActionShouldReturnProperRedirectWhenTestIsNull()
        {
            var redirectResult = this.TestsController.ConfirmDeleteAll(100) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(GlobalConstants.Index, redirectResult.RouteValues["action"]);
        }

        [Test]
        public void ConfirmDeleteAllActionShouldReturnProperMessageWhenTestIsNull()
        {
            var redirectResult = this.TestsController.ConfirmDeleteAll(100) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.TestsController.TempData.ContainsKey(GlobalConstants.DangerMessage);
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.TestsController.TempData[GlobalConstants.DangerMessage];
            Assert.AreEqual("Невалидна задача", tempDataMessage);
        }

        [Test]
        public void ConfirmDeleteAllActionShouldReturnProperRedirectWhenTestIsCorrect()
        {
            var redirectResult = this.TestsController.ConfirmDeleteAll(1) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Problem", redirectResult.RouteValues["action"]);

            var tempDataHasKey = this.TestsController.TempData.ContainsKey(GlobalConstants.InfoMessage);
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.TestsController.TempData[GlobalConstants.InfoMessage];
            Assert.AreEqual("Тестовете бяха изтрити успешно", tempDataMessage);
        }
    }
}
