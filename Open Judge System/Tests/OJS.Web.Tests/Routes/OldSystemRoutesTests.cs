namespace OJS.Web.Tests.Routes
{
    using System.Web.Mvc;

    using NUnit.Framework;

    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Data.Models;
    using OJS.Web.Controllers;

    [TestFixture]
    public class OldSystemRoutesTests : RoutesTestsBase
    {
        [Test]
        public void HardcodedRedirectUrlsShouldNavigateProperly()
        {
            foreach (var oldSystemRedirect in RedirectsController.OldSystemRedirects)
            {
                this.VerifyUrlRedirection(string.Format("~/{0}", oldSystemRedirect.Key), oldSystemRedirect.Value);
            }
        }

        [Test]
        public void ContestCompeteLinkShouldNavigateProperly()
        {
            var contest = new Contest { OldId = 1337, };
            this.EmptyOjsData.Contests.Add(contest);
            this.EmptyOjsData.SaveChanges();

            this.VerifyUrlRedirection("~/Contest/Compete/1337", string.Format("/Contests/Compete/Index/{0}", contest.Id));
        }

        [Test]
        public void ContestPracticeLinkShouldNavigateProperly()
        {
            var contest = new Contest { OldId = 1338, };
            this.EmptyOjsData.Contests.Add(contest);
            this.EmptyOjsData.SaveChanges();

            this.VerifyUrlRedirection("~/Contest/Practice/1338", string.Format("/Contests/Practice/Index/{0}", contest.Id));
        }

        [Test]
        public void ContestResultsLinkShouldNavigateProperly()
        {
            var contest = new Contest { OldId = 1339, };
            this.EmptyOjsData.Contests.Add(contest);
            this.EmptyOjsData.SaveChanges();

            this.VerifyUrlRedirection("~/Contest/ContestResults/1339", string.Format("/Contests/Compete/Results/Simple/{0}", contest.Id));
        }

        [Test]
        public void PracticeResultsLinkShouldNavigateProperly()
        {
            var contest = new Contest { OldId = 1340, };
            this.EmptyOjsData.Contests.Add(contest);
            this.EmptyOjsData.SaveChanges();

            this.VerifyUrlRedirection("~/Contest/ContestResults/1340", string.Format("/Contests/Compete/Results/Simple/{0}", contest.Id));
        }

        [Test]
        public void AccountProfileViewLinkShouldNavigateProperly()
        {
            this.EmptyOjsData.Users.Add(
                new UserProfile { UserName = "Nikolay.IT", Email = "1337@bgcoder.com", OldId = 1337, });
            this.EmptyOjsData.SaveChanges();

            this.VerifyUrlRedirection("~/Account/ProfileView/1337", "/Users/Nikolay.IT");
        }

        [Test]
        public void DownloadTaskLinkShouldNavigateProperly()
        {
            var contest = new Contest { Name = "1337", };
            var problem = new Problem { Contest = contest, Name = "1337", OldId = 1337 };
            var resource = new ProblemResource { Problem = problem, Type = ProblemResourceType.ProblemDescription };
            this.EmptyOjsData.Resources.Add(resource);
            this.EmptyOjsData.SaveChanges();

            this.VerifyUrlRedirection("~/Contest/DownloadTask/1337", string.Format("/Contests/Practice/DownloadResource/{0}", resource.Id));
        }

        private void VerifyUrlRedirection(string oldUrl, string newUrl)
        {
            var routeData = this.GetRouteData(oldUrl);

            Assert.IsNotNull(routeData);
            Assert.AreEqual("Redirects", routeData.Values["controller"], "Invalid controller when redirecting from: {0}", oldUrl);
            Assert.IsNotNull(routeData.Values["id"], "Invalid action id when redirecting from: {0}", oldUrl);

            var controller = new RedirectsController(this.EmptyOjsData);
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
