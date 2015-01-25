namespace OJS.Web.Tests.Routes
{
    using NUnit.Framework;

    using OJS.Common;
    using OJS.Web.Areas.Users;

    [TestFixture]
    public class UsersRoutesTests : RoutesTestsBase
    {
        [Test]
        public void CurrentUserProfileLinkShouldNavigateProperly()
        {
            var routeData = this.GetAreaRouteData("~/Users/Profile", new UsersAreaAreaRegistration());

            Assert.IsNotNull(routeData);
            Assert.AreEqual("Profile", routeData.Values["controller"]);
            Assert.AreEqual(GlobalConstants.Index, routeData.Values["action"]);
        }

        [Test]
        public void SettingsLinkShouldNavigateProperly()
        {
            var routeData = this.GetAreaRouteData("~/Users/Settings", new UsersAreaAreaRegistration());

            Assert.IsNotNull(routeData);
            Assert.AreEqual("Settings", routeData.Values["controller"]);
            Assert.AreEqual(GlobalConstants.Index, routeData.Values["action"]);
        }

        [Test]
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
