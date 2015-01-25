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
            var result = this.EmptyOjsData.News.All()
                .Where(x => x.Title == "Created")
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
            var createdBeforeModification = this.EmptyOjsData.News.All()
                .Where(x => x.Title == "Created")
                .Select(x => new
                {
                    CreatedOn = x.CreatedOn
                })
                .FirstOrDefault().CreatedOn;

            this.EmptyOjsData.News.All()
                .Where(x => x.Title == "Created")
                .FirstOrDefault().Title = "Modified";

            this.EmptyOjsData.SaveChanges();

            var createdAfterModification = this.EmptyOjsData.News.All()
                .Where(x => x.Title == "Modified")
                .Select(x => new
                {
                    CreatedOn = x.CreatedOn
                })
                .FirstOrDefault().CreatedOn;

            bool result = createdBeforeModification == createdAfterModification;

            Assert.IsTrue(result);
        }
    }
}
