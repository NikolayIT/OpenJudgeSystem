namespace OJS.Web.Tests.Administration.TestsControllerTests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Web.Mvc;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Web.Areas.Administration.ViewModels;

    [TestClass]
    public class CreateActionTests : TestsControllerBaseTestsClass
    {
        [TestMethod]
        public void CreateGetActionShouldReturnProperRedirectWhenIdIsNull()
        {
            var redirectResult = this.testsController.Create(null) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
        }

        [TestMethod]
        public void CreateGetActionShouldReturnProperMessageWhenIdIsNull()
        {
            var redirectResult = this.testsController.Create(null) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.testsController.TempData.ContainsKey("DangerMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.testsController.TempData["DangerMessage"];
            Assert.AreEqual("Невалидна задача", tempDataMessage);
        }

        [TestMethod]
        public void CreateGetActionShouldReturnProperRedirectWhenProblemIsNull()
        {
            var redirectResult = this.testsController.Create(100) as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
        }

        [TestMethod]
        public void CreateGetActionShouldReturnProperMessageWhenProblemIsNull()
        {
            var redirectResult = this.testsController.Create(100) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.testsController.TempData.ContainsKey("DangerMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.testsController.TempData["DangerMessage"];
            Assert.AreEqual("Невалидна задача", tempDataMessage);
        }

        [TestMethod]
        public void CreateGetActionShouldContainProperViewBagEntriesOnCorrectProblem()
        {
            var viewResult = this.testsController.Create(1) as ViewResult;
            Assert.IsNotNull(viewResult);

            var viewBagProblemId = viewResult.ViewBag.ProblemId;
            Assert.IsNotNull(viewBagProblemId);
            Assert.AreEqual(1, viewBagProblemId);

            var viewBagProblemName = viewResult.ViewBag.ProblemName;
            Assert.IsNotNull(viewBagProblemName);
            Assert.AreEqual("Problem", viewBagProblemName);
        }

        [TestMethod]
        public void CreatePostActionShouldReturnProperRedirectAndMessageWhenProblemDoesNotExist()
        {
            var redirectResult = this.testsController.Create(100, null) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            var tempDataHasKey = this.testsController.TempData.ContainsKey("DangerMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.testsController.TempData["DangerMessage"];
            Assert.AreEqual("Невалидна задача", tempDataMessage);
        }

        [TestMethod]
        public void CreatePostActionShouldReturnViewWhenPostedProblemIsNull()
        {
            var viewResult = this.testsController.Create(1, null) as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.IsNull(viewResult.Model);
        }

        [TestMethod]
        public void CreatePostActionShouldReturnProperMessageWhenModelStateIsValid()
        {
            var viewResult = this.testsController.Create(1, this.testViewModel) as RedirectToRouteResult;
            Assert.IsNotNull(viewResult);

            var tempDataHasKey = this.testsController.TempData.ContainsKey("InfoMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.testsController.TempData["InfoMessage"];
            Assert.AreEqual("Теста беше добавен успешно", tempDataMessage);
        }

        [TestMethod]
        public void CreatePostActionShouldReturnProperRedirectWhenModelStateIsValid()
        {
            var viewResult = this.testsController.Create(1, this.testViewModel) as RedirectToRouteResult;
            Assert.IsNotNull(viewResult);

            var routeAction = viewResult.RouteValues["action"];
            Assert.IsNotNull(routeAction);
            Assert.AreEqual("Problem", routeAction);

            var routeId = viewResult.RouteValues["id"];
            Assert.IsNotNull(routeAction);
            Assert.AreEqual(1, routeId);
        }

        [TestMethod]
        public void CreatePostActionShouldNotAddTestIfInputIsNullOrEmpty()
        {
            this.testsController.ViewData.ModelState.Clear();

            this.testViewModel.InputFull = string.Empty;
            this.TryValidateModel(this.testViewModel, this.testsController);
            var viewResult = this.testsController.Create(1, this.testViewModel) as ViewResult;
            this.testViewModel.InputFull = "Input test";

            var modelState = this.testsController.ViewData.ModelState;

            Assert.IsNotNull(viewResult);
            Assert.IsTrue(modelState.Count == 2);
            Assert.IsTrue(modelState.ContainsKey("Input"));
            Assert.IsTrue(modelState.ContainsKey("InputFull"));
            Assert.IsTrue(modelState.Values.ToList()[0].Errors.First().ErrorMessage == "Входа е задължителен!");
            Assert.IsTrue(modelState.Values.ToList()[1].Errors.First().ErrorMessage == "Входа е задължителен!");
        }

        [TestMethod]
        public void CreatePostActionShouldNotAddTestIfOutputIsNullOrEmpty()
        {
            this.testsController.ViewData.ModelState.Clear();

            this.testViewModel.OutputFull = string.Empty;
            this.TryValidateModel(this.testViewModel, this.testsController);
            var viewResult = this.testsController.Create(1, this.testViewModel) as ViewResult;
            this.testViewModel.OutputFull = "Output test";

            var modelState = this.testsController.ViewData.ModelState;

            Assert.IsNotNull(viewResult);
            Assert.IsTrue(modelState.Count == 2);
            Assert.IsTrue(modelState.ContainsKey("Output"));
            Assert.IsTrue(modelState.ContainsKey("OutputFull"));
            Assert.IsTrue(modelState.Values.ToList()[0].Errors.First().ErrorMessage == "Изхода е задължителен!");
            Assert.IsTrue(modelState.Values.ToList()[1].Errors.First().ErrorMessage == "Изхода е задължителен!");
        }
    }
}
