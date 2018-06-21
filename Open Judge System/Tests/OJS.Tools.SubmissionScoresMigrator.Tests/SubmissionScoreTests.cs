namespace OJS.Tools.SubmissionScoresMigrator.Tests
{
    using System.Linq;
    using Data.Models;

    using global::SubmissionScoresMigrator;

    using NUnit.Framework;

    [TestFixture]
    public class SubmissionScoreTests
    {
        [Test]
        public void GetSubmissionsShouldReturnCorrectResults()
        {
            var submissions = this.GetSubmissions().AsQueryable();

            var result = Startup.GetBestSubmissions(submissions, 0, 2);

            Assert.AreEqual(2, result.Length);

            var first = result.First();
            var second = result.Last();

            this.AssertSubmission(
                submission: first,
                submissionId: 10,
                participantId: 2,
                problemId: 2,
                isOfficial: false,
                participantName: "Second",
                points: 60);

            this.AssertSubmission(
                submission: second,
                submissionId: 8,
                participantId: 2,
                problemId: 2,
                isOfficial: true,
                participantName: "Second",
                points: 60);

            result = Startup.GetBestSubmissions(submissions, 1, 2);

            Assert.AreEqual(2, result.Length);

            first = result.First();
            second = result.Last();

            this.AssertSubmission(
                submission: first,
                submissionId: 7,
                participantId: 2,
                problemId: 1,
                isOfficial: true,
                participantName: "Second",
                points: 40);

            this.AssertSubmission(
                submission: second,
                submissionId: 4,
                participantId: 1,
                problemId: 2,
                isOfficial: true,
                participantName: "First",
                points: 100);

            result = Startup.GetBestSubmissions(submissions, 2, 2);

            Assert.AreEqual(1, result.Length);

            first = result.First();

            this.AssertSubmission(
                submission: first,
                submissionId: 2,
                participantId: 1,
                problemId: 1,
                isOfficial: true,
                participantName: "First",
                points: 90);
        }

        private void AssertSubmission(
            SubmissionScoreModel submission,
            int submissionId,
            int participantId,
            int problemId,
            bool isOfficial,
            string participantName,
            int points)
        {
            Assert.NotNull(submission);
            Assert.AreEqual(submissionId, submission.SubmissionId);
            Assert.AreEqual(participantId, submission.ParticipantId);
            Assert.AreEqual(problemId, submission.ProblemId);
            Assert.AreEqual(isOfficial, submission.IsOfficial);
            Assert.AreEqual(participantName, submission.ParticipantName);
            Assert.AreEqual(points, submission.Points);
        }

        private Submission[] GetSubmissions()
            => new Submission[]
            {
                new Submission
                {
                    Id = 2,
                    ParticipantId = 1,
                    ProblemId = 1,
                    Participant = new Participant { IsOfficial = true, User = new UserProfile { UserName = "First" } },
                    Points = 90,
                },
                new Submission
                {
                    Id = 9,
                    ParticipantId = 2,
                    ProblemId = 2,
                    Participant = new Participant { IsOfficial = false, User = new UserProfile { UserName = "Second" } },
                    Points = 60,
                },
                new Submission
                {
                    Id = 4,
                    ParticipantId = 1,
                    ProblemId = 2,
                    Participant = new Participant { IsOfficial = true, User = new UserProfile { UserName = "First" } },
                    Points = 100,
                },
                new Submission
                {
                    Id = 10,
                    ParticipantId = 2,
                    ProblemId = 2,
                    Participant = new Participant { IsOfficial = false, User = new UserProfile { UserName = "Second" } },
                    Points = 60,
                },
                new Submission
                {
                    Id = 3,
                    ParticipantId = 1,
                    ProblemId = 1,
                    Participant = new Participant { IsOfficial = true, User = new UserProfile { UserName = "First" } },
                    Points = 80,
                },
                new Submission
                {
                    Id = 1,
                    IsDeleted = true
                },
                new Submission
                {
                    Id = 7,
                    ParticipantId = 2,
                    ProblemId = 1,
                    Participant = new Participant { IsOfficial = true, User = new UserProfile { UserName = "Second" } },
                    Points = 40,
                },
                new Submission
                {
                    Id = 5,
                    IsDeleted = true
                },
                new Submission
                {
                    Id = 8,
                    ParticipantId = 2,
                    ProblemId = 2,
                    Participant = new Participant { IsOfficial = true, User = new UserProfile { UserName = "Second" } },
                    Points = 60,
                },
                new Submission
                {
                    Id = 6,
                    ParticipantId = 2,
                    ProblemId = 1,
                    Participant = new Participant { IsOfficial = true, User = new UserProfile { UserName = "Second" } },
                    Points = 30,
                },
            };
    }
}
