namespace OJS.Web.Tests.Contollers.NewsControllerTests
{
    using System.Linq;
    using System.Web.Mvc;

    using NUnit.Framework;

    using OJS.Web.Controllers;
    using OJS.Web.ViewModels.News;

    [TestFixture]
    public class AllNewsActionTests : NewsTestBaseClass
    {
        [Test]
        public void AllActionShouldReturnViewModel()
        {
            var controller = new NewsController(this.EmptyOjsData);
            var result = controller.All() as ViewResult;
            var model = result.Model as AllNewsViewModel;

            Assert.AreEqual(10, model.AllNews.Count());
            Assert.AreEqual(1, model.CurrentPage);
            Assert.AreEqual(10, model.PageSize);
            Assert.AreEqual(4, model.AllPages);
        }

        [Test]
        public void AllActionShouldReturnCorrectNewsCountWithDefaultValues()
        {
            var controller = new NewsController(this.EmptyOjsData);
            var result = controller.All() as ViewResult;
            var model = result.Model as AllNewsViewModel;

            Assert.AreEqual(10, model.AllNews.Count());
        }

        [Test]
        public void AllActionShouldReturnCorrectNewsCountWithSetInitialPage()
        {
            var controller = new NewsController(this.EmptyOjsData);
            var result = controller.All(2) as ViewResult;
            var model = result.Model as AllNewsViewModel;

            Assert.AreEqual(10, model.AllNews.Count());
        }

        [Test]
        public void AllActionShouldReturnCorrectNewsCountWithSetPageSize()
        {
            var controller = new NewsController(this.EmptyOjsData);
            var result = controller.All(2, 15) as ViewResult;
            var model = result.Model as AllNewsViewModel;

            Assert.AreEqual(15, model.AllNews.Count());
        }

        [Test]
        public void AllActionShouldReturnCorrectNewsCountInLastPage()
        {
            var controller = new NewsController(this.EmptyOjsData);
            var result = controller.All(5) as ViewResult;
            var model = result.Model as AllNewsViewModel;

            Assert.AreEqual(9, model.AllNews.Count());
        }

        [Test]
        public void AllActionShouldReturnCorrectNewsCountInLastPageWithCustomPageSize()
        {
            var controller = new NewsController(this.EmptyOjsData);
            var result = controller.All(3, 20) as ViewResult;
            var model = result.Model as AllNewsViewModel;

            Assert.AreEqual(19, model.AllNews.Count());
        }

        [Test]
        public void AllActionShouldReturnCorrectNewsCountAndFirstPageIfInvalidPage()
        {
            var controller = new NewsController(this.EmptyOjsData);
            var result = controller.All(-2) as ViewResult;
            var model = result.Model as AllNewsViewModel;

            Assert.AreEqual(10, model.AllNews.Count());
            Assert.AreEqual(1, model.CurrentPage);
        }

        [Test]
        public void AllActionShouldReturnCorrectPageSizeIfInvalidNumber()
        {
            var controller = new NewsController(this.EmptyOjsData);
            var result = controller.All(1, -50) as ViewResult;
            var model = result.Model as AllNewsViewModel;

            Assert.AreEqual(10, model.PageSize);
        }

        [Test]
        public void AllActionShouldReturnLastPageIfPassesParameterIsTooBig()
        {
            var controller = new NewsController(this.EmptyOjsData);
            var result = controller.All(100) as ViewResult;
            var model = result.Model as AllNewsViewModel;

            Assert.AreEqual(4, model.CurrentPage);
        }

        [Test]
        public void AllActionShouldReturnCorrectNewsTitles()
        {
            var controller = new NewsController(this.EmptyOjsData);
            var result = controller.All() as ViewResult;
            var model = result.Model as AllNewsViewModel;

            foreach (var news in model.AllNews)
            {
                Assert.AreEqual("News Title ", news.Title.Substring(0, 11));
            }
        }
    }
}
