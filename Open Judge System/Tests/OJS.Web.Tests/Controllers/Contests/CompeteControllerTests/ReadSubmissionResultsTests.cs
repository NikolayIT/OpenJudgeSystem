namespace OJS.Web.Tests.Controllers.Contests.CompeteControllerTests
{
    using System;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    using Kendo.Mvc.UI;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Data.Models;

    [TestClass]
    public class ReadSubmissionResultsTests : CompeteControllerBaseTestsClass
    {
        [TestMethod]
        public void ReadSubmissionResultsWhenUserNotRegisteredForContestShouldThrowException()
        {
            var contest = this.CreateAndSaveContest("contest", this.ActiveContestNoPasswordOptions, this.ActiveContestNoPasswordOptions);

            var problem = new Problem();
            contest.Problems.Add(problem);

            var submissionType = new SubmissionType();
            contest.SubmissionTypes.Add(submissionType);

            this.EmptyOjsData.SaveChanges();

            try
            {
                var result = this.CompeteController.ReadSubmissionResults(new DataSourceRequest(), problem.Id, this.IsCompete);
                Assert.Fail("Expected an exception when user is not registered for exam, but tries to access his results");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.Unauthorized, ex.GetHttpCode());
            }
        }

        [TestMethod]
        public void ReadSubmissionResultsWhenParticipantHasNoSubmissionShouldReturnNoResults()
        {
            var contest = this.CreateAndSaveContest("contest", this.ActiveContestNoPasswordOptions, this.ActiveContestNoPasswordOptions);
            var problem = new Problem
            {
                ShowResults = true
            };

            var submissionType = new SubmissionType();
            contest.Problems.Add(problem);
            contest.SubmissionTypes.Add(submissionType);
            contest.Participants.Add(new Participant(contest.Id, this.FakeUserProfile.Id, this.IsCompete));
            this.EmptyOjsData.SaveChanges();

            var result = this.CompeteController
                .ReadSubmissionResults(new DataSourceRequest(), problem.Id, this.IsCompete) as JsonResult;

            var responseData = result.Data as DataSourceResult;
            Assert.AreEqual(0, responseData.Total);
        }

        [TestMethod]
        public void ReadSubmissionResultsWhenParticipantHasSubmissionsShouldReturnNumberOfSubmissions()
        {
            var contest = this.CreateAndSaveContest("contest", this.ActiveContestNoPasswordOptions, this.ActiveContestNoPasswordOptions);

            var problem = new Problem
            {
                ShowResults = true
            };

            contest.Problems.Add(problem);

            var submissionType = new SubmissionType();
            contest.SubmissionTypes.Add(submissionType);

            var participant = new Participant(contest.Id, this.FakeUserProfile.Id, this.IsCompete);
            contest.Participants.Add(participant);

            for (int i = 0; i < 10; i++)
            {
                var submission = new Submission
                {
                    ContentAsString = "Test submission " + i,
                    SubmissionType = submissionType,
                    Problem = problem
                };

                participant.Submissions.Add(submission);
            }

            this.EmptyOjsData.SaveChanges();

            var result = this.CompeteController
                .ReadSubmissionResults(new DataSourceRequest(), problem.Id, this.IsCompete) as JsonResult;

            var responseData = result.Data as DataSourceResult;
            Assert.AreEqual(participant.Submissions.Count, responseData.Total);
        }
    }
}
