namespace OJS.Web.Tests.Controllers.Contests.CompeteControllerTests
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Data.Models;
    using OJS.Web.Areas.Contests.Models;
    using OJS.Web.Areas.Contests.ViewModels;

    [TestClass]
    public class RegisterActionTests : CompeteControllerBaseTestsClass
    {
        [TestMethod]
        public void RegisterActionForInvalidContestShouldThrowException()
        {
            try
            {
                var result = this.CompeteController.Register(-1, this.IsCompete);
                Assert.Fail("Expected an exception to be thrown when an invalid contest id is provided!");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.NotFound, ex.GetHttpCode());
            }
        }

        [TestMethod]
        public void RegisterActionForInvalidPracticeShouldThrowException()
        {
            try
            {
                var result = this.CompeteController.Register(-1, this.IsPractice);
                Assert.Fail("Expected an exception to be thrown when an invalid contest id is provided!");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.NotFound, ex.GetHttpCode());
            }
        }

        [TestMethod]
        public void RegisterActionWhenContestCannotBeCompetedShouldThrowException()
        {
            var contest = this.CreateAndSaveContest("testContest", this.InactiveContestOptions, this.InactiveContestOptions);

            try
            {
                var result = this.CompeteController.Register(contest.Id, this.IsCompete);
                Assert.Fail("Expected an exception to be thrown when a contest cannot be competed, but student tries to compete!");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.Forbidden, ex.GetHttpCode());
            }
        }

        [TestMethod]
        public void RegisterActionWhenContestCannotBePracticedShouldThrowException()
        {
            var contest = this.CreateAndSaveContest("testContest", this.InactiveContestOptions, this.InactiveContestOptions);

            try
            {
                var result = this.CompeteController.Register(contest.Id, this.IsPractice);
                Assert.Fail("Expected an exception to be thrown when a contest cannot be practiced, but student tries to practice it!");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.Forbidden, ex.GetHttpCode());
            }
        }

        [TestMethod]
        public void RegisterActionWhenContestCanBePracticedShouldReturnView()
        {
            var contest = this.CreateAndSaveContest("testContest", this.InactiveContestOptions, this.ActiveContestWithPasswordOptions);

            var result = this.CompeteController.Register(contest.Id, this.IsPractice) as ViewResult;
            var model = result.Model as ContestRegistrationViewModel;
            var contestRequiresPracticePassword = !string.IsNullOrEmpty(contest.PracticePassword);

            Assert.IsNotNull(model);
            Assert.AreEqual(contest.Name, model.ContestName);
            Assert.AreEqual(contestRequiresPracticePassword, model.RequirePassword);
            Assert.AreEqual(contest.Questions.Count, model.Questions.Count());
        }

        [TestMethod]
        public void RegisterActionWhenContestCanBeCompetedShouldReturnView()
        {
            var contest = this.CreateAndSaveContest("testContest", this.ActiveContestWithPasswordOptions, this.InactiveContestOptions);

            var result = this.CompeteController.Register(contest.Id, this.IsCompete) as ViewResult;
            var model = result.Model as ContestRegistrationViewModel;
            var contestRequiresPracticePassword = !string.IsNullOrEmpty(contest.ContestPassword);

            Assert.IsNotNull(model);
            Assert.AreEqual(contest.Name, model.ContestName);
            Assert.AreEqual(contestRequiresPracticePassword, model.RequirePassword);
            Assert.AreEqual(contest.Questions.Count, model.Questions.Count());
        }

        [TestMethod]
        public void RegisterActionWhenUserAlreadyRegisteredToCompeteShouldRedirectToIndex()
        {
            var contest = this.CreateAndSaveContest("testContest", this.ActiveContestWithPasswordOptions, this.ActiveContestWithPasswordOptions);

            this.EmptyOjsData.Participants.Add(new Participant(contest.Id, this.FakeUserProfile.Id, this.IsCompete));
            this.EmptyOjsData.SaveChanges();

            var result = this.CompeteController.Register(contest.Id, this.IsCompete) as RedirectToRouteResult;

            Assert.IsNull(result.RouteValues["controller"]);
            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [TestMethod]
        public void RegisterActionWhenUserAlreadyRegisteredToPracticeAndTryingToRegisterForCompeteWhenCompeteNotAvailableShouldThrowException()
        {
            var contest = this.CreateAndSaveContest("testContest", this.InactiveContestOptions, this.ActiveContestWithPasswordOptions);

            this.EmptyOjsData.Participants.Add(new Participant(contest.Id, this.FakeUserProfile.Id, this.IsPractice));
            this.EmptyOjsData.SaveChanges();

            try
            {
                var result = this.CompeteController.Register(contest.Id, this.IsCompete);
                Assert.Fail("Expected an exception to be thrown when a user is registered to practice," +
                            "but tries to compete when compete is not allowed!");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.Forbidden, ex.GetHttpCode());
            }
        }

        [TestMethod]
        public void RegisterActionWhenUserAlreadyRegisteredToPracticeAndTryingToRegisterForCompeteShouldReturnView()
        {
            var contest = this.CreateAndSaveContest("testContest", this.ActiveContestWithPasswordOptions, this.ActiveContestWithPasswordOptions);

            this.EmptyOjsData.Participants.Add(new Participant(contest.Id, this.FakeUserProfile.Id, this.IsPractice));
            this.EmptyOjsData.SaveChanges();

            var result = this.CompeteController.Register(contest.Id, this.IsCompete) as ViewResult;
            var model = result.Model as ContestRegistrationViewModel;
            var contestRequiresPracticePassword = !string.IsNullOrEmpty(contest.ContestPassword);

            Assert.IsNotNull(model);
            Assert.AreEqual(contest.Name, model.ContestName);
            Assert.AreEqual(contestRequiresPracticePassword, model.RequirePassword);
            Assert.AreEqual(contest.Questions.Count, model.Questions.Count());
        }

        [TestMethod]
        public void RegisterActionWhenPracticeHasNoPasswordShouldRedirectToIndex()
        {
            var contest = this.CreateAndSaveContest("testContest", this.ActiveContestNoPasswordOptions, this.ActiveContestNoPasswordOptions);

            var result = this.CompeteController.Register(contest.Id, this.IsCompete) as RedirectToRouteResult;

            Assert.IsNull(result.RouteValues["controller"]);
            Assert.AreEqual("Index", result.RouteValues["action"]);
            Assert.AreEqual(contest.Id, result.RouteValues["id"]);
            Assert.AreEqual(this.IsCompete, result.RouteValues["official"]);
            Assert.IsTrue(this.EmptyOjsData.Participants.Any(contest.Id, this.FakeUserProfile.Id, this.IsCompete));
        }

        [TestMethod]
        public void RegisterActionWhenPostedDataAndInvalidContestIdWasProvidedShouldThrowException()
        {
            try
            {
                this.CompeteController.Register(this.IsPractice, new ContestRegistrationModel(this.EmptyOjsData));
                Assert.Fail("Expected an exception to be thrown when an invalid contest id is provided.");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.NotFound, ex.GetHttpCode());
            }
        }

        [TestMethod]
        public void RegisterActionWhenPostedDataAndContestCannotBePracticedShouldThrowException()
        {
            var contest = this.CreateAndSaveContest("testContest", this.InactiveContestOptions, this.InactiveContestOptions);

            try
            {
                var contestRegistrationModel = new ContestRegistrationModel(this.EmptyOjsData);
                contestRegistrationModel.ContestId = contest.Id;

                this.CompeteController.Register(this.IsPractice, contestRegistrationModel);
                Assert.Fail("Expected exception trying to register to practice contest, when practice is not available");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.Forbidden, ex.GetHttpCode());
            }
        }

        [TestMethod]
        public void RegisterActionWhenPostedDataAndContestCannotBeCompetedShouldThrowException()
        {
            var contest = this.CreateAndSaveContest("testContest", this.InactiveContestOptions, this.InactiveContestOptions);

            try
            {
                var contestRegistrationModel = new ContestRegistrationModel(this.EmptyOjsData);
                contestRegistrationModel.ContestId = contest.Id;
                this.CompeteController.Register(this.IsCompete, contestRegistrationModel);
                Assert.Fail("Expected exception trying to register to compete in a contest, when compete is not available");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.Forbidden, ex.GetHttpCode());
            }
        }

        [TestMethod]
        public void RegisterActionWhenPostedDataAndUserAlreadyRegisteredShouldRedirectToIndex()
        {
            var contest = this.CreateAndSaveContest("testContest", this.ActiveContestWithPasswordOptions, this.ActiveContestWithPasswordOptions);

            this.EmptyOjsData.Participants.Add(new Participant(contest.Id, this.FakeUserProfile.Id, this.IsCompete));
            this.EmptyOjsData.SaveChanges();

            var contestRegistrationModel = new ContestRegistrationModel(this.EmptyOjsData);
            contestRegistrationModel.ContestId = contest.Id;

            var result = this.CompeteController.Register(this.IsCompete, contestRegistrationModel) as RedirectToRouteResult;

            Assert.IsNull(result.RouteValues["controller"]);
            Assert.AreEqual("Index", result.RouteValues["action"]);
            Assert.AreEqual(contest.Id, result.RouteValues["id"]);
            Assert.AreEqual(this.IsCompete, result.RouteValues["official"]);
        }

        [TestMethod]
        public void RegisterActionWhenPostedDataContestHasPasswordAndProvidedIncorrectPasswordShouldReturnView()
        {
            var contest = this.CreateAndSaveContest("testContest", this.ActiveContestWithPasswordOptions, this.InactiveContestOptions);

            var contestRegistrationModel = new ContestRegistrationModel(this.EmptyOjsData);
            contestRegistrationModel.Password = "invalidPassword";
            contestRegistrationModel.ContestId = contest.Id;

            var result = this.CompeteController.Register(this.IsCompete, contestRegistrationModel) as ViewResult;
            var model = result.Model as ContestRegistrationViewModel;

            Assert.IsNotNull(model);
            Assert.IsNull(model.Password);
            Assert.IsFalse(this.CompeteController.ModelState.IsValid);
            Assert.IsNotNull(this.CompeteController.ModelState["Password"]);
            Assert.AreEqual(contest.Name, model.ContestName);
            Assert.IsTrue(model.RequirePassword);
        }

        [TestMethod]
        public void RegisterActionWhenPostedDataPracticeHasPasswordAndProvidedIncorrectPasswordShouldReturnView()
        {
            var contest = this.CreateAndSaveContest("testContest", this.InactiveContestOptions, this.ActiveContestWithPasswordOptions);

            var contestRegistrationModel = new ContestRegistrationModel(this.EmptyOjsData);
            contestRegistrationModel.Password = "invalidPassword";
            contestRegistrationModel.ContestId = contest.Id;

            var result = this.CompeteController.Register(this.IsPractice, contestRegistrationModel) as ViewResult;
            var model = result.Model as ContestRegistrationViewModel;

            Assert.IsNotNull(model);
            Assert.IsNull(model.Password);
            Assert.IsFalse(this.CompeteController.ModelState.IsValid);
            Assert.IsNotNull(this.CompeteController.ModelState["Password"]);
            Assert.AreEqual(contest.Name, model.ContestName);
            Assert.IsTrue(model.RequirePassword);
        }

        [TestMethod]
        public void RegisterActionWhenPostedDataContestHasPasswordAndProvidedCorrectPasswordShouldRedirectToIndex()
        {
            var contest = this.CreateAndSaveContest("contestName", this.ActiveContestWithPasswordOptions, this.InactiveContestOptions);

            var contestRegistrationModel = new ContestRegistrationModel(this.EmptyOjsData);
            contestRegistrationModel.Password = this.DefaultCompetePassword;
            contestRegistrationModel.ContestId = contest.Id;

            var result = this.CompeteController.Register(this.IsCompete, contestRegistrationModel) as RedirectToRouteResult;

            Assert.IsNull(result.RouteValues["controller"]);
            Assert.AreEqual("Index", result.RouteValues["action"]);
            Assert.AreEqual(this.IsCompete, result.RouteValues["official"]);
            Assert.AreEqual(contest.Id, result.RouteValues["id"]);
        }

        [TestMethod]
        public void RegisterActionWhenPostedDataPracticeHasPasswordAndProvidedCorrectPasswordShouldRedirectToIndex()
        {
            var contest = this.CreateAndSaveContest("contestName", this.InactiveContestOptions, this.ActiveContestWithPasswordOptions);

            var contestRegistrationModel = new ContestRegistrationModel(this.EmptyOjsData);
            contestRegistrationModel.Password = this.DefaultPracticePassword;
            contestRegistrationModel.ContestId = contest.Id;

            var result = this.CompeteController.Register(this.IsPractice, contestRegistrationModel) as RedirectToRouteResult;

            Assert.IsNull(result.RouteValues["controller"]);
            Assert.AreEqual("Index", result.RouteValues["action"]);
            Assert.AreEqual(this.IsPractice, result.RouteValues["official"]);
            Assert.AreEqual(contest.Id, result.RouteValues["id"]);
        }

        [TestMethod]
        public void RegisterActionWhenPostedDataContestHasPasswordAndQuestionsUnansweredShouldReturnView()
        {
            var contest = this.CreateAndSaveContest("testContest", this.ActiveContestWithPasswordAndQuestionsOptions, this.InactiveContestOptions);

            var contestRegistrationModel = new ContestRegistrationModel(this.EmptyOjsData);
            contestRegistrationModel.Password = this.DefaultCompetePassword;
            contestRegistrationModel.ContestId = contest.Id;

            this.TryValidateModel(contestRegistrationModel, this.CompeteController);
            var result = this.CompeteController.Register(this.IsCompete, contestRegistrationModel) as ViewResult;
            var resultModel = result.Model as ContestRegistrationViewModel;

            Assert.IsNotNull(resultModel);
            Assert.AreEqual(contest.Questions.Count, resultModel.Questions.Count());
            Assert.IsTrue(contest.HasContestPassword);
        }

        [TestMethod]
        public void RegisterActionWhenPostedDataPracticeHasPasswordAndQuestionsUnansweredShouldReturnView()
        {
            var contest = this.CreateAndSaveContest("testContest", this.InactiveContestOptions, this.ActiveContestWithPasswordAndQuestionsOptions);

            var contestRegistrationModel = new ContestRegistrationModel(this.EmptyOjsData);
            contestRegistrationModel.Password = this.DefaultPracticePassword;
            contestRegistrationModel.ContestId = contest.Id;

            this.TryValidateModel(contestRegistrationModel, this.CompeteController);
            var result = this.CompeteController.Register(this.IsPractice, contestRegistrationModel) as ViewResult;
            var resultModel = result.Model as ContestRegistrationViewModel;

            Assert.IsNotNull(resultModel);
            Assert.AreEqual(contest.Questions.Count, resultModel.Questions.Count());
            Assert.IsTrue(contest.HasPracticePassword);
        }

        [TestMethod]
        public void RegisterActionWhenPostedDataContestHasPasswordAndAllQuestionsAnsweredShouldRedirectToIndex()
        {
            var contest = this.CreateAndSaveContest("testContest", this.ActiveContestWithQuestionsOptions, this.InactiveContestOptions);

            var contestRegistrationModel = new ContestRegistrationModel(this.EmptyOjsData);
            contestRegistrationModel.Password = this.DefaultCompetePassword;
            contestRegistrationModel.ContestId = contest.Id;
            contestRegistrationModel.Questions = contest.Questions.Select(x => new ContestQuestionAnswerModel
            {
                QuestionId = x.Id,
                Answer = "answer"
            });

            var result = this.CompeteController.Register(this.IsCompete, contestRegistrationModel) as RedirectToRouteResult;

            Assert.IsNull(result.RouteValues["controller"]);
            Assert.AreEqual("Index", result.RouteValues["action"]);
            Assert.AreEqual(contest.Id, result.RouteValues["id"]);
            Assert.AreEqual(this.IsCompete, result.RouteValues["official"]);
        }
    }
}