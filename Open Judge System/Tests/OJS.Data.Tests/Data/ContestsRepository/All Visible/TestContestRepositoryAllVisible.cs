namespace OJS.Data.Tests.Data.ContestsRepository.AllVisible
{
    using System.Collections.Generic;
    using System.Linq;

    using NUnit.Framework;

    using OJS.Data.Models;
    using OJS.Data.Tests.Data.ContestsRepository.Base_Data;

    [TestFixture]
    public class TestContestRepositoryAllVisible : TestContestRepositoryBaseData
    {
        public TestContestRepositoryAllVisible()
        {
            this.PopulateEmptyDataBaseWithContests();
            this.AllVisible = this.Repository.AllVisible().ToList();
        }

        private IList<Contest> AllVisible { get; set; }

        [Test]
        public void ContestRepositoryShouldReturnProperActiveContestsCount()
        {
            Assert.AreEqual(40, this.AllVisible.Count);
        }

        [Test]
        public void ContestRepositoryShouldReturnProperAllActiveNames()
        {
            for (int i = 1; i <= this.AllVisible.Count; i++)
            {
                Assert.AreEqual(i.ToString(), this.AllVisible[i - 1].Name);
            }
        }

        [Test]
        public void ContestRepositoryShouldReturnProperAllActiveIsVisible()
        {
            for (int i = 1; i <= this.AllVisible.Count; i++)
            {
                Assert.AreEqual(true, this.AllVisible[i - 1].IsVisible);
            }
        }

        [Test]
        public void ContestRepositoryShouldReturnProperAllActiveIsDeleted()
        {
            for (int i = 1; i <= this.AllVisible.Count; i++)
            {
                Assert.AreEqual(false, this.AllVisible[i - 1].IsDeleted);
            }
        }
    }
}
