namespace OJS.Data.Tests.News.Date
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestNewsModifiedOndata : TestNewsBaseData
    {
        public TestNewsModifiedOndata()
        {
            base.PopulateEmptyDataBaseWithNews();
        }

        [TestMethod]
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

        [TestMethod]
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
