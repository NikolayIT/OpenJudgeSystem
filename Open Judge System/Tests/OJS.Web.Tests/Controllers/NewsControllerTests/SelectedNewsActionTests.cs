namespace OJS.Web.Tests.Contollers.NewsControllerTests
{
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Tests.Common;
    using OJS.Web.Controllers;
    using OJS.Web.ViewModels.News;

    [TestClass]
    public class SelectedNewsActionTests : NewsTestBaseClass
    {
        public SelectedNewsActionTests()
            : base()
        {
        }

        [TestMethod]
        public void SelectedActionShouldReturnViewModel()
        {
            var controller = new NewsController(this.EmptyOjsData);
            var id = this.EmptyOjsData.News.All().FirstOrDefault(x => x.IsVisible && !x.IsDeleted).Id;
            var result = controller.Selected(id) as ViewResult;
            var model = result.Model as SelectedNewsViewModel;

            Assert.AreEqual("News Title ", model.Title.Substring(0, 11));
            Assert.AreEqual("News Content ", model.Content.Substring(0, 13));
        }

        [TestMethod]
        public void SelectedActionShouldReturnViewWithMessageWhenSelectedNewsIsNull()
        {
            try
            {
                var controller = new NewsController(this.EmptyOjsData);
                var result = controller.Selected(10000) as ViewResult;
                Assert.Fail();
            }
            catch (HttpException ex)
            {
                Assert.AreEqual(ex.GetHttpCode(), (int)HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public void SelectedNewsShouldReturnProperPreviousNews()
        {
            var controller = new NewsController(this.EmptyOjsData);
            var id = this.EmptyOjsData.News.All().FirstOrDefault(x => x.IsVisible && !x.IsDeleted).Id;
            var result = controller.Selected(id) as ViewResult;
            var model = result.Model as SelectedNewsViewModel;

            var lastId = this.EmptyOjsData.News.All().OrderByDescending(x => x.Id).FirstOrDefault(x => x.IsVisible && !x.IsDeleted).Id;

            bool finalResult = model.PreviousId < model.Id || model.PreviousId == lastId;

            Assert.IsTrue(finalResult);
        }

        [TestMethod]
        public void SelectedNewsShouldReturnProperNextNews()
        {
            var controller = new NewsController(this.EmptyOjsData);
            var id = this.EmptyOjsData.News.All().FirstOrDefault(x => x.IsVisible && !x.IsDeleted).Id;
            var result = controller.Selected(id) as ViewResult;
            var model = result.Model as SelectedNewsViewModel;

            var firstId = this.EmptyOjsData.News.All().OrderBy(x => x.Id).FirstOrDefault(x => x.IsVisible && !x.IsDeleted).Id;

            bool finalResult = model.NextId > model.Id || model.NextId == firstId;

            Assert.IsTrue(finalResult);
        }

        [TestMethod]
        public void SelectedNewsShouldReturnCorrectNews()
        {
            var controller = new NewsController(this.EmptyOjsData);
            var selectedNews = this.EmptyOjsData.News.All().FirstOrDefault(x => x.IsVisible && !x.IsDeleted);
            var result = controller.Selected(selectedNews.Id) as ViewResult;
            var model = result.Model as SelectedNewsViewModel;

            Assert.AreEqual(selectedNews.Title, model.Title);
            Assert.AreEqual(selectedNews.Content, model.Content);
            Assert.AreEqual(selectedNews.CreatedOn, model.TimeCreated);
        }
    }
}
