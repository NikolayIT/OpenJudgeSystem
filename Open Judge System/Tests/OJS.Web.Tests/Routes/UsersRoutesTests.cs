namespace OJS.Web.Tests.Routes
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Common;
    using OJS.Web.Areas.Users;

    [TestClass]
    public class UsersRoutesTests : RoutesTestsBase
    {
        [TestMethod]
        public void CurrentUserProfileLinkShouldNavigateProperly()
        {
            var routeData = this.GetAreaRouteData("~/Users/Profile", new UsersAreaAreaRegistration());

            Assert.IsNotNull(routeData);
            Assert.AreEqual("Profile", routeData.Values["controller"]);
            Assert.AreEqual(GlobalConstants.Index, routeData.Values["action"]);
        }

        [TestMethod]
        public void SettingsLinkShouldNavigateProperly()
        {
            var routeData = this.GetAreaRouteData("~/Users/Settings", new UsersAreaAreaRegistration());

            Assert.IsNotNull(routeData);
            Assert.AreEqual("Settings", routeData.Values["controller"]);
            Assert.AreEqual(GlobalConstants.Index, routeData.Values["action"]);
        }

        [TestMethod]
        public void UserProfileLinkShouldNavigateProperly()
        {
            var routeData = this.GetAreaRouteData("~/Users/Nikolay.IT", new UsersAreaAreaRegistration());

            Assert.IsNotNull(routeData);
            Assert.AreEqual("Profile", routeData.Values["controller"]);
            Assert.AreEqual(GlobalConstants.Index, routeData.Values["action"]);
            Assert.AreEqual("Nikolay.IT", routeData.Values["id"]);
        }
    }
}
