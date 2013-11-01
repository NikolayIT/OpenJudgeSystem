namespace OJS.Web.Tests.Controllers.Contests.CompeteControllerTests
{
    using System;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Data.Models;

    [TestClass]
    public class IndexActionTests : CompeteControllerBaseTestsClass
    {
        [TestMethod]
        public void IndexActionForInvalidContestShouldThrowException()
        {
            try
            {
                var result = this.CompeteController.Index(-1, this.IsCompete);
                Assert.Fail("Expected an exception when an invalid contest id is provided!");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.NotFound, ex.GetHttpCode());
            }
        }

        [TestMethod]
        public void IndexActionForInvalidPracticeShouldThrowException()
        {
            try
            {
                var result = this.CompeteController.Index(-1, this.IsPractice);
                Assert.Fail("Expected an exception when an invalid contest id is provided!");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.NotFound, ex.GetHttpCode());
            }
        }

        [TestMethod]
        public void IndexActionWhenContestCannotBeCompetedShouldThrowException()
        {
            var contest = this.CreateAndSaveContest("testContest", this.InactiveContestOptions, this.InactiveContestOptions);

            try
            {
                var result = this.CompeteController.Index(contest.Id, this.IsCompete);
                Assert.Fail("Expected an exception when trying to compete a contest when contest cannot be competed!");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.Forbidden, ex.GetHttpCode());
            }
        }

        [TestMethod]
        public void IndexActionWhenContestCannotBePracticedShouldThrowException()
        {
            var contest = this.CreateAndSaveContest("testContest", this.InactiveContestOptions, this.InactiveContestOptions);

            try
            {
                var result = this.CompeteController.Index(contest.Id, this.IsPractice);
                Assert.Fail("Expected an exception when trying to practice a contest when contest cannot be practiced!");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.Forbidden, ex.GetHttpCode());
            }
        }

        [TestMethod]
        public void IndexActionWhenUserIsNotRegisteredToPracticeAndPracticeHasNoPasswordShouldReturnView()
        {
            var contest = this.CreateAndSaveContest("testContest", this.InactiveContestOptions, this.ActiveContestNoPasswordOptions);

            var result = this.CompeteController.Index(contest.Id, this.IsPractice) as ViewResult;

            Assert.AreEqual("Practice", result.ViewBag.CompeteType);
            Assert.IsNotNull(result.Model);
        }

        [TestMethod]
        public void IndexActionWhenUserIsAlreadyRegisteredToCompeteShouldReturnView()
        {
            var contest = this.CreateAndSaveContest("testContest", this.ActiveContestWithPasswordAndQuestionsOptions, this.ActiveContestWithPasswordAndQuestionsOptions);

            this.EmptyOjsData.Participants.Add(new Participant(contest.Id, this.FakeUserProfile.Id, this.IsCompete));
            this.EmptyOjsData.SaveChanges();

            var result = this.CompeteController.Index(contest.Id, this.IsCompete) as ViewResult;

            Assert.AreEqual("Compete", result.ViewBag.CompeteType);
            Assert.IsNotNull(result.Model);
        }

        [TestMethod]
        public void IndexActionWhenUserIsAlreadyRegisteredToPracticeShouldReturnView()
        {
            var contest = this.CreateAndSaveContest("testContest", this.InactiveContestOptions, this.ActiveContestNoPasswordOptions);

            this.EmptyOjsData.Participants.Add(new Participant(contest.Id, this.FakeUserProfile.Id, this.IsPractice));
            this.EmptyOjsData.SaveChanges();

            var result = this.CompeteController.Index(contest.Id, this.IsPractice) as ViewResult;

            Assert.AreEqual("Practice", result.ViewBag.CompeteType);
            Assert.IsNotNull(result.Model);
        }

        [TestMethod]
        public void IndexActionWhenUserIsRegisteredToPracticeButTriesToCompeteShouldRedirectToRegistration()
        {
            var contest = this.CreateAndSaveContest("testContest", this.ActiveContestWithPasswordAndQuestionsOptions, this.ActiveContestWithPasswordAndQuestionsOptions);

            this.EmptyOjsData.Participants.Add(new Participant(contest.Id, this.FakeUserProfile.Id, this.IsPractice));
            this.EmptyOjsData.SaveChanges();

            var result = this.CompeteController.Index(contest.Id, this.IsCompete) as RedirectToRouteResult;

            Assert.AreEqual("Register", result.RouteValues["action"]);
            Assert.IsNull(result.RouteValues["controller"]);
            Assert.AreEqual(contest.Id, result.RouteValues["id"]);
            Assert.AreEqual(this.IsCompete, result.RouteValues["official"]);
        }

        [TestMethod]
        public void IndexActionWhenContestHasQuestionsShouldRedirectToRegistration()
        {
            var contest = this.CreateAndSaveContest("testContest", this.ActiveContestWithQuestionsOptions, this.InactiveContestOptions);

            this.EmptyOjsData.Participants.Add(new Participant(contest.Id, this.FakeUserProfile.Id, this.IsPractice));
            this.EmptyOjsData.SaveChanges();

            var result = this.CompeteController.Index(contest.Id, this.IsCompete) as RedirectToRouteResult;

            Assert.AreEqual("Register", result.RouteValues["action"]);
            Assert.IsNull(result.RouteValues["controller"]);
            Assert.AreEqual(contest.Id, result.RouteValues["id"]);
            Assert.AreEqual(this.IsCompete, result.RouteValues["official"]);
        }

        [TestMethod]
        public void IndexActionWhenPracticeHasQuestionsShouldRedirectToRegistration()
        {
            var contest = this.CreateAndSaveContest("testContest", this.InactiveContestOptions, this.ActiveContestWithQuestionsOptions);

            var result = this.CompeteController.Index(contest.Id, this.IsPractice) as RedirectToRouteResult;

            Assert.AreEqual("Register", result.RouteValues["action"]);
            Assert.IsNull(result.RouteValues["controller"]);
            Assert.AreEqual(contest.Id, result.RouteValues["id"]);
            Assert.AreEqual(this.IsPractice, result.RouteValues["official"]);
        }

        [TestMethod]
        public void IndexActionWhenPracticeHasPasswordShouldRedirectToRegistration()
        {
            var contest = this.CreateAndSaveContest("testContest", this.InactiveContestOptions, this.ActiveContestWithPasswordOptions);

            var result = this.CompeteController.Index(contest.Id, this.IsPractice) as RedirectToRouteResult;

            Assert.AreEqual("Register", result.RouteValues["action"]);
            Assert.IsNull(result.RouteValues["controller"]);
            Assert.AreEqual(contest.Id, result.RouteValues["id"]);
            Assert.AreEqual(this.IsPractice, result.RouteValues["official"]);
        }

        [TestMethod]
        public void IndexActionWhenContestHasPasswordShouldRedirectToRegistration()
        {
            var contest = this.CreateAndSaveContest("testContest", this.ActiveContestWithPasswordAndQuestionsOptions, this.InactiveContestOptions);

            var result = this.CompeteController.Index(contest.Id, this.IsCompete) as RedirectToRouteResult;

            Assert.AreEqual("Register", result.RouteValues["action"]);
            Assert.IsNull(result.RouteValues["controller"]);
            Assert.AreEqual(contest.Id, result.RouteValues["id"]);
            Assert.AreEqual(this.IsCompete, result.RouteValues["official"]);
        }
    }
}
