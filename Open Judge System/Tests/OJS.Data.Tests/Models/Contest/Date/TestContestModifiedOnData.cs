namespace OJS.Data.Tests.Contest.Date
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestContestModifiedOnData : TestContestBaseData
    {
        public TestContestModifiedOnData()
        {
            this.PopulateEmptyDataBaseWithContest();
        }

        [TestMethod]
        public void ModifiedOnShouldBeNullOnCreation()
        {
            var result = this.EmptyOjsData.Contests.All()
                .Where(x => x.Name == "Created")
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
            this.EmptyOjsData.Contests.All()
                .Where(x => x.Name == "Created")
                .FirstOrDefault().Name = "Modified";

            this.EmptyOjsData.SaveChanges();

            var result = this.EmptyOjsData.Contests.All()
                .Where(x => x.Name == "Modified")
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
