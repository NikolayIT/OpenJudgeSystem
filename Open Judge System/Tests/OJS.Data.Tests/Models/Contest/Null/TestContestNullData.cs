namespace OJS.Data.Tests.Contest.Null
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestContestNullData : TestContestBaseData
    {
        public TestContestNullData()
        {
            base.PopulateEmptyDataBaseWithContest();
        }

        [TestMethod]
        public void CreatedContestShouldNotBeNull()
        {
            var result = EmptyOjsData.Contests.All()
                .Where(x => x.Name == "Created")
                .Select(x => new
                {
                    CreatedOn = x.CreatedOn
                })
                .FirstOrDefault();

            Assert.IsNotNull(result);
        }
    }
}
