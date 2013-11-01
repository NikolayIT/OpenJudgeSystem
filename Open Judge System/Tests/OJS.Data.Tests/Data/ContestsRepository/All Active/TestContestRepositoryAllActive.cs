namespace OJS.Data.Tests.Data.ContestsRepository.AllActive
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
    public class TestContestRepositoryAllActive : TestContestRepositoryBaseData
    {
        private IList<Contest> AllActive { get; set; }

        public TestContestRepositoryAllActive()
        {
            base.PopulateEmptyDataBaseWithContests();
            this.AllActive = this.Repository.AllActive().ToList();
        }

        [TestMethod]
        public void ContestRepositoryShouldReturnProperActiveContestsCount()
        {
            Assert.AreEqual(10, this.AllActive.Count);
        }

        [TestMethod]
        public void ContestRepositoryShouldReturnProperAllActiveNames()
        {
            for (int i = 1; i <= this.AllActive.Count; i++)
            {
                Assert.AreEqual(i.ToString(), this.AllActive[i - 1].Name);
            }
        }

        [TestMethod]
        public void ContestRepositoryShouldReturnProperAllActiveIsVisible()
        {
            for (int i = 1; i <= this.AllActive.Count; i++)
            {
                Assert.AreEqual(true, this.AllActive[i - 1].IsVisible);
            }
        }

        [TestMethod]
        public void ContestRepositoryShouldReturnProperAllActiveIsDeleted()
        {
            for (int i = 1; i <= this.AllActive.Count; i++)
            {
                Assert.AreEqual(false, this.AllActive[i - 1].IsDeleted);
            }
        }

        [TestMethod]
        public void ContestRepositoryShouldReturnProperAllActiveStartTime()
        {
            for (int i = 1; i <= this.AllActive.Count; i++)
            {
                bool result = this.AllActive[i - 1].StartTime <= DateTime.Now;

                Assert.AreEqual(true, result);
            }
        }

        [TestMethod]
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
