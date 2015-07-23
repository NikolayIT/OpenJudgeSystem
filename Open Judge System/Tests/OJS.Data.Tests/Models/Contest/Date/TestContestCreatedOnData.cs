namespace OJS.Data.Tests.Models.Contest.Date
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    using OJS.Data.Tests.Contest;

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
                    x.CreatedOn
                })
                .FirstOrDefault();

            var expected = DateTime.Now.AddSeconds(-5) < result.CreatedOn
                && result.CreatedOn < DateTime.Now.AddSeconds(5);

            Assert.IsTrue(expected);
        }

        [Test]
        public void CreatedOnShouldNotChangeAfterModification()
        {
            var createdBeforeModification =
                this.EmptyOjsData.Contests.All()
                    .Where(x => x.Name == "Created")
                    .Select(x => new { x.CreatedOn })
                    .FirstOrDefault()
                    .CreatedOn;

            this.EmptyOjsData.Contests.All().FirstOrDefault(x => x.Name == "Created").Name = "Modified";

            this.EmptyOjsData.SaveChanges();

            var createdAfterModification =
                this.EmptyOjsData.Contests.All()
                    .Where(x => x.Name == "Modified")
                    .Select(x => new { x.CreatedOn })
                    .FirstOrDefault()
                    .CreatedOn;

            var result = createdBeforeModification == createdAfterModification;

            Assert.IsTrue(result);
        }

        [Test]
        public void PreserveCreatedOnShouldNotAllowChangeInCreatedOn()
        {
            this.EmptyOjsData.Contests.All().FirstOrDefault(x => x.Name == "Created").PreserveCreatedOn = true;

            var originalCreatedOn = this.EmptyOjsData.Contests.All().FirstOrDefault(x => x.Name == "Created").CreatedOn;

            this.EmptyOjsData.SaveChanges();

            this.EmptyOjsData.Contests.All().FirstOrDefault(x => x.Name == "Created").CreatedOn.AddDays(5);

            var createdOn = this.EmptyOjsData.Contests.All().FirstOrDefault(x => x.Name == "Created").CreatedOn;

            var expected = originalCreatedOn == createdOn;

            Assert.IsTrue(expected);
        }
    }
}
