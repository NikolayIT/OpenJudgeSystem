namespace OJS.Web.Tests.Administration.TestsControllerTests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OJS.Web.Areas.Administration.Controllers;
    using System.Web.Mvc;

    [TestClass]
    public class IndexActionTests : TestsControllerBaseTestsClass
    {
        [TestMethod]
        public void IndexActionShouldReturnViewWithNullModel()
        {
            var result = this.testsController.Index() as ViewResult;
            var model = result.Model;

            Assert.IsNull(model);
        }
    }
}
