namespace OJS.Web.Tests.Administration.TestsControllerTests
{
    using System.Web.Mvc;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ProblemActionTests : TestsControllerBaseTestsClass
    {
        [TestMethod]
        public void ProblemActionShouldReturnViewWithNullModel()
        {
            var result = this.TestsController.Problem(null) as ViewResult;
            var model = result.Model;

            Assert.IsNull(model);
        }

        [TestMethod]
        public void ProblemActionShouldReturnNullViewBagIfIdIsNotDefined()
        {
            var view = this.TestsController.Problem(null) as ViewResult;
            var viewBagProblem = view.ViewBag.ProblemId;

            Assert.IsNull(viewBagProblem);
        }

        [TestMethod]
        public void ProblemActionShouldReturnValueInViewBagIfIdDefined()
        {
            var view = this.TestsController.Problem(10) as ViewResult;
            var viewBagProblem = view.ViewBag.ProblemId;

            Assert.AreEqual(10, viewBagProblem);
        }
    }
}
