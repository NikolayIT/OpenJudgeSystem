namespace OJS.Web.Tests.Routes
{
    using NUnit.Framework;

    using OJS.Web.Areas.Administration;

    [TestFixture]
    public class AdministrationRoutesTests : RoutesTestsBase
    {
        [Test]
        public void AdministrationNavigationShouldReturnProperControllerAndAction()
        {
            var routeData = this.GetAreaRouteData(
                "~/Administration/Navigation",
                new AdministrationAreaAreaRegistration());

            Assert.IsNotNull(routeData);
            Assert.AreEqual("Navigation", routeData.Values["controller"]);
        }

        [Test]
        public void AdministrationNewsShouldReturnProperControllerAndAction()
        {
            var routeData = this.GetAreaRouteData(
                "~/Administration/News",
                new AdministrationAreaAreaRegistration());

            Assert.IsNotNull(routeData);
            Assert.AreEqual("News", routeData.Values["controller"]);
        }

        [Test]
        public void AdministrationContestsShouldReturnProperControllerAndAction()
        {
            var routeData = this.GetAreaRouteData(
                "~/Administration/Contests",
                new AdministrationAreaAreaRegistration());

            Assert.IsNotNull(routeData);
            Assert.AreEqual("Contests", routeData.Values["controller"]);
        }

        [Test]
        public void AdministrationContestsCategoriesReturnProperControllerAndAction()
        {
            var routeData = this.GetAreaRouteData(
                "~/Administration/ContestCategories",
                new AdministrationAreaAreaRegistration());

            Assert.IsNotNull(routeData);
            Assert.AreEqual("ContestCategories", routeData.Values["controller"]);
        }

        [Test]
        public void AdministrationContestsCategoriesHierarchyReturnProperControllerAndAction()
        {
            var routeData = this.GetAreaRouteData(
                "~/Administration/ContestCategories/Hierarchy",
                new AdministrationAreaAreaRegistration());

            Assert.IsNotNull(routeData);
            Assert.AreEqual("ContestCategories", routeData.Values["controller"]);
            Assert.AreEqual("Hierarchy", routeData.Values["action"]);
        }
    }
}
