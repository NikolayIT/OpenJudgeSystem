namespace OJS.Web.Tests.Controllers
{
    using System.Linq;
    using System.Web.Mvc;

    using NUnit.Framework;

    using OJS.Tests.Common;
    using OJS.Web.Controllers;
    using OJS.Web.ViewModels.Home.Index;

    [TestFixture]
    public class HomeControllerTests : TestClassBase
    {
        [Test]
        public void IndexActionShouldReturnViewModel()
        {
            var controller = new HomeController(EmptyOjsData);
            var result = controller.Index() as ViewResult;
            var model = result.Model as IndexViewModel;
            Assert.AreEqual(0, model.ActiveContests.Count());
            Assert.AreEqual(0, model.PastContests.Count());
            Assert.AreEqual(0, model.FutureContests.Count());
        }

        [Test]
        public void IndexActionShouldReturnProperActiveContestsCount()
        {
            var controller = new HomeController(OjsData);
            var result = controller.Index() as ViewResult;
            var model = result.Model as IndexViewModel;
            Assert.AreEqual(2, model.ActiveContests.Count());
        }

        [Test]
        public void IndexActionShouldReturnProperPastContestsCount()
        {
            var controller = new HomeController(OjsData);
            var result = controller.Index() as ViewResult;
            var model = result.Model as IndexViewModel;
            Assert.AreEqual(3, model.PastContests.Count());
        }

        [Test]
        public void IndexActionShouldReturnProperFutureContestsCount()
        {
            var controller = new HomeController(OjsData);
            var result = controller.Index() as ViewResult;
            var model = result.Model as IndexViewModel;
            Assert.AreEqual(4, model.FutureContests.Count());
        }

        [Test]
        public void IndexActionShouldReturnProperActiveContestNames()
        {
            var controller = new HomeController(OjsData);
            var result = controller.Index() as ViewResult;
            var model = result.Model as IndexViewModel;

            foreach (var contest in model.ActiveContests)
            {
                Assert.AreEqual("Active-Visible", contest.Name);
            }
        }

        [Test]
        public void IndexActionShouldReturnProperPastContestNames()
        {
            var controller = new HomeController(OjsData);
            var result = controller.Index() as ViewResult;
            var model = result.Model as IndexViewModel;

            foreach (var contest in model.PastContests)
            {
                Assert.AreEqual("Past-Visible", contest.Name);
            }
        }

        [Test]
        public void IndexActionShouldReturnProperFutureContestNames()
        {
            var controller = new HomeController(OjsData);
            var result = controller.Index() as ViewResult;
            var model = result.Model as IndexViewModel;

            foreach (var contest in model.FutureContests)
            {
                Assert.AreEqual("Future-Visible", contest.Name);
            }
        }

        [Test]
        public void IndexActionShouldReturnProperVisibleActiveContests()
        {
            var controller = new HomeController(OjsData);
            var result = controller.Index() as ViewResult;
            var model = result.Model as IndexViewModel;

            Assert.AreEqual(2, model.ActiveContests.Count());
        }

        [Test]
        public void IndexActionShouldReturnProperVisiblePastContests()
        {
            var controller = new HomeController(OjsData);
            var result = controller.Index() as ViewResult;
            var model = result.Model as IndexViewModel;

            Assert.AreEqual(3, model.PastContests.Count());
        }

        [Test]
        public void IndexActionShouldReturnProperVisibleFutureContests()
        {
            var controller = new HomeController(OjsData);
            var result = controller.Index() as ViewResult;
            var model = result.Model as IndexViewModel;

            Assert.AreEqual(4, model.FutureContests.Count());
        }
    }
}
