namespace OJS.Web.Tests.Contollers.NewsControllerTests
{
    using System.Data.Entity;
    using System.Linq;

    using OJS.Data;
    using OJS.Tests.Common;
    using OJS.Web.Controllers;
    using System.Web.Mvc;

    using OJS.Web.ViewModels.News;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OJS.Data.Models;
    using System.Collections.Generic;

    [TestClass]
    public class LatestNewsActionTests : NewsTestBaseClass
    {
        public LatestNewsActionTests()
            : base ()
        {
        }

        [TestMethod]
        public void LatestNewsShouldReturnProperNewsCount()
        {
            var controller = new NewsController(this.EmptyOjsData);
            var result = controller.LatestNews() as PartialViewResult;
            var model = result.Model as IEnumerable<SelectedNewsViewModel>;

            Assert.AreEqual(5, model.Count());
        }

        [TestMethod]
        public void LatestNewsShouldReturnProperNewsCountWithCustomNewsNumber()
        {
            var controller = new NewsController(this.EmptyOjsData);
            var result = controller.LatestNews(20) as PartialViewResult;
            var model = result.Model as IEnumerable<SelectedNewsViewModel>;

            Assert.AreEqual(20, model.Count());
        }
    }
}
