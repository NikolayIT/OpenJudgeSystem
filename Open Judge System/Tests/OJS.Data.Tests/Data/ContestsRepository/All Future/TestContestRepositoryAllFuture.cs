namespace OJS.Data.Tests.Data.ContestsRepository.All_Future
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using NUnit.Framework;

    using OJS.Data.Models;
    using OJS.Data.Tests.Data.ContestsRepository.Base_Data;

    [TestFixture]
    public class TestContestRepositoryAllFuture : TestContestRepositoryBaseData
    {
        public TestContestRepositoryAllFuture()
        {
            this.PopulateEmptyDataBaseWithContests();
            this.AllFuture = this.Repository.AllFuture().ToList();
        }

        private IList<Contest> AllFuture { get; set; }

        [Test]
        public void ContestRepositoryShouldReturnProperActiveContestsCount()
        {
            Assert.AreEqual(10, this.AllFuture.Count);
        }

        [Test]
        public void ContestRepositoryShouldReturnProperAllActiveNames()
        {
            for (int i = 1; i <= this.AllFuture.Count; i++)
            {
                Assert.AreEqual((i + 30).ToString(CultureInfo.InvariantCulture), this.AllFuture[i - 1].Name);
            }
        }

        [Test]
        public void ContestRepositoryShouldReturnProperAllActiveIsVisible()
        {
            for (int i = 1; i <= this.AllFuture.Count; i++)
            {
                Assert.AreEqual(true, this.AllFuture[i - 1].IsVisible);
            }
        }

        [Test]
        public void ContestRepositoryShouldReturnProperAllActiveIsDeleted()
        {
            for (int i = 1; i <= this.AllFuture.Count; i++)
            {
                Assert.AreEqual(false, this.AllFuture[i - 1].IsDeleted);
            }
        }

        [Test]
        public void ContestRepositoryShouldReturnProperAllActiveStartTime()
        {
            for (int i = 1; i <= this.AllFuture.Count; i++)
            {
                bool result = this.AllFuture[i - 1].StartTime > DateTime.Now;

                Assert.AreEqual(true, result);
            }
        }

        [Test]
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
