namespace OJS.Data.Tests.Contest.Practicable
{
    using System;

    using NUnit.Framework;

    using OJS.Data.Models;

    using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

    [TestFixture]
    public class TestContestCanBePracticed : TestContestBaseData
    {
        [Test]
        public void NonVisibleContestShouldNotBePracticed()
        {
            var contest = new Contest
            {
                Name = "Contest",
                IsVisible = false,
                IsDeleted = false,
                PracticeStartTime = DateTime.Now.AddHours(-2),
                PracticeEndTime = DateTime.Now.AddHours(2),
            };

            var result = contest.CanBePracticed;

            Assert.IsFalse(result);
        }

        [Test]
        public void DeletedContestShouldNotBePracticed()
        {
            var contest = new Contest
            {
                Name = "Contest",
                IsVisible = true,
                IsDeleted = true,
                PracticeStartTime = DateTime.Now.AddHours(-2),
                PracticeEndTime = DateTime.Now.AddHours(2),
            };

            var result = contest.CanBePracticed;

            Assert.IsFalse(result);
        }

        [Test]
        public void ContestWithNoStartTimeShouldNotBePracticed()
        {
            var contest = new Contest
            {
                Name = "Contest",
                IsVisible = true,
                IsDeleted = false,
                PracticeStartTime = null
            };

            var result = contest.CanBePracticed;

            Assert.IsFalse(result);
        }

        [Test]
        public void ContestWithNoEndTimeAndLaterStartTimeShouldNotBePracticed()
        {
            var contest = new Contest
            {
                Name = "Contest",
                IsVisible = true,
                IsDeleted = false,
                PracticeStartTime = DateTime.Now.AddDays(2),
                PracticeEndTime = null
            };

            var result = contest.CanBePracticed;

            Assert.IsFalse(result);
        }

        [Test]
        public void ContestWithNoEndTimeAndEarlyStartTimeShouldNotBePracticed()
        {
            var contest = new Contest
            {
                Name = "Contest",
                IsVisible = true,
                IsDeleted = false,
                PracticeStartTime = DateTime.Now.AddDays(-2),
                PracticeEndTime = null
            };

            var result = contest.CanBePracticed;

            Assert.IsTrue(result);
        }

        [Test]
        public void ContestWithEarlyStartTimeAndLateEndTimeShouldBePracticed()
        {
            var contest = new Contest
            {
                Name = "Contest",
                IsVisible = true,
                IsDeleted = false,
                PracticeStartTime = DateTime.Now.AddDays(-2),
                PracticeEndTime = DateTime.Now.AddDays(2)
            };

            var result = contest.CanBePracticed;

            Assert.IsTrue(result);
        }

        [Test]
        public void ContestWithEarlyStartTimeAndEarlyEndTimeShouldBePracticed()
        {
            var contest = new Contest
            {
                Name = "Contest",
                IsVisible = true,
                IsDeleted = false,
                PracticeStartTime = DateTime.Now.AddDays(-2),
                PracticeEndTime = DateTime.Now.AddDays(-1)
            };

            var result = contest.CanBePracticed;

            Assert.IsFalse(result);
        }
    }
}
