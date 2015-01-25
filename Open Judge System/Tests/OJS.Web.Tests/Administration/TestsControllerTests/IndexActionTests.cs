namespace OJS.Web.Tests.Administration.TestsControllerTests
{
    using System.Web.Mvc;

    using NUnit.Framework;

    [TestFixture]
    public class IndexActionTests : TestsControllerBaseTestsClass
    {
        [Test]
        public void IndexActionShouldReturnViewWithNullModel()
        {
            var result = this.TestsController.Index() as ViewResult;
            var model = result.Model;

            Assert.IsNull(model);
        }
    }
}
