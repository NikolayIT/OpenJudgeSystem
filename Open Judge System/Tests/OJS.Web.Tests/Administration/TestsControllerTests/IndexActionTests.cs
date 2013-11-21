namespace OJS.Web.Tests.Administration.TestsControllerTests
{
    using System;
    using System.Web.Mvc;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    
    using OJS.Web.Areas.Administration.Controllers;

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
