namespace OJS.Web.Tests.Controllers
{
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using Microsoft.VisualStudio.TestTools.UnitTesting;    
    
    using OJS.Data;
    using OJS.Tests.Common;
    using OJS.Web.Controllers;
    using OJS.Web.ViewModels.Home.Index;

    [TestClass]
    public class HomeControllerTests : TestClassBase
    {
        [TestMethod]
        public void IndexActionShouldReturnViewModel()
        {
            var controller = new HomeController(EmptyOjsData);
            var result = controller.Index() as ViewResult;
            var model = result.Model as IndexViewModel;
            Assert.AreEqual(0, model.ActiveContests.Count());
            Assert.AreEqual(0, model.PastContests.Count());
            Assert.AreEqual(0, model.FutureContests.Count());
        }

        [TestMethod]
        public void IndexActionShouldReturnProperActiveContestsCount()
        {
            var controller = new HomeController(OjsData);
            var result = controller.Index() as ViewResult;
            var model = result.Model as IndexViewModel;
            Assert.AreEqual(2, model.ActiveContests.Count());
        }

        [TestMethod]
        public void IndexActionShouldReturnProperPastContestsCount()
        {
            var controller = new HomeController(OjsData);
            var result = controller.Index() as ViewResult;
            var model = result.Model as IndexViewModel;
            Assert.AreEqual(3, model.PastContests.Count());
        }

        [TestMethod]
        public void IndexActionShouldReturnProperFutureContestsCount()
        {
            var controller = new HomeController(OjsData);
            var result = controller.Index() as ViewResult;
            var model = result.Model as IndexViewModel;
            Assert.AreEqual(4, model.FutureContests.Count());
        }

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
        public void IndexActionShouldReturnProperVisibleActiveContests()
        {
            var controller = new HomeController(OjsData);
            var result = controller.Index() as ViewResult;
            var model = result.Model as IndexViewModel;

            Assert.AreEqual(2, model.ActiveContests.Count());
        }

        [TestMethod]
        public void IndexActionShouldReturnProperVisiblePastContests()
        {
            var controller = new HomeController(OjsData);
            var result = controller.Index() as ViewResult;
            var model = result.Model as IndexViewModel;

            Assert.AreEqual(3, model.PastContests.Count());
        }

        [TestMethod]
        public void IndexActionShouldReturnProperVisibleFutureContests()
        {
            var controller = new HomeController(OjsData);
            var result = controller.Index() as ViewResult;
            var model = result.Model as IndexViewModel;

            Assert.AreEqual(4, model.FutureContests.Count());
        }
    }
}
