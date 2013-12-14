namespace OJS.Web.Tests.Routes
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class OldSystemRoutesTests : RoutesTestsBase
    {
        [TestMethod]
        public void ContestListUrlShouldNavigateProperly()
        {
            var routeData = this.GetRouteData("~/Contest/List");

            Assert.IsNotNull(routeData);
            Assert.AreEqual("Redirects", routeData.Values["controller"]);
            Assert.AreEqual("Index", routeData.Values["action"]);
        }
    }
}
