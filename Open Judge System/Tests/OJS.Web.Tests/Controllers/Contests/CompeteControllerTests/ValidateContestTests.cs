namespace OJS.Web.Tests.Controllers.Contests.CompeteControllerTests
{
    using System;
    using System.Net;
    using System.Web;

    using NUnit.Framework;

    using OJS.Data.Models;
    using OJS.Web.Areas.Contests.Controllers;

    [TestFixture]
    public class ValidateContestTests : CompeteControllerBaseTestsClass
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
        public void ValidateContestWhenContestCanBePracticedShouldNotThrowAnException()
        {
            var contest = new Contest
            {
                IsVisible = true,
                PracticeStartTime = new DateTime(1990, 1, 1)
            };

            CompeteController.ValidateContest(contest, false);
        }

        [Test]
        public void ValidateContestWhenContestCanBeCompetedShouldNotThrowAnException()
        {
            var contest = new Contest
            {
                IsVisible = true,
                StartTime = new DateTime(1990, 1, 1)
            };

            CompeteController.ValidateContest(contest, true);
        }

        [Test]
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

        [Test]
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
