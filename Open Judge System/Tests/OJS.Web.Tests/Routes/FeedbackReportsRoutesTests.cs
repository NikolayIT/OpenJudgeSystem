namespace OJS.Web.Tests.Routes
{
    using NUnit.Framework;

    using OJS.Common;

    [TestFixture]
    public class FeedbackReportsRoutesTests : RoutesTestsBase
    {
        [Test]
        public void FeedbackReportsShouldReturnProperRoute()
        {
            var routeData = this.GetRouteData("~/Feedback/");

            Assert.IsNotNull(routeData);
            Assert.AreEqual("Feedback", routeData.Values["controller"]);
            Assert.AreEqual(GlobalConstants.Index, routeData.Values["action"]);
        }

        [Test]
        public void FeedbackReportsSubmitShouldReturnProperRoute()
        {
            var routeData = this.GetRouteData("~/Feedback/Submitted");

            Assert.IsNotNull(routeData);
            Assert.AreEqual("Feedback", routeData.Values["controller"]);
            Assert.AreEqual("Submitted", routeData.Values["action"]);
        }
    }
}
