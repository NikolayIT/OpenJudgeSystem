namespace OJS.Data.Tests.Contest.ToString
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    using OJS.Data.Models;

    [TestFixture]
    public class TestContestToStringMethod : TestContestBaseData
    {
        public TestContestToStringMethod()
        {
            this.FullCleanDatabase();
        }

        [Test]
        public void ToStringMethodShouldReturnProperValue()
        {
            this.EmptyOjsData.Contests.Add(new Contest
            {
                Name = "Contest",
                IsVisible = false,
                IsDeleted = false,
                PracticeStartTime = DateTime.Now.AddHours(-2),
                PracticeEndTime = DateTime.Now.AddHours(2),
            });

            this.EmptyOjsData.SaveChanges();

            var result = this.EmptyOjsData.Contests.All().FirstOrDefault(x => x.Name == "Contest").ToString();

            result = result.Substring(result.Length - 7);

            Assert.AreEqual("Contest", result);
        }
    }
}
