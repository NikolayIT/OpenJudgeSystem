namespace OJS.Data.Tests.News.Date
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    [TestFixture]
    public class TestNewsModifiedOndata : TestNewsBaseData
    {
        public TestNewsModifiedOndata()
        {
            this.PopulateEmptyDataBaseWithNews();
        }

        [Test]
        public void ModifiedOnShouldBeNullOnCreation()
        {
            var result = this.EmptyOjsData.News.All()
                .Where(x => x.Title == "Created")
                .Select(x => new
                {
                    ModifiedOn = x.ModifiedOn
                })
                .FirstOrDefault();

            Assert.IsNull(result.ModifiedOn);
        }

        [Test]
        public void ModifiedOnShouldBeCorrectOnModification()
        {
            this.EmptyOjsData.News.All()
                .Where(x => x.Title == "Created")
                .FirstOrDefault().Title = "Modified";

            this.EmptyOjsData.SaveChanges();

            var result = this.EmptyOjsData.News.All()
                .Where(x => x.Title == "Modified")
                .Select(x => new
                {
                    ModifiedOn = x.ModifiedOn
                })
                .FirstOrDefault();

            bool expected = DateTime.Now.AddSeconds(-5) < result.ModifiedOn
                && result.ModifiedOn < DateTime.Now.AddSeconds(5);

            Assert.IsTrue(expected);
        }
    }
}
