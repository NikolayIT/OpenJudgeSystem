namespace OJS.Web.Tests.Routes
{
    using System.Web.Mvc;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Common.Extensions;
    using OJS.Data.Models;
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

        [TestMethod]
        public void AccountProfileViewLinkShouldNavigateProperly()
        {
            this.EmptyOjsData.Users.Add(new UserProfile
                                            {
                                                UserName = "Nikolay.IT",
                                                Email = "1337@bgcoder.com",
                                                OldId = 1337,
                                            });
            this.EmptyOjsData.SaveChanges();

            VerifyUrlRedirection("~/Account/ProfileView/1337", "/Users/Nikolay.IT");
        }

        private void VerifyUrlRedirection(string oldUrl, string newUrl)
        {
            var routeData = this.GetRouteData(oldUrl);

            Assert.IsNotNull(routeData);
            Assert.AreEqual("Redirects", routeData.Values["controller"], "Invalid controller when redirecting from: {0}", oldUrl);
            Assert.IsNotNull(routeData.Values["id"], "Invalid action id when redirecting from: {0}", oldUrl);

            var controller = new RedirectsController(EmptyOjsData);
            var id = routeData.Values["id"].ToString().ToInteger();
            var actionName = routeData.Values["action"].ToString();
            var type = controller.GetType();
            var methodInfo = type.GetMethod(actionName);
            var result = methodInfo.Invoke(controller, new object[] { id }) as RedirectResult;
            Assert.IsNotNull(result, "Action invokation returned null when redirecting from: {0}", oldUrl);
            Assert.AreEqual(newUrl, result.Url, "Invalid redirect url when redirecting from: {0}", oldUrl);
        }
    }
}
