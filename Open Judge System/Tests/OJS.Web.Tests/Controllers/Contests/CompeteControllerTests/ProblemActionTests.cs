namespace OJS.Web.Tests.Controllers.Contests.CompeteControllerTests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    using NUnit.Framework;

    using OJS.Data.Models;
    using OJS.Web.Areas.Contests.ViewModels.Contests;

    [TestFixture]
    public class ProblemActionTests : CompeteControllerBaseTestsClass
    {
        [Test]
        public void ProblemActionWhenIdIsInvalidAndTryingToPracticeShouldThrowException()
        {
            try
            {
                var result = this.CompeteController.Problem(-1, this.IsPractice);
                Assert.Fail("No exception was thrown when an invalid problem id was provided, but an exception was expected!");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.NotFound, ex.GetHttpCode());
            }
        }

        [Test]
        public void ProblemActionWhenIdIsInvalidAndTryingToCompeteShouldThrowException()
        {
            try
            {
                var result = this.CompeteController.Problem(-1, this.IsCompete);
                Assert.Fail("No exception was thrown when an invalid problem id was provided, but an exception was expected!");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.NotFound, ex.GetHttpCode());
            }
        }

        [Test]
        public void ProblemActionWhenContestCannotBeCompetedShouldThrowException()
        {
            var contest = this.CreateAndSaveContest("testContest", this.InactiveContestOptions, this.InactiveContestOptions);
            var problem = new Problem
                                    {
                                        ContestId = contest.Id,
                                        Name = "Sample Problem"
                                    };

            contest.Problems.Add(problem);
            this.EmptyOjsData.Contests.Add(contest);
            this.EmptyOjsData.SaveChanges();

            try
            {
                var result = this.CompeteController.Problem(problem.Id, this.IsCompete);
                Assert.Fail("No exception was thrown when a contest cannot be competed, but a contest roblem is requested.");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.Forbidden, ex.GetHttpCode());
            }
        }

        [Test]
        public void ProblemActionWhenContestCannotBePracticedShouldThrowException()
        {
            var contest = this.CreateAndSaveContest("testContest", this.InactiveContestOptions, this.InactiveContestOptions);
            var problem = new Problem
            {
                ContestId = contest.Id,
                Name = "Sample Problem"
            };

            contest.Problems.Add(problem);
            this.EmptyOjsData.SaveChanges();

            try
            {
                var result = this.CompeteController.Problem(problem.Id, this.IsPractice);
                Assert.Fail("No exception was thrown when a contest cannot be competed, but a contest roblem is requested.");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.Forbidden, ex.GetHttpCode());
            }
        }

        [Test]
        public void ProblemActionWhenContestCanBePracticedButUserIsNotRegisteredShouldRedirectToRegistration()
        {
            var contest = this.CreateAndSaveContest("testContest", this.ActiveContestWithPasswordAndQuestionsOptions, this.ActiveContestWithPasswordAndQuestionsOptions);
            var problem = new Problem
            {
                ContestId = contest.Id,
                Name = "Sample Problem"
            };

            contest.Problems.Add(problem);
            this.EmptyOjsData.SaveChanges();

            var result = this.CompeteController.Problem(problem.Id, this.IsPractice) as RedirectToRouteResult;

            Assert.IsNull(result.RouteValues["controller"]);
            Assert.AreEqual(contest.Id, result.RouteValues["id"]);
            Assert.AreEqual(this.IsPractice, result.RouteValues["official"]);
            Assert.AreEqual("Register", result.RouteValues["action"]);
        }

        [Test]
        public void ProblemActionWhenContestCanBeCompetedButUserIsNotRegisteredShouldRedirectToRegistration()
        {
            var contest = this.CreateAndSaveContest("testContest", this.ActiveContestWithPasswordAndQuestionsOptions, this.ActiveContestWithPasswordAndQuestionsOptions);
            var problem = new Problem
            {
                ContestId = contest.Id,
                Name = "Sample Problem"
            };

            contest.Problems.Add(problem);
            this.EmptyOjsData.SaveChanges();

            var result = this.CompeteController.Problem(problem.Id, this.IsCompete) as RedirectToRouteResult;

            Assert.IsNull(result.RouteValues["controller"]);
            Assert.AreEqual(contest.Id, result.RouteValues["id"]);
            Assert.AreEqual(this.IsCompete, result.RouteValues["official"]);
            Assert.AreEqual("Register", result.RouteValues["action"]);
        }

        [Test]
        public void ProblemActionWhenContestCanBeCompetedUserIsRegisteredAndProblemHasNoMaterialsShouldReturnPartialView()
        {
            var contest = this.CreateAndSaveContest("testContest", this.ActiveContestNoPasswordOptions, this.InactiveContestOptions);
            var problem = new Problem
            {
                ContestId = contest.Id,
                Name = "Sample Problem",
                Checker = new Checker { Name = "Checker" }
            };

            contest.Problems.Add(problem);
            contest.Participants.Add(new Participant(contest.Id, this.FakeUserProfile.Id, this.IsCompete));
            this.EmptyOjsData.SaveChanges();

            var result = this.CompeteController.Problem(problem.Id, this.IsCompete) as PartialViewResult;
            var model = result.Model as ContestProblemViewModel;

            Assert.AreEqual("Compete", result.ViewBag.CompeteType);
            Assert.IsNotNull(model);
            Assert.AreEqual(problem.Name, model.Name);
            Assert.AreEqual(problem.Id, model.ProblemId);
            Assert.AreEqual(problem.Resources.Count, model.Resources.Count());
        }

        [Test]
        public void ProblemActionWhenContestCanBeCompetedUserIsRegisteredAndProblemHasMaterialsShouldReturnPartialView()
        {
            var contest = this.CreateAndSaveContest("testContest", this.ActiveContestNoPasswordOptions, this.InactiveContestOptions);
            var problem = new Problem()
            {
                ContestId = contest.Id,
                Name = "Sample Problem",
                Resources = new HashSet<ProblemResource>
                {
                    new ProblemResource 
                    {
                        File = new byte[10],
                        FileExtension = "txt"
                    },
                    new ProblemResource 
                    {
                        File = new byte[100],
                        FileExtension = "docx"
                    },
                    new ProblemResource 
                    {
                        Link = "http://www.testlink.com"
                    }
                },
                Checker = new Checker { Name = "Checker" }
            };

            contest.Problems.Add(problem);
            this.EmptyOjsData.Participants.Add(new Participant(contest.Id, this.FakeUserProfile.Id, this.IsCompete));
            this.EmptyOjsData.SaveChanges();

            var result = this.CompeteController.Problem(problem.Id, this.IsCompete) as PartialViewResult;
            var model = result.Model as ContestProblemViewModel;

            Assert.AreEqual("Compete", result.ViewBag.CompeteType);
            Assert.IsNotNull(model);
            Assert.AreEqual(problem.Name, model.Name);
            Assert.AreEqual(problem.Id, model.ProblemId);
            Assert.AreEqual(problem.Resources.Count, model.Resources.Count());
        }
    }
}
