namespace OJS.Web.Tests.Controllers.Contests.CompeteControllerTests
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OJS.Data.Models;
    using System.Web;
    using System.Net;

    [TestClass]
    public class GetSubmissionContentActionTests : CompeteControllerBaseTestsClass
    {
        [TestMethod]
        public void GetSubmissionContentWhenInvalidSubmissionIdShouldThrowException()
        {
            var contest = this.CreateAndSaveContest("sample Name", this.ActiveContestNoPasswordOptions, this.ActiveContestNoPasswordOptions);
            var problem = new Problem();
            contest.Problems.Add(problem);

            var submissionType = new SubmissionType();
            contest.SubmissionTypes.Add(submissionType);

            var participant = new Participant(contest.Id, this.FakeUserProfile.Id, this.IsCompete);
            contest.Participants.Add(participant);
            var submission = new Submission
            {
                ContentAsString = "test content"
            };

            participant.Submissions.Add(submission);
            this.EmptyOjsData.SaveChanges();

            try
            {
                var result = this.CompeteController.GetSubmissionContent(-1);
                Assert.Fail("Expected an exception when an invalid submission id is provided.");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.NotFound, ex.GetHttpCode());
            }
        }

        [TestMethod]
        public void GetSubmissionContentWhenSubmissionNotMadeByTheParticipantShouldThrowException()
        {
            var contest = this.CreateAndSaveContest("sample Name", this.ActiveContestNoPasswordOptions, this.ActiveContestNoPasswordOptions);
            var problem = new Problem();
            contest.Problems.Add(problem);

            var submissionType = new SubmissionType();
            contest.SubmissionTypes.Add(submissionType);

            var participant = new Participant(contest.Id, this.FakeUserProfile.Id, this.IsCompete);

            var anotherUser = new UserProfile
            {
                UserName = "test user",
                Email = "testmail@testprovider.com"
            };

            this.EmptyOjsData.Users.Add(anotherUser);
            var anotherParticipant = new Participant(contest.Id, anotherUser.Id, this.IsCompete);

            contest.Participants.Add(participant);
            contest.Participants.Add(anotherParticipant);

            var submission = new Submission
            {
                ContentAsString = "test content"
            };

            anotherParticipant.Submissions.Add(submission);
            this.EmptyOjsData.SaveChanges();

            try
            {
                var result = this.CompeteController.GetSubmissionContent(submission.Id);
                Assert.Fail("Expected an exception when trying to download a submission that was not made by the participant that requested it.");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.Forbidden, ex.GetHttpCode());
            }
        }

        [TestMethod]
        public void GetSubmissionContentActionWhenInvalidSubmissionIdShouldThrowException()
        {
        }
    }
}
