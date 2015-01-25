namespace OJS.Web.Tests.Controllers.Contests.CompeteControllerTests
{
    using System.Collections.Generic;
    using System.Net;
    using System.Web;

    using NUnit.Framework;

    using OJS.Data.Models;
    using OJS.Web.Areas.Contests.Controllers;

    [TestFixture]
    public class ValidateSubmissionTypeTests
    {
        [Test]
        public void ValidateSubmissionTypeWhenSubmissionTypeIsNotFoundThrowException()
        {
            var contest = new Contest
            {
                SubmissionTypes = new List<SubmissionType>
                {
                    new SubmissionType { Id = 1 },
                }
            };

            try
            {
                CompeteController.ValidateSubmissionType(0, contest);
                Assert.Fail("Expected an exception when submission type is null");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.BadRequest, ex.GetHttpCode());
            }
        }

        [Test]
        public void ValidateSubmissionTypeWhenSubmissionTypeIsFoundShouldNotThrowException()
        {
            var contest = new Contest
            {
                SubmissionTypes = new List<SubmissionType>
                {
                    new SubmissionType { Id = 1 },
                }
            };

            CompeteController.ValidateSubmissionType(1, contest);
        }
    }
}