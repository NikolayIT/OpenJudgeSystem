namespace OJS.Web.Tests.Controllers.Contests.CompeteControllerTests
{
    using System;
    using System.Net;
    using System.Web;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Web.Areas.Contests.Controllers;
    using OJS.Data.Models;

    [TestClass]
    public class ValidateContestTests : CompeteControllerBaseTestsClass
    {
        [TestMethod]
        public void ValidateContestWhenContestIsNotFoundShouldAndTryingToPracticeThrowException()
        {
            try
            {
                CompeteController.ValidateContest(null, false);
                Assert.Fail("Expected an exception when contest is null");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.NotFound, ex.GetHttpCode());
            }
        }

        [TestMethod]
        public void ValidateContestWhenContestIsNotFoundShouldAndTryingToCompeteThrowException()
        {
            try
            {
                CompeteController.ValidateContest(null, true);
                Assert.Fail("Expected an exception when contest is null");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.NotFound, ex.GetHttpCode());
            }
        }

        [TestMethod]
        public void ValidateContestWhenContestIsInvisibleAndTryingToCompeteShouldThrowException()
        {
            var contest = new Contest
            {
                IsVisible = false
            };

            try
            {
                CompeteController.ValidateContest(contest, true);
                Assert.Fail("Expected an exception when trying to access a contest that is not visible");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.NotFound, ex.GetHttpCode());
            }
        }

        [TestMethod]
        public void ValidateContestWhenContestIsInvisibleAndTryingToPracticeShouldThrowException()
        {
            var contest = new Contest
            {
                IsVisible = false
            };

            try
            {
                CompeteController.ValidateContest(contest, false);
                Assert.Fail("Expected an exception when contest is null");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.NotFound, ex.GetHttpCode());
            }
        }

        [TestMethod]
        public void ValidateContestWhenContestIsDeletedAndTryingToPracticeShouldThrowException()
        {
            var contest = new Contest
            {
                IsDeleted = true
            };

            try
            {
                CompeteController.ValidateContest(contest, false);
                Assert.Fail("Expected an exception when contest is null");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.NotFound, ex.GetHttpCode());
            }
        }

        [TestMethod]
        public void ValidateContestWhenContestIsDeletedAndTryingToCompeteShouldThrowException()
        {
            var contest = new Contest
            {
                IsDeleted = true
            };

            try
            {
                CompeteController.ValidateContest(contest, false);
                Assert.Fail("Expected an exception when contest is null");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.NotFound, ex.GetHttpCode());
            }
        }

        [TestMethod]
        public void ValidateContestWhenContestCannotBeCompetedShouldThrowException()
        {
            var contest = new Contest
            {
                IsVisible = true
            };

            try
            {
                CompeteController.ValidateContest(contest, true);
                Assert.Fail("Expected an exception when contest is null");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.Forbidden, ex.GetHttpCode());
            }
        }

        [TestMethod]
        public void ValidateContestWhenContestCannotBePracticedShouldThrowException()
        {
            var contest = new Contest
            {
                IsVisible = true
            };

            try
            {
                CompeteController.ValidateContest(contest, false);
                Assert.Fail("Expected an exception when contest is null");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.Forbidden, ex.GetHttpCode());
            }
        }

        [TestMethod]
        public void ValidateContestWhenContestCanBePracticedShouldNotThrowAnException()
        {
            var contest = new Contest
            {
                IsVisible = true,
                PracticeStartTime = new DateTime(1990, 1, 1)
            };

            CompeteController.ValidateContest(contest, false);
        }

        [TestMethod]
        public void ValidateContestWhenContestCanBeCompetedShouldNotThrowAnException()
        {
            var contest = new Contest
            {
                IsVisible = true,
                StartTime = new DateTime(1990, 1, 1)
            };

            CompeteController.ValidateContest(contest, true);
        }

        [TestMethod]
        public void ValidateContestWhenContestCanBePracticedButTryingToCompeteItShouldThrowAnException()
        {
            var contest = new Contest
            {
                IsVisible = true,
                PracticeStartTime = new DateTime(1990, 1, 1)
            };

            try
            {
                CompeteController.ValidateContest(contest, true);
                Assert.Fail("Expected an exception when contest is null");                
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.Forbidden, ex.GetHttpCode());
            }
        }

        [TestMethod]
        public void ValidateContestWhenContestCanBeCompetedButTryingToPracticeItShouldThrowAnException()
        {
            var contest = new Contest
            {
                IsVisible = true,
                StartTime = new DateTime(1990, 1, 1)
            };

            try
            {
                CompeteController.ValidateContest(contest, false);
                Assert.Fail("Expected an exception when contest is null");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.Forbidden, ex.GetHttpCode());
            }
        }
    }
}
