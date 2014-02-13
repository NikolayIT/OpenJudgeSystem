namespace OJS.Web.Tests.Routes
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Common;

    [TestClass]
    public class FeedbackReportsRoutesTests : RoutesTestsBase
    {
        [TestMethod]
        public void FeedbackReportsShouldReturnProperRoute()
        {
            var routeData = this.GetRouteData("~/Feedback/");

            Assert.IsNotNull(routeData);
            Assert.AreEqual("Feedback", routeData.Values["controller"]);
            Assert.AreEqual(GlobalConstants.Index, routeData.Values["action"]);
        }

        [TestMethod]
        public void FeedbackReportsSubmitShouldReturnProperRoute()
        {
            var routeData = this.GetRouteData("~/Feedback/Submitted");

            Assert.IsNotNull(routeData);
            Assert.AreEqual("Feedback", routeData.Values["controller"]);
            Assert.AreEqual("Submitted", routeData.Values["action"]);
        }
    }
}
