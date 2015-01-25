namespace OJS.Web.Tests.Administration.NewsControllerTests
{
    using Moq;

    using NUnit.Framework;

    using OJS.Data;
    using OJS.Web.Areas.Administration.Controllers;

    [TestFixture]
    public class NewsControllerBaseTestsClass : BaseWebTests
    {
        private readonly Mock<IOjsData> data;

        public NewsControllerBaseTestsClass()
        {
            this.data = new Mock<IOjsData>();
            this.NewsController = new NewsController(this.data.Object);
        }

        protected NewsController NewsController { get; set; }
    }
}
