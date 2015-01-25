namespace OJS.Data.Tests.Data.ContestsRepository.AllActive
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using NUnit.Framework;

    using OJS.Data.Models;
    using OJS.Data.Tests.Data.ContestsRepository.Base_Data;

    [TestFixture]
    public class TestContestRepositoryAllActive : TestContestRepositoryBaseData
    {
        public TestContestRepositoryAllActive()
        {
            this.PopulateEmptyDataBaseWithContests();
            this.AllActive = this.Repository.AllActive().ToList();
        }

        private IList<Contest> AllActive { get; set; }

        [Test]
        public void ContestRepositoryShouldReturnProperActiveContestsCount()
        {
            Assert.AreEqual(10, this.AllActive.Count);
        }

        [Test]
        public void ContestRepositoryShouldReturnProperAllActiveNames()
        {
            for (int i = 1; i <= this.AllActive.Count; i++)
            {
                Assert.AreEqual(i.ToString(), this.AllActive[i - 1].Name);
            }
        }

        [Test]
        public void ContestRepositoryShouldReturnProperAllActiveIsVisible()
        {
            for (int i = 1; i <= this.AllActive.Count; i++)
            {
                Assert.AreEqual(true, this.AllActive[i - 1].IsVisible);
            }
        }

        [Test]
        public void ContestRepositoryShouldReturnProperAllActiveIsDeleted()
        {
            for (int i = 1; i <= this.AllActive.Count; i++)
            {
                Assert.AreEqual(false, this.AllActive[i - 1].IsDeleted);
            }
        }

        [Test]
        public void ContestRepositoryShouldReturnProperAllActiveStartTime()
        {
            for (int i = 1; i <= this.AllActive.Count; i++)
            {
                bool result = this.AllActive[i - 1].StartTime <= DateTime.Now;

                Assert.AreEqual(true, result);
            }
        }

        [Test]
        public void ContestRepositoryShouldReturnProperAllActiveEndTime()
        {
            for (int i = 1; i <= this.AllActive.Count; i++)
            {
                bool result = this.AllActive[i - 1].EndTime >= DateTime.Now;

                Assert.AreEqual(true, result);
            }
        }
    }
}
