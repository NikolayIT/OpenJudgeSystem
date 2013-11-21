namespace OJS.Data.Tests.Contest.Date
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Data;
    using OJS.Data.Contracts;
    using OJS.Data.Models;

    [TestClass]
    public class TestContestDeletedData : TestContestBaseData
    {
        public TestContestDeletedData()
        {
            this.PopulateEmptyDataBaseWithContest();
        }

        [TestMethod]
        public void IsDeletedShouldReturnProperValueAfterDeletion()
        {
            var result = this.EmptyOjsData.Contests.All()
                .Where(x => x.Name == "Created")
                .FirstOrDefault();

            this.EmptyOjsData.Contests.Delete(result);
            this.EmptyOjsData.SaveChanges();

            bool deletedOnActual = this.EmptyOjsData.Contests.AllWithDeleted()
                .Where(x => x.Name == "Created").First().IsDeleted;

            Assert.IsTrue(deletedOnActual);
        }

        [TestMethod]
        public void DeletedOnShouldReturnProperValueAfterDeletion()
        {
            this.EmptyOjsData.Contests.Add(new Contest
            {
                Name = "ToBeDeleted",
                IsVisible = true,
                IsDeleted = false,
                PracticeStartTime = DateTime.Now.AddHours(-2),
                PracticeEndTime = DateTime.Now.AddHours(2),
            });

            this.EmptyOjsData.SaveChanges();

            var result = this.EmptyOjsData.Contests.All()
                .Where(x => x.Name == "ToBeDeleted")
                .FirstOrDefault();

            this.EmptyOjsData.Contests.Delete(result);
            this.EmptyOjsData.SaveChanges();

            var deletedOnActual = this.EmptyOjsData.Contests.AllWithDeleted()
                .Where(x => x.Name == "ToBeDeleted").First().DeletedOn;

            bool expected = DateTime.Now.AddSeconds(-5) < deletedOnActual
                && deletedOnActual < DateTime.Now.AddSeconds(5);

            Assert.IsTrue(expected);
        }
    }
}
