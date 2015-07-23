namespace OJS.Web.Tests.Contollers.NewsControllerTests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using NUnit.Framework;

    using OJS.Web.Controllers;
    using OJS.Web.ViewModels.News;

    [TestFixture]
    public class LatestNewsActionTests : NewsTestBaseClass
    {
        [Test]
        public void LatestNewsShouldReturnProperNewsCount()
        {
            var controller = new NewsController(this.EmptyOjsData);
            var result = controller.LatestNews() as PartialViewResult;
            var model = result.Model as IEnumerable<SelectedNewsViewModel>;

            Assert.AreEqual(5, model.Count());
        }

        [Test]
        public void LatestNewsShouldReturnProperNewsCountWithCustomNewsNumber()
        {
            var controller = new NewsController(this.EmptyOjsData);
            var result = controller.LatestNews(20) as PartialViewResult;
            var model = result.Model as IEnumerable<SelectedNewsViewModel>;

            Assert.AreEqual(20, model.Count());
        }
    }
}
