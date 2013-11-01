namespace OJS.Data.Tests.Contest.Practicable
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Data.Contracts;

    [TestClass]
    public class TestContestCanBePracticed : TestContestBaseData
    {
        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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
