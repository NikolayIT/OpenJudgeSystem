namespace OJS.Web.Tests.Controllers.Contests.CompeteControllerTests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    using NUnit.Framework;

    using OJS.Common.Models;
    using OJS.Data.Models;

    using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

    [TestFixture]
    public class GetAllowedSubmissionTypesTests : CompeteControllerBaseTestsClass
    {
        [Test]
        public void GetAllowedSubmissionTypesWhenAnInvalidContestIdIsProvidedShouldThrowAnException()
        {
            try
            {
                var result = this.CompeteController.GetAllowedSubmissionTypes(-1);
                Assert.Fail("Expected an exception when an invalid contest id is provided.");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.NotFound, ex.GetHttpCode());
            }
        }

        [Test]
        public void GetAllowedSubmissionTypesWhenRequestIsValidShouldReturnSubmissionTypes()
        {
            var contest = this.CreateAndSaveContest("testName", this.ActiveContestWithPasswordAndQuestionsOptions, this.ActiveContestWithPasswordAndQuestionsOptions);

            var javaSubmissionType = new SubmissionType
            {
                IsSelectedByDefault = true,
                Name = "java type",
                CompilerType = CompilerType.Java,
            };

            var csharpSubmissionType = new SubmissionType
            {
                Name = "c# type",
                CompilerType = CompilerType.CSharp
            };

            contest.SubmissionTypes.Add(javaSubmissionType);
            contest.SubmissionTypes.Add(csharpSubmissionType);
            this.EmptyOjsData.SaveChanges();

            var result = this.CompeteController.GetAllowedSubmissionTypes(contest.Id) as JsonResult;
            var data = result.Data as IEnumerable<SelectListItem>;
            Assert.IsNotNull(result);
            Assert.AreEqual(contest.SubmissionTypes.Count, data.Count());
            Assert.IsFalse(data.Any(x => !contest.SubmissionTypes.Any(t => t.Id.ToString() == x.Value && t.Name == x.Text)));
            Assert.IsTrue(data.FirstOrDefault(x => x.Text == "java type").Selected);
        }
    }
}
