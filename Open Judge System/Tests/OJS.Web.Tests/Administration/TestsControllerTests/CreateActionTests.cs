namespace OJS.Web.Tests.Administration.TestsControllerTests
{
    using System.Linq;
    using System.Web.Mvc;

    using NUnit.Framework;

    using OJS.Common;

    [TestFixture]
    public class CreateActionTests : TestsControllerBaseTestsClass
    {
        [Test]
        public void CreateGetActionShouldReturnProperRedirectWhenIdIsNull()
        {
            var redirectResult = this.TestsController.Create(null) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(GlobalConstants.Index, redirectResult.RouteValues["action"]);
        }

        [Test]
        public void CreateGetActionShouldReturnProperMessageWhenIdIsNull()
        {
            var redirectResult = this.TestsController.Create(null) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.TestsController.TempData.ContainsKey(GlobalConstants.DangerMessage);
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.TestsController.TempData[GlobalConstants.DangerMessage];
            Assert.AreEqual("Невалидна задача", tempDataMessage);
        }

        [Test]
        public void CreateGetActionShouldReturnProperRedirectWhenProblemIsNull()
        {
            var redirectResult = this.TestsController.Create(100) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(GlobalConstants.Index, redirectResult.RouteValues["action"]);
        }

        [Test]
        public void CreateGetActionShouldReturnProperMessageWhenProblemIsNull()
        {
            var redirectResult = this.TestsController.Create(100) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.TestsController.TempData.ContainsKey(GlobalConstants.DangerMessage);
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.TestsController.TempData[GlobalConstants.DangerMessage];
            Assert.AreEqual("Невалидна задача", tempDataMessage);
        }

        [Test]
        public void CreatePostActionShouldReturnProperRedirectAndMessageWhenProblemDoesNotExist()
        {
            var redirectResult = this.TestsController.Create(100, null) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.TestsController.TempData.ContainsKey(GlobalConstants.DangerMessage);
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.TestsController.TempData[GlobalConstants.DangerMessage];
            Assert.AreEqual("Невалидна задача", tempDataMessage);
        }

        [Test]
        public void CreatePostActionShouldReturnViewWhenPostedProblemIsNull()
        {
            var viewResult = this.TestsController.Create(1, null) as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.IsNull(viewResult.Model);
        }

        [Test]
        public void CreatePostActionShouldReturnProperMessageWhenModelStateIsValid()
        {
            var viewResult = this.TestsController.Create(1, this.TestViewModel) as RedirectToRouteResult;
            Assert.IsNotNull(viewResult);

            var tempDataHasKey = this.TestsController.TempData.ContainsKey(GlobalConstants.InfoMessage);
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.TestsController.TempData[GlobalConstants.InfoMessage];
            Assert.AreEqual("Теста беше добавен успешно", tempDataMessage);
        }

        [Test]
        public void CreatePostActionShouldReturnProperRedirectWhenModelStateIsValid()
        {
            var viewResult = this.TestsController.Create(1, this.TestViewModel) as RedirectToRouteResult;
            Assert.IsNotNull(viewResult);

            var routeAction = viewResult.RouteValues["action"];
            Assert.IsNotNull(routeAction);
            Assert.AreEqual("Problem", routeAction);

            var routeId = viewResult.RouteValues["id"];
            Assert.IsNotNull(routeAction);
            Assert.AreEqual(1, routeId);
        }

        [Test]
        public void CreatePostActionShouldNotAddTestIfInputIsNullOrEmpty()
        {
            this.TestsController.ViewData.ModelState.Clear();

            this.TestViewModel.InputFull = string.Empty;
            this.TryValidateModel(this.TestViewModel, this.TestsController);
            var viewResult = this.TestsController.Create(1, this.TestViewModel) as ViewResult;
            this.TestViewModel.InputFull = "Input test";

            var modelState = this.TestsController.ViewData.ModelState;

            Assert.IsNotNull(viewResult);
            Assert.IsTrue(modelState.Count == 2);
            Assert.IsTrue(modelState.ContainsKey("Input"));
            Assert.IsTrue(modelState.ContainsKey("InputFull"));
            Assert.IsTrue(modelState.Values.ToList()[0].Errors.First().ErrorMessage == "Входа е задължителен!");
            Assert.IsTrue(modelState.Values.ToList()[1].Errors.First().ErrorMessage == "Входа е задължителен!");
        }

        [Test]
        public void CreatePostActionShouldNotAddTestIfOutputIsNullOrEmpty()
        {
            this.TestsController.ViewData.ModelState.Clear();

            this.TestViewModel.OutputFull = string.Empty;
            this.TryValidateModel(this.TestViewModel, this.TestsController);
            var viewResult = this.TestsController.Create(1, this.TestViewModel) as ViewResult;
            this.TestViewModel.OutputFull = "Output test";

            var modelState = this.TestsController.ViewData.ModelState;

            Assert.IsNotNull(viewResult);
            Assert.IsTrue(modelState.Count == 2);
            Assert.IsTrue(modelState.ContainsKey("Output"));
            Assert.IsTrue(modelState.ContainsKey("OutputFull"));
            Assert.IsTrue(modelState.Values.ToList()[0].Errors.First().ErrorMessage == "Изхода е задължителен!");
            Assert.IsTrue(modelState.Values.ToList()[1].Errors.First().ErrorMessage == "Изхода е задължителен!");
        }
    }
}
