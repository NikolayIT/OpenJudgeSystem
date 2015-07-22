namespace OJS.Data.Tests.News.Date
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    [TestFixture]
    public class TestContestDateData : TestNewsBaseData
    {
        public TestContestDateData()
        {
            this.PopulateEmptyDataBaseWithNews();
        }

        [Test]
        public void CreatedOnPropertyShouldBeInCorrectInterval()
        {
            var result =
                this.EmptyOjsData.News.All()
                    .Where(x => x.Title == "Created")
                    .Select(x => new { x.CreatedOn })
                    .FirstOrDefault();

            var expected = DateTime.Now.AddSeconds(-5) < result.CreatedOn
                           && result.CreatedOn < DateTime.Now.AddSeconds(5);

            Assert.IsTrue(expected);
        }

        [Test]
        public void CreatedOnShouldNotChangeAfterModification()
        {
            var createdBeforeModification =
                this.EmptyOjsData.News.All()
                    .Where(x => x.Title == "Created")
                    .Select(x => new { x.CreatedOn })
                    .FirstOrDefault()
                    .CreatedOn;

            this.EmptyOjsData.News.All().FirstOrDefault(x => x.Title == "Created").Title = "Modified";

            this.EmptyOjsData.SaveChanges();

            var createdAfterModification =
                this.EmptyOjsData.News.All()
                    .Where(x => x.Title == "Modified")
                    .Select(x => new { x.CreatedOn })
                    .FirstOrDefault()
                    .CreatedOn;

            var result = createdBeforeModification == createdAfterModification;

            Assert.IsTrue(result);
        }
    }
}
