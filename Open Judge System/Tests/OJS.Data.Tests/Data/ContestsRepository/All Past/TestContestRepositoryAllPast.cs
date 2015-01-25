namespace OJS.Data.Tests.Data.ContestsRepository.AllPast
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using NUnit.Framework;

    using OJS.Data.Models;
    using OJS.Data.Tests.Data.ContestsRepository.Base_Data;

    [TestFixture]
    public class TestContestRepositoryAllPast : TestContestRepositoryBaseData
    {
        public TestContestRepositoryAllPast()
        {
            this.PopulateEmptyDataBaseWithContests();
            this.AllPast = this.Repository.AllPast().ToList();
        }

        private IList<Contest> AllPast { get; set; }

        [Test]
        public void ContestRepositoryShouldReturnProperActiveContestsCount()
        {
            Assert.AreEqual(20, this.AllPast.Count);
        }

        [Test]
        public void ContestRepositoryShouldReturnProperAllActiveNames()
        {
            for (int i = 1; i <= this.AllPast.Count; i++)
            {
                Assert.AreEqual((i + 10).ToString(), this.AllPast[i - 1].Name);
            }
        }

        [Test]
        public void ContestRepositoryShouldReturnProperAllActiveIsVisible()
        {
            for (int i = 1; i <= this.AllPast.Count; i++)
            {
                Assert.AreEqual(true, this.AllPast[i - 1].IsVisible);
            }
        }

        [Test]
        public void ContestRepositoryShouldReturnProperAllActiveIsDeleted()
        {
            for (int i = 1; i <= this.AllPast.Count; i++)
            {
                Assert.AreEqual(false, this.AllPast[i - 1].IsDeleted);
            }
        }

        [Test]
        public void ContestRepositoryShouldReturnProperAllActiveStartTime()
        {
            for (int i = 1; i <= this.AllPast.Count; i++)
            {
                bool result = this.AllPast[i - 1].StartTime < DateTime.Now;

                Assert.AreEqual(true, result);
            }
        }

        [Test]
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
