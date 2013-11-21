namespace OJS.Data.Tests.Data.ContestsRepository.AllVisible
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Data;
    using OJS.Data.Contracts;
    using OJS.Data.Models;

    [TestClass]
    public class TestContestRepositoryAllVisible : TestContestRepositoryBaseData
    {
        public TestContestRepositoryAllVisible()
        {
            this.PopulateEmptyDataBaseWithContests();
            this.AllVisible = this.Repository.AllVisible().ToList();
        }

        private IList<Contest> AllVisible { get; set; }

        [TestMethod]
        public void ContestRepositoryShouldReturnProperActiveContestsCount()
        {
            Assert.AreEqual(40, this.AllVisible.Count);
        }

        [TestMethod]
        public void ContestRepositoryShouldReturnProperAllActiveNames()
        {
            for (int i = 1; i <= this.AllVisible.Count; i++)
            {
                Assert.AreEqual(i.ToString(), this.AllVisible[i - 1].Name);
            }
        }

        [TestMethod]
        public void ContestRepositoryShouldReturnProperAllActiveIsVisible()
        {
            for (int i = 1; i <= this.AllVisible.Count; i++)
            {
                Assert.AreEqual(true, this.AllVisible[i - 1].IsVisible);
            }
        }

        [TestMethod]
        public void ContestRepositoryShouldReturnProperAllActiveIsDeleted()
        {
            for (int i = 1; i <= this.AllVisible.Count; i++)
            {
                Assert.AreEqual(false, this.AllVisible[i - 1].IsDeleted);
            }
        }
    }
}
