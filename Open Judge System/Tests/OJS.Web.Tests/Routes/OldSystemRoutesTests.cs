namespace OJS.Web.Tests.Routes
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Common.Extensions;
    using OJS.Web.Controllers;

    [TestClass]
    public class OldSystemRoutesTests : RoutesTestsBase
    {
        [TestMethod]
        public void HardcodedRedirectUrlsShouldNavigateProperly()
        {
            foreach (var oldSystemRedirect in RedirectsController.OldSystemRedirects)
            {
                VerifyUrlRedirection(string.Format("~/{0}", oldSystemRedirect.Key), oldSystemRedirect.Value);
            }
        }

        public void VerifyUrlRedirection(string oldUrl, string newUrl)
        {
            var routeData = this.GetRouteData(oldUrl);

            Assert.IsNotNull(routeData);
            Assert.AreEqual("Redirects", routeData.Values["controller"], "Invalid controller when redirecting from: {0}", oldUrl);
            Assert.AreEqual("Index", routeData.Values["action"], "Invalid action when redirecting from: {0}", oldUrl);
            Assert.IsNotNull(routeData.Values["id"], "Invalid action id when redirecting from: {0}", oldUrl);

            var controller = new RedirectsController(EmptyOjsData);
            var result = controller.Index(routeData.Values["id"].ToString().ToInteger());
            Assert.AreEqual(newUrl, result.Url, "Invalid redirect url when redirecting from: {0}", oldUrl);
        }
    }
}
