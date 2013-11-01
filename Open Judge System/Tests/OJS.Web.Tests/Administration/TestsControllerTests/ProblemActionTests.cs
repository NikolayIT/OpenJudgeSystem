namespace OJS.Web.Tests.Administration.TestsControllerTests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.CSharp;
    using OJS.Web.Areas.Administration.Controllers;
    using System.Web.Mvc;

    [TestClass]
    public class ProblemActionTests : TestsControllerBaseTestsClass
    {
        [TestMethod]
        public void ProblemActionShouldReturnViewWithNullModel()
        {
            var result = this.testsController.Problem(null) as ViewResult;
            var model = result.Model;

            Assert.IsNull(model);
        }

        [TestMethod]
        public void ProblemActionShouldReturnNullViewBagIfIdIsNotDefined()
        {
            var view = this.testsController.Problem(null) as ViewResult;
            var viewBagProblem = view.ViewBag.ProblemId;

            Assert.IsNull(viewBagProblem);
        }

        [TestMethod]
        public void ProblemActionShouldReturnValueInViewBagIfIdDefined()
        {
            var view = this.testsController.Problem(10) as ViewResult;
            var viewBagProblem = view.ViewBag.ProblemId;

            Assert.AreEqual(10, viewBagProblem);
        }
    }
}
