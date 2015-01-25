namespace OJS.Web.Tests.Routes
{
    using NUnit.Framework;

    using OJS.Common;
    using OJS.Web.Areas.Contests;

    using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

    [TestFixture]
    public class ContestsRoutesTests : RoutesTestsBase
    {
        [Test]
        public void ContestsListUrlShouldNavigateProperly()
        {
            var routeData = this.GetAreaRouteData("~/Contests", new ContestsAreaAreaRegistration());

            Assert.IsNotNull(routeData);
            Assert.AreEqual("List", routeData.Values["controller"]);
            Assert.AreEqual(GlobalConstants.Index, routeData.Values["action"]);
        }

        [Test]
        public void ContestsDetailsUrlShouldNavigateProperly()
        {
            var routeData = this.GetAreaRouteData("~/Contests/123/some-contest", new ContestsAreaAreaRegistration());

            Assert.IsNotNull(routeData);
            Assert.AreEqual("Contests", routeData.Values["controller"]);
            Assert.AreEqual("Details", routeData.Values["action"]);
            Assert.AreEqual("123", routeData.Values["id"]);
            Assert.AreEqual("some-contest", routeData.Values["name"]);
        }

        [Test]
        public void GetContestsByCategoryShouldNavigateProperly()
        {
            var routeData = this.GetAreaRouteData("~/Contests/List/ByCategory/14", new ContestsAreaAreaRegistration());

            Assert.IsNotNull(routeData);
            Assert.AreEqual("List", routeData.Values["controller"]);
            Assert.AreEqual("ByCategory", routeData.Values["action"]);
            Assert.AreEqual("14", routeData.Values["id"]);
        }

        [Test]
        public void ReadContestsCategoriesShouldNavigateProperly()
        {
            var routeData = this.GetAreaRouteData("~/Contests/List/ReadCategories", new ContestsAreaAreaRegistration());

            Assert.IsNotNull(routeData);
            Assert.AreEqual("List", routeData.Values["controller"]);
            Assert.AreEqual("ReadCategories", routeData.Values["action"]);
        }

        [Test]
        public void ContestsCompeteLinkShouldNavigateProperly()
        {
            var routeData = this.GetAreaRouteData("~/Contests/Compete/Index/11", new ContestsAreaAreaRegistration());

            Assert.IsNotNull(routeData);
            Assert.AreEqual("Compete", routeData.Values["controller"]);
            Assert.AreEqual(GlobalConstants.Index, routeData.Values["action"]);
            Assert.AreEqual("11", routeData.Values["id"]);
            Assert.IsTrue((bool)routeData.Values["official"]);
        }

        [Test]
        public void ContestsPracticeLinkShouldNavigateProperly()
        {
            var routeData = this.GetAreaRouteData("~/Contests/Practice/Index/12", new ContestsAreaAreaRegistration());

            Assert.IsNotNull(routeData);
            Assert.AreEqual("Compete", routeData.Values["controller"]);
            Assert.AreEqual(GlobalConstants.Index, routeData.Values["action"]);
            Assert.AreEqual("12", routeData.Values["id"]);
            Assert.IsFalse((bool)routeData.Values["official"]);
        }

        [Test]
        public void ContestsCompeteRegistrationLinkShouldNavigateProperly()
        {
            var routeData = this.GetAreaRouteData("~/Contests/Compete/Register/11", new ContestsAreaAreaRegistration());

            Assert.IsNotNull(routeData);
            Assert.AreEqual("Compete", routeData.Values["controller"]);
            Assert.AreEqual("Register", routeData.Values["action"]);
            Assert.AreEqual("11", routeData.Values["id"]);
            Assert.IsTrue((bool)routeData.Values["official"]);
        }

        [Test]
        public void ContestsPracticeRegistrationLinkShouldNavigateProperly()
        {
            var routeData = this.GetAreaRouteData("~/Contests/Practice/Register/12", new ContestsAreaAreaRegistration());

            Assert.IsNotNull(routeData);
            Assert.AreEqual("Compete", routeData.Values["controller"]);
            Assert.AreEqual("Register", routeData.Values["action"]);
            Assert.AreEqual("12", routeData.Values["id"]);
            Assert.IsFalse((bool)routeData.Values["official"]);
        }

        [Test]
        public void ContestsCompeteResultsLinkShouldNavigateProperly()
        {
            var routeData = this.GetAreaRouteData("~/Contests/Compete/Results/Simple/11", new ContestsAreaAreaRegistration());

            Assert.IsNotNull(routeData);
            Assert.AreEqual("Results", routeData.Values["controller"]);
            Assert.AreEqual("Simple", routeData.Values["action"]);
            Assert.AreEqual("11", routeData.Values["id"]);
            Assert.IsTrue((bool)routeData.Values["official"]);
        }

        [Test]
        public void ContestsPracticeResultsLinkShouldNavigateProperly()
        {
            var routeData = this.GetAreaRouteData("~/Contests/Practice/Results/Simple/12", new ContestsAreaAreaRegistration());

            Assert.IsNotNull(routeData);
            Assert.AreEqual("Results", routeData.Values["controller"]);
            Assert.AreEqual("Simple", routeData.Values["action"]);
            Assert.AreEqual("12", routeData.Values["id"]);
            Assert.IsFalse((bool)routeData.Values["official"]);
        }

        [Test]
        public void ContestsCompeteFullResultsLinkShouldNavigateProperly()
        {
            var routeData = this.GetAreaRouteData("~/Contests/Compete/Results/Full/11", new ContestsAreaAreaRegistration());

            Assert.IsNotNull(routeData);
            Assert.AreEqual("Results", routeData.Values["controller"]);
            Assert.AreEqual("Full", routeData.Values["action"]);
            Assert.AreEqual("11", routeData.Values["id"]);
            Assert.IsTrue((bool)routeData.Values["official"]);
        }

        [Test]
        public void ContestsPracticeFullResultsLinkShouldNavigateProperly()
        {
            var routeData = this.GetAreaRouteData("~/Contests/Practice/Results/Full/12", new ContestsAreaAreaRegistration());

            Assert.IsNotNull(routeData);
            Assert.AreEqual("Results", routeData.Values["controller"]);
            Assert.AreEqual("Full", routeData.Values["action"]);
            Assert.AreEqual("12", routeData.Values["id"]);
            Assert.IsFalse((bool)routeData.Values["official"]);
        }

        [Test]
        public void ContestsCompeteGetResultsByProblemIdLinkShouldNavigateProperly()
        {
            var routeData = this.GetAreaRouteData("~/Contests/Compete/Results/ByProblem/11", new ContestsAreaAreaRegistration());

            Assert.IsNotNull(routeData);
            Assert.AreEqual("Results", routeData.Values["controller"]);
            Assert.AreEqual("ByProblem", routeData.Values["action"]);
            Assert.AreEqual("11", routeData.Values["id"]);
            Assert.IsTrue((bool)routeData.Values["official"]);
        }

        [Test]
        public void ContestsPracticeGetResultsByProblemIdLinkShouldNavigateProperly()
        {
            var routeData = this.GetAreaRouteData("~/Contests/Practice/Results/ByProblem/12", new ContestsAreaAreaRegistration());

            Assert.IsNotNull(routeData);
            Assert.AreEqual("Results", routeData.Values["controller"]);
            Assert.AreEqual("ByProblem", routeData.Values["action"]);
            Assert.AreEqual("12", routeData.Values["id"]);
            Assert.IsFalse((bool)routeData.Values["official"]);
        }
    }
}
