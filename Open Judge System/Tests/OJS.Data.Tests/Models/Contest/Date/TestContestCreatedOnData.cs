namespace OJS.Data.Tests.Contest.Date
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    [TestFixture]
    public class TestContestCreatedOnData : TestContestBaseData
    {
        public TestContestCreatedOnData()
        {
            this.PopulateEmptyDataBaseWithContest();
        }

        [Test]
        public void CreatedOnPropertyShouldBeInCorrectInterval()
        {
            var result = this.EmptyOjsData.Contests.All()
                .Where(x => x.Name == "Created")
                .Select(x => new
                {
                    CreatedOn = x.CreatedOn
                })
                .FirstOrDefault();

            bool expected = DateTime.Now.AddSeconds(-5) < result.CreatedOn
                && result.CreatedOn < DateTime.Now.AddSeconds(5);

            Assert.IsTrue(expected);
        }

        [Test]
        public void CreatedOnShouldNotChangeAfterModification()
        {
            var createdBeforeModification = this.EmptyOjsData.Contests.All()
                .Where(x => x.Name == "Created")
                .Select(x => new
                {
                    CreatedOn = x.CreatedOn
                })
                .FirstOrDefault().CreatedOn;

            this.EmptyOjsData.Contests.All()
                .Where(x => x.Name == "Created")
                .FirstOrDefault().Name = "Modified";

            this.EmptyOjsData.SaveChanges();

            var createdAfterModification = this.EmptyOjsData.Contests.All()
                .Where(x => x.Name == "Modified")
                .Select(x => new
                {
                    CreatedOn = x.CreatedOn
                })
                .FirstOrDefault().CreatedOn;

            bool result = createdBeforeModification == createdAfterModification;

            Assert.IsTrue(result);
        }

        [Test]
        public void PreserveCreatedOnShouldNotAllowChangeInCreatedOn()
        {
            this.EmptyOjsData.Contests.All()
                .Where(x => x.Name == "Created")
                .FirstOrDefault().PreserveCreatedOn = true;

            var originalCreatedOn = this.EmptyOjsData.Contests.All()
                .Where(x => x.Name == "Created")
                .FirstOrDefault().CreatedOn;

            this.EmptyOjsData.SaveChanges();

            this.EmptyOjsData.Contests.All()
                .Where(x => x.Name == "Created")
                .FirstOrDefault().CreatedOn.AddDays(5);

            var createdOn = this.EmptyOjsData.Contests.All()
                .Where(x => x.Name == "Created")
                .FirstOrDefault().CreatedOn;

            bool expected = originalCreatedOn == createdOn;

            Assert.IsTrue(expected);
        }
    }
}
