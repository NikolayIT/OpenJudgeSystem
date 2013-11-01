namespace OJS.Data.Tests.Data.ContestsRepository.AllPast
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Data.Contracts;

    [TestClass]
    public class TestContestRepositoryAllPast : TestContestRepositoryBaseData
    {
        private IList<Contest> AllPast { get; set; }

        public TestContestRepositoryAllPast()
        {
            base.PopulateEmptyDataBaseWithContests();
            this.AllPast = this.Repository.AllPast().ToList();
        }

        [TestMethod]
        public void ContestRepositoryShouldReturnProperActiveContestsCount()
        {
            Assert.AreEqual(20, this.AllPast.Count);
        }

        [TestMethod]
        public void ContestRepositoryShouldReturnProperAllActiveNames()
        {
            for (int i = 1; i <= this.AllPast.Count; i++)
            {
                Assert.AreEqual((i + 10).ToString(), this.AllPast[i - 1].Name);
            }
        }

        [TestMethod]
        public void ContestRepositoryShouldReturnProperAllActiveIsVisible()
        {
            for (int i = 1; i <= this.AllPast.Count; i++)
            {
                Assert.AreEqual(true, this.AllPast[i - 1].IsVisible);
            }
        }

        [TestMethod]
        public void ContestRepositoryShouldReturnProperAllActiveIsDeleted()
        {
            for (int i = 1; i <= this.AllPast.Count; i++)
            {
                Assert.AreEqual(false, this.AllPast[i - 1].IsDeleted);
            }
        }

        [TestMethod]
        public void ContestRepositoryShouldReturnProperAllActiveStartTime()
        {
            for (int i = 1; i <= this.AllPast.Count; i++)
            {
                bool result = this.AllPast[i - 1].StartTime < DateTime.Now;

                Assert.AreEqual(true, result);
            }
        }

        [TestMethod]
        public void ContestRepositoryShouldReturnProperAllActiveEndTime()
        {
            for (int i = 1; i <= this.AllPast.Count; i++)
            {
                bool result = this.AllPast[i - 1].EndTime < DateTime.Now;

                Assert.AreEqual(true, result);
            }
        }
    }
}
