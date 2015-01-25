namespace OJS.Web.Tests.Controllers.Contests.CompeteControllerTests
{
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    using NUnit.Framework;

    using OJS.Data.Models;
    using OJS.Web.Areas.Contests.Controllers;

    [TestFixture]
    public class DownloadResourceActionTests : CompeteControllerBaseTestsClass
    {
        [Test]
        public void DownloadResourceActionWhenPracticeAndInvalidResourceIdIsProvidedShouldThrowException()
        {
            try
            {
                var controller = this.CompeteController.DownloadResource(-1, this.IsPractice);
                Assert.Fail("Expected an exception, when trying to download and practice when a resource with an invalid id is provided.");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.NotFound, ex.GetHttpCode());
            }
        }

        [Test]
        public void DownloadResourceActionWhenCompeteAndInvalidResourceIdIsProvidedShouldThrowException()
        {
            try
            {
                var controller = this.CompeteController.DownloadResource(-1, this.IsCompete);
                Assert.Fail("Expected an exception, when trying to download and compete when a resource with an invalid id is provided.");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.NotFound, ex.GetHttpCode());
            }
        }

        [Test]
        public void DownloadResourceActionWhenPracticeAndUserNotLoggedInShouldRegirectToLoginPage()
        {
            var contest = this.CreateAndSaveContest("testContest", this.InactiveContestOptions, this.ActiveContestWithPasswordOptions);
            var problem = new Problem
            {
                Name = "test problem"
            };

            var resource = new ProblemResource
            {
                File = new byte[1],
                FileExtension = "test"
            };

            problem.Resources.Add(resource);
            contest.Problems.Add(problem);
            this.EmptyOjsData.SaveChanges();

            var controller = new CompeteController(this.EmptyOjsData, null);
            var result = controller.DownloadResource(resource.Id, this.IsPractice) as RedirectToRouteResult;

            Assert.AreEqual("Register", result.RouteValues["action"]);
            Assert.AreEqual(this.IsPractice, result.RouteValues["official"]);
            Assert.AreEqual(contest.Id, result.RouteValues["id"]);
        }

        [Test]
        public void DownloadResourceActionWhenPracticeAndUserNotRegisteredForPracticeShouldRegirectToRegistrationPage()
        {
            var contest = this.CreateAndSaveContest("testContest", this.InactiveContestOptions, this.ActiveContestWithPasswordOptions);
            var problem = new Problem
            {
                Name = "test problem"
            };

            var resource = new ProblemResource
            {
                File = new byte[1],
                FileExtension = "test"
            };

            problem.Resources.Add(resource);
            contest.Problems.Add(problem);
            this.EmptyOjsData.SaveChanges();

            var result = this.CompeteController.DownloadResource(resource.Id, this.IsPractice) as RedirectToRouteResult;

            Assert.AreEqual("Register", result.RouteValues["action"]);
            Assert.AreEqual(contest.Id, result.RouteValues["id"]);
            Assert.AreEqual(this.IsPractice, result.RouteValues["official"]);
        }

        [Test]
        public void DownloadResourceActionWhenCompeteAndUserNotRegisteredForCompeteShouldRegirectToRegistrationPage()
        {
            var contest = this.CreateAndSaveContest("testContest", this.InactiveContestOptions, this.ActiveContestWithPasswordOptions);
            var problem = new Problem
            {
                Name = "test problem"
            };

            var resource = new ProblemResource
            {
                File = new byte[1],
                FileExtension = "test"
            };

            problem.Resources.Add(resource);
            contest.Problems.Add(problem);
            this.EmptyOjsData.SaveChanges();

            var result = this.CompeteController.DownloadResource(resource.Id, this.IsPractice) as RedirectToRouteResult;

            Assert.AreEqual("Register", result.RouteValues["action"]);
            Assert.AreEqual(contest.Id, result.RouteValues["id"]);
            Assert.AreEqual(this.IsPractice, result.RouteValues["official"]);
        }

        [Test]
        public void DownloadResourceActionWhenPracticingButPracticeNotAllowedShouldThrowException()
        {
            var contest = this.CreateAndSaveContest("testContest", this.InactiveContestOptions, this.InactiveContestOptions);
            var problem = new Problem
            {
                Name = "test problem"
            };

            var resource = new ProblemResource
            {
                File = new byte[1],
                FileExtension = "test"
            };

            problem.Resources.Add(resource);
            contest.Problems.Add(problem);
            this.EmptyOjsData.SaveChanges();

            try
            {
                var result = this.CompeteController.DownloadResource(resource.Id, this.IsPractice) as RedirectToRouteResult;
                Assert.Fail("Expected an exception when a user is trying to download a resource for a contest that cannot be practiced.");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.Forbidden, ex.GetHttpCode());
            }
        }

        [Test]
        public void DownloadResourceActionWhenPracticingResourceIsAvailableAndUserIsRegisteredForPracticeShouldReturnResource()
        {
            var contest = this.CreateAndSaveContest("testContest", this.InactiveContestOptions, this.ActiveContestWithPasswordOptions);
            var problem = new Problem
            {
                Name = "test problem"
            };

            var resource = new ProblemResource
            {
                File = new byte[]
                {
                    (byte)this.RandomGenerator.Next(0, byte.MaxValue),
                    (byte)this.RandomGenerator.Next(0, byte.MaxValue)
                },
                Name = "testResource",
                FileExtension = "test"
            };

            problem.Resources.Add(resource);
            contest.Problems.Add(problem);
            contest.Participants.Add(new Participant(contest.Id, this.FakeUserProfile.Id, this.IsPractice));
            this.EmptyOjsData.SaveChanges();

            var result = this.CompeteController.DownloadResource(resource.Id, this.IsPractice) as FileContentResult;
            var expectedFileName = string.Format("{0}_{1}.{2}", problem.Name, resource.Name, resource.FileExtension);
            Assert.AreEqual(expectedFileName, result.FileDownloadName);
            Assert.IsTrue(resource.File.SequenceEqual(result.FileContents));
        }

        [Test]
        public void DownloadResourceActionWhenCompetingResourceIsAvailableAndUserIsRegisteredForCompeteShouldReturnResource()
        {
            var contest = this.CreateAndSaveContest("testContest", this.ActiveContestWithPasswordOptions, this.InactiveContestOptions);
            var problem = new Problem
            {
                Name = "test problem"
            };

            var resource = new ProblemResource
            {
                File = new byte[]
                {
                    (byte)this.RandomGenerator.Next(0, byte.MaxValue),
                    (byte)this.RandomGenerator.Next(0, byte.MaxValue)
                },
                Name = "resourceName",
                FileExtension = "test"
            };

            problem.Resources.Add(resource);
            contest.Problems.Add(problem);
            contest.Participants.Add(new Participant(contest.Id, this.FakeUserProfile.Id, this.IsCompete));
            this.EmptyOjsData.SaveChanges();

            var result = this.CompeteController.DownloadResource(resource.Id, this.IsCompete) as FileContentResult;
            var expectedFileName = string.Format("{0}_{1}.{2}", problem.Name, resource.Name, resource.FileExtension);
            Assert.AreEqual(expectedFileName, result.FileDownloadName);
            Assert.IsTrue(resource.File.SequenceEqual(result.FileContents));
        }

        [Test]
        public void DownloadResourceActionWhenPracticingResourceIsNullAndUserIsRegisteredForContestShouldThrowAnException()
        {
            var contest = this.CreateAndSaveContest("testContest", this.InactiveContestOptions, this.ActiveContestWithPasswordOptions);
            var problem = new Problem
            {
                Name = "test problem"
            };

            var resource = new ProblemResource
            {
                FileExtension = "test"
            };

            problem.Resources.Add(resource);
            contest.Problems.Add(problem);
            contest.Participants.Add(new Participant(contest.Id, this.FakeUserProfile.Id, this.IsPractice));
            this.EmptyOjsData.SaveChanges();

            try
            {
                var result = this.CompeteController.DownloadResource(resource.Id, this.IsPractice) as FileContentResult;
                Assert.Fail("Expected an exception when trying to download a resource where the file is null");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.Forbidden, ex.GetHttpCode());
            }
        }

        [Test]
        public void DownloadResourceActionWhenPracticingWhenNoFileExtensionAndUserIsRegisteredForContestShouldThrowAnException()
        {
            var contest = this.CreateAndSaveContest("testContest", this.InactiveContestOptions, this.ActiveContestWithPasswordOptions);
            var problem = new Problem
            {
                Name = "test problem"
            };

            var resource = new ProblemResource
            {
                File = new byte[]
                {
                    (byte)this.RandomGenerator.Next(0, byte.MaxValue),
                    (byte)this.RandomGenerator.Next(0, byte.MaxValue)
                }
            };

            problem.Resources.Add(resource);
            contest.Problems.Add(problem);
            contest.Participants.Add(new Participant(contest.Id, this.FakeUserProfile.Id, this.IsPractice));
            this.EmptyOjsData.SaveChanges();

            try
            {
                var result = this.CompeteController.DownloadResource(resource.Id, this.IsPractice) as FileContentResult;
                Assert.Fail("Expected an exception when trying to download a resource without a file extension.");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.Forbidden, ex.GetHttpCode());
            }
        }

        [Test]
        public void DownloadResourceActionWhenPracticingWhenFileHasNoContentAndUserIsRegisteredForContestShouldThrowAnException()
        {
            var contest = this.CreateAndSaveContest("testContest", this.InactiveContestOptions, this.ActiveContestWithPasswordOptions);
            var problem = new Problem
            {
                Name = "test problem"
            };

            var resource = new ProblemResource
            {
                File = new byte[0],
                FileExtension = "txt"
            };

            problem.Resources.Add(resource);
            contest.Problems.Add(problem);
            contest.Participants.Add(new Participant(contest.Id, this.FakeUserProfile.Id, this.IsPractice));
            this.EmptyOjsData.SaveChanges();

            try
            {
                var result = this.CompeteController.DownloadResource(resource.Id, this.IsPractice) as FileContentResult;
                Assert.Fail("Expected an exception when trying to download a resource with 0 file length.");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.Forbidden, ex.GetHttpCode());
            }
        }
    }
}
