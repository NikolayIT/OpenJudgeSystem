namespace OJS.Web.Tests.Controllers.Contests.ContestsControllerTests
{
    using System;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Data.Models;
    using OJS.Web.Areas.Contests.Controllers;
    using OJS.Web.Areas.Contests.ViewModels.Contests;

    [TestClass]
    public class DetailsActionTests : BaseWebTests, IDisposable
    {
        private ContestsController contestsController;

        [TestInitialize]
        public void Initialize()
        {
            this.contestsController = new ContestsController(this.EmptyOjsData);
        }

        [TestMethod]
        public void DetailsActionWhenInvalidContestIdIsProvidedShouldThrowException()
        {
            try
            {
                var result = this.contestsController.Details(-1) as ViewResult;
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.NotFound, ex.GetHttpCode());
            }
        }

        [TestMethod]
        public void DetailsActionWhenContestIsNotVisibleShouldThrowException()
        {
            try
            {
                var contest = new Contest
                {
                    Name = "test contest",
                    IsVisible = false
                };

                this.EmptyOjsData.Contests.Add(contest);
                this.EmptyOjsData.SaveChanges();

                var result = this.contestsController.Details(contest.Id) as ViewResult;
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.NotFound, ex.GetHttpCode());
            }
        }

        [TestMethod]
        public void DetailsActionWhenValidContestIdIsProvidedShouldReturnContest()
        {
            var contest = new Contest
            {
                Name = "test contest",
                IsVisible = true,
                IsDeleted = false
            };

            this.EmptyOjsData.Contests.Add(contest);
            this.EmptyOjsData.SaveChanges();

            var result = this.contestsController.Details(contest.Id) as ViewResult;
            var model = (ContestViewModel)result.Model;
            Assert.IsNotNull(model);
            Assert.AreEqual(contest.Id, model.Id);
            Assert.AreEqual(contest.Name, model.Name);
            Assert.AreEqual(contest.Description, model.Description);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.contestsController != null)
                {
                    this.contestsController.Dispose();
                }
            }
        }
    }
}
