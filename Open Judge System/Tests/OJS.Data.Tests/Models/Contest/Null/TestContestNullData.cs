namespace OJS.Data.Tests.Contest.Null
{
    using System.Linq;
    
    using NUnit.Framework;

    using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

    [TestFixture]
    public class TestContestNullData : TestContestBaseData
    {
        public TestContestNullData()
        {
            this.PopulateEmptyDataBaseWithContest();
        }

        [Test]
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
