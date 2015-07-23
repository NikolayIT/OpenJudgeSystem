namespace OJS.Web.Tests.Administration.NewsControllerTests
{
    using Moq;

    using NUnit.Framework;

    using OJS.Data;
    using OJS.Web.Areas.Administration.Controllers;

    [TestFixture]
    public class NewsControllerBaseTestsClass : BaseWebTests
    {
        public NewsControllerBaseTestsClass()
        {
            var data = new Mock<IOjsData>();
            this.NewsController = new NewsController(data.Object);
        }

        protected NewsController NewsController { get; set; }
    }
}
