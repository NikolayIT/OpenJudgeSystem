namespace OJS.Data.Tests.Contest.Competable
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Data;
    using OJS.Data.Contracts;
    using OJS.Data.Models;

    [TestClass]
    public class TestContestCanBeCompeted : TestContestBaseData
    {
        //// [TestInitialize]
        //// public void CleanDatabase()
        //// {
        ////     base.FullCleanDatabase();
        //// }

        [TestMethod]
        public void NonVisibleContestShouldNotBeCompeted()
        {
            var contest = new Contest
            {
                Name = "Contest",
                IsVisible = false,
                IsDeleted = false,
                StartTime = DateTime.Now.AddHours(-2),
                EndTime = DateTime.Now.AddHours(2),
            };

            var result = contest.CanBeCompeted;

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void DeletedContestShouldNotBeCompeted()
        {
            var contest = new Contest
            {
                Name = "Contest",
                IsVisible = true,
                IsDeleted = true,
                StartTime = DateTime.Now.AddHours(-2),
                EndTime = DateTime.Now.AddHours(2),
            };

            var result = contest.CanBeCompeted;

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ContestWithNoStartTimeShouldNotBeCompeted()
        {
            var contest = new Contest
            {
                Name = "Contest",
                IsVisible = true,
                IsDeleted = false,
                StartTime = null
            };

            var result = contest.CanBeCompeted;

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ContestWithNoEndTimeAndLaterStartTimeShouldNotBeCompeted()
        {
            var contest = new Contest
            {
                Name = "Contest",
                IsVisible = true,
                IsDeleted = false,
                StartTime = DateTime.Now.AddDays(2),
                EndTime = null
            };

            var result = contest.CanBeCompeted;

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ContestWithNoEndTimeAndEarlyStartTimeShouldNotBeCompeted()
        {
            var contest = new Contest
            {
                Name = "Contest",
                IsVisible = true,
                IsDeleted = false,
                StartTime = DateTime.Now.AddDays(-2),
                EndTime = null
            };

            var result = contest.CanBeCompeted;

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ContestWithEarlyStartTimeAndLateEndTimeShouldBeCompeted()
        {
            var contest = new Contest
            {
                Name = "Contest",
                IsVisible = true,
                IsDeleted = false,
                StartTime = DateTime.Now.AddDays(-2),
                EndTime = DateTime.Now.AddDays(2)
            };

            var result = contest.CanBeCompeted;

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ContestWithEarlyStartTimeAndEarlyEndTimeShouldNotBeCompeted()
        {
            var contest = new Contest
            {
                Name = "Contest",
                IsVisible = true,
                IsDeleted = false,
                StartTime = DateTime.Now.AddDays(-2),
                EndTime = DateTime.Now.AddDays(-1)
            };

            var result = contest.CanBeCompeted;

            Assert.IsFalse(result);
        }
    }
}
