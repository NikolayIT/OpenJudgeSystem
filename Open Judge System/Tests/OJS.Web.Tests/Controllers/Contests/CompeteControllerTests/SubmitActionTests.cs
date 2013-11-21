namespace OJS.Web.Tests.Controllers.Contests.CompeteControllerTests
{
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Data.Models;
    using OJS.Web.Areas.Contests.Models;

    [TestClass]
    public class SubmitActionTests : CompeteControllerBaseTestsClass
    {
        [TestMethod]
        public void SubmitActionWhenUserIsNotRegisteredToParticipateShouldThrowException()
        {
            var contest = this.CreateAndSaveContest("someContest", this.ActiveContestNoPasswordOptions, this.ActiveContestNoPasswordOptions);
            var problem = new Problem
            {
                Name = "test problem"
            };

            var submissionType = new SubmissionType
            {
                Name = "test submission type"
            };

            contest.Problems.Add(problem);
            contest.SubmissionTypes.Add(submissionType);
            this.EmptyOjsData.SaveChanges();

            var submission = new SubmissionModel
            {
                Content = "test submission",
                ProblemId = problem.Id,
                SubmissionTypeId = submissionType.Id
            };

            try
            {
                var result = this.CompeteController.Submit(submission, contest.Id, this.IsCompete);
                Assert.Fail("An exception was expected when a user is trying to submit for a contest that he isn't registered for.");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.Unauthorized, ex.GetHttpCode());
            }
        }

        [TestMethod]
        public void SubmitActionWhenParticipantSendsAValidSubmitShouldReturnJson()
        {
            var contest = this.CreateAndSaveContest("test contest", this.ActiveContestNoPasswordOptions, this.ActiveContestNoPasswordOptions);
            var problem = new Problem
            {
                Name = "test problem"
            };

            var submissionType = new SubmissionType
            {
                Name = "test submission type"
            };

            contest.Problems.Add(problem);
            contest.SubmissionTypes.Add(submissionType);
            contest.Participants.Add(new Participant(contest.Id, this.FakeUserProfile.Id, this.IsCompete));
            this.EmptyOjsData.SaveChanges();

            var submission = new SubmissionModel
            {
                Content = "test content",
                ProblemId = problem.Id,
                SubmissionTypeId = submissionType.Id
            };

            var result = this.CompeteController.Submit(submission, contest.Id, this.IsCompete) as JsonResult;
            var receivedContestId = (int)result.Data;
            Assert.AreEqual(receivedContestId, contest.Id);
        }

        [TestMethod]
        public void SubmitActionWhenParticipantSendsAnotherSubmissionBeforeLimitHasPassedShouldThrowException()
        {
            var contest = this.CreateAndSaveContest("test contest", this.ActiveContestNoPasswordOptions, this.ActiveContestNoPasswordOptions);
            contest.LimitBetweenSubmissions = 100;

            var problem = new Problem
            {
                Name = "test problem"
            };

            var submissionType = new SubmissionType
            {
                Name = "test submission type"
            };

            contest.Problems.Add(problem);
            contest.SubmissionTypes.Add(submissionType);
            contest.Participants.Add(new Participant(contest.Id, this.FakeUserProfile.Id, this.IsCompete));
            this.EmptyOjsData.SaveChanges();

            var submission = new SubmissionModel
            {
                Content = "test content",
                ProblemId = problem.Id,
                SubmissionTypeId = submissionType.Id
            };

            var result = this.CompeteController.Submit(submission, contest.Id, this.IsCompete) as JsonResult;
            var receivedContestId = (int)result.Data;
            Assert.AreEqual(receivedContestId, contest.Id);

            try
            {
                var secondSubmissionResult = this.CompeteController.Submit(submission, contest.Id, this.IsCompete);
                Assert.Fail("Expected an exception when a participant sends too many submissions");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.ServiceUnavailable, ex.GetHttpCode());
            }
        }

        [TestMethod]
        public void SubmitActionWhenParticipantSendsAnotherSubmissionAndThereIsNoLimitShouldReturnJson()
        {
            var contest = this.CreateAndSaveContest("test contest", this.ActiveContestNoPasswordOptions, this.ActiveContestNoPasswordOptions);

            // no limit between submissions
            contest.LimitBetweenSubmissions = 0;

            var problem = new Problem
            {
                Name = "test problem"
            };

            var submissionType = new SubmissionType
            {
                Name = "test submission type"
            };

            contest.Problems.Add(problem);
            contest.SubmissionTypes.Add(submissionType);
            contest.Participants.Add(new Participant(contest.Id, this.FakeUserProfile.Id, this.IsCompete));
            this.EmptyOjsData.SaveChanges();

            var submission = new SubmissionModel
            {
                Content = "test content",
                ProblemId = problem.Id,
                SubmissionTypeId = submissionType.Id
            };

            var result = this.CompeteController.Submit(submission, contest.Id, this.IsCompete) as JsonResult;
            var receivedContestId = (int)result.Data;
            Assert.AreEqual(receivedContestId, contest.Id);

            var secondSubmissionResult = this.CompeteController.Submit(submission, contest.Id, this.IsCompete) as JsonResult;
            var secondSubmissionResultContestId = (int)secondSubmissionResult.Data;

            Assert.AreEqual(receivedContestId, secondSubmissionResultContestId);
        }

        [TestMethod]
        public void SubmitActionWhenParticipantSendsEmptySubmissionContestShouldThrowException()
        {
            var contest = this.CreateAndSaveContest("test contest", this.ActiveContestNoPasswordOptions, this.ActiveContestNoPasswordOptions);

            // no limit between submissions
            contest.LimitBetweenSubmissions = 0;

            var problem = new Problem
            {
                Name = "test problem"
            };

            var submissionType = new SubmissionType
            {
                Name = "test submission type"
            };

            contest.Problems.Add(problem);
            contest.SubmissionTypes.Add(submissionType);
            contest.Participants.Add(new Participant(contest.Id, this.FakeUserProfile.Id, this.IsCompete));
            this.EmptyOjsData.SaveChanges();

            var submission = new SubmissionModel
            {
                Content = string.Empty,
                ProblemId = problem.Id,
                SubmissionTypeId = submissionType.Id
            };

            this.TryValidateModel(submission, this.CompeteController);

            try
            {
                var result = this.CompeteController.Submit(submission, contest.Id, this.IsCompete);
                Assert.Fail("Expected an exception when sending a submission with no content");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual((int)HttpStatusCode.BadRequest, ex.GetHttpCode());
            }
        }
    }
}
