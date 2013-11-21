namespace OJS.Data.Tests.Data.ContestsRepository.All_Future
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Data.Models;

    [TestClass]
    public class TestContestRepositoryAllFuture : TestContestRepositoryBaseData
    {
        public TestContestRepositoryAllFuture()
        {
            this.PopulateEmptyDataBaseWithContests();
            this.AllFuture = this.Repository.AllFuture().ToList();
        }

        private IList<Contest> AllFuture { get; set; }

        [TestMethod]
        public void ContestRepositoryShouldReturnProperActiveContestsCount()
        {
            Assert.AreEqual(10, this.AllFuture.Count);
        }

        [TestMethod]
        public void ContestRepositoryShouldReturnProperAllActiveNames()
        {
            for (int i = 1; i <= this.AllFuture.Count; i++)
            {
                Assert.AreEqual((i + 30).ToString(CultureInfo.InvariantCulture), this.AllFuture[i - 1].Name);
            }
        }

        [TestMethod]
        public void ContestRepositoryShouldReturnProperAllActiveIsVisible()
        {
            for (int i = 1; i <= this.AllFuture.Count; i++)
            {
                Assert.AreEqual(true, this.AllFuture[i - 1].IsVisible);
            }
        }

        [TestMethod]
        public void ContestRepositoryShouldReturnProperAllActiveIsDeleted()
        {
            for (int i = 1; i <= this.AllFuture.Count; i++)
            {
                Assert.AreEqual(false, this.AllFuture[i - 1].IsDeleted);
            }
        }

        [TestMethod]
        public void ContestRepositoryShouldReturnProperAllActiveStartTime()
        {
            for (int i = 1; i <= this.AllFuture.Count; i++)
            {
                bool result = this.AllFuture[i - 1].StartTime > DateTime.Now;

                Assert.AreEqual(true, result);
            }
        }

        [TestMethod]
        public void ContestRepositoryShouldReturnProperAllActiveEndTime()
        {
            for (int i = 1; i <= this.AllFuture.Count; i++)
            {
                bool result = this.AllFuture[i - 1].EndTime > DateTime.Now;

                Assert.AreEqual(true, result);
            }
        }
    }
}
