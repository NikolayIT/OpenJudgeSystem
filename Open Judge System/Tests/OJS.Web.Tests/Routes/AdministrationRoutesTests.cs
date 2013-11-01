namespace OJS.Web.Tests.Routes
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Web.Areas.Administration;

    [TestClass]
    public class AdministrationRoutesTests : RoutesTestsBase
    {
        [TestMethod]
        public void AdministrationNavigationShouldReturnProperControllerAndAction()
        {
            var routeData = this.GetAreaRouteData(
                "~/Administration/Navigation",
                new AdministrationAreaAreaRegistration());

            Assert.IsNotNull(routeData);
            Assert.AreEqual("Navigation", routeData.Values["controller"]);
        }

        [TestMethod]
        public void AdministrationNewsShouldReturnProperControllerAndAction()
        {
            var routeData = this.GetAreaRouteData(
                "~/Administration/News",
                new AdministrationAreaAreaRegistration());

            Assert.IsNotNull(routeData);
            Assert.AreEqual("News", routeData.Values["controller"]);
        }

        [TestMethod]
        public void AdministrationContestsShouldReturnProperControllerAndAction()
        {
            var routeData = this.GetAreaRouteData(
                "~/Administration/Contests",
                new AdministrationAreaAreaRegistration());

            Assert.IsNotNull(routeData);
            Assert.AreEqual("Contests", routeData.Values["controller"]);
        }

        [TestMethod]
        public void AdministrationContestsCategoriesReturnProperControllerAndAction()
        {
            var routeData = this.GetAreaRouteData(
                "~/Administration/ContestCategories",
                new AdministrationAreaAreaRegistration());

            Assert.IsNotNull(routeData);
            Assert.AreEqual("ContestCategories", routeData.Values["controller"]);
        }

        [TestMethod]
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
