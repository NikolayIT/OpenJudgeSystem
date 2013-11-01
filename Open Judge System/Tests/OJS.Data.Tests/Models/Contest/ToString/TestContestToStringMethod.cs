namespace OJS.Data.Tests.Contest.ToString
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Data.Contracts;

    [TestClass]
    public class TestContestToStringMethod : TestContestBaseData
    {
        public TestContestToStringMethod()
        {
            base.FullCleanDatabase();
        }

        [TestMethod]
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

            var result = this.EmptyOjsData.Contests.All()
                .Where(x => x.Name == "Contest")
                .FirstOrDefault().ToString();

            result = result.Substring(result.Length - 7);

            Assert.AreEqual("Contest", result);
        }
    }
}
