namespace OJS.Data.Tests.Contest.Date
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    using OJS.Data.Models;

    [TestFixture]
    public class TestContestDeletedData : TestContestBaseData
    {
        public TestContestDeletedData()
        {
            this.PopulateEmptyDataBaseWithContest();
        }

        [Test]
        public void IsDeletedShouldReturnProperValueAfterDeletion()
        {
            var result = this.EmptyOjsData.Contests.All().FirstOrDefault(x => x.Name == "Created");

            this.EmptyOjsData.Contests.Delete(result);
            this.EmptyOjsData.SaveChanges();

            bool deletedOnActual = this.EmptyOjsData.Contests.AllWithDeleted().First(x => x.Name == "Created").IsDeleted;

            Assert.IsTrue(deletedOnActual);
        }

        [Test]
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

            var result = this.EmptyOjsData.Contests.All().FirstOrDefault(x => x.Name == "ToBeDeleted");

            this.EmptyOjsData.Contests.Delete(result);
            this.EmptyOjsData.SaveChanges();

            var deletedOnActual =
                this.EmptyOjsData.Contests.AllWithDeleted().First(x => x.Name == "ToBeDeleted").DeletedOn;

            bool expected = DateTime.Now.AddSeconds(-5) < deletedOnActual
                && deletedOnActual < DateTime.Now.AddSeconds(5);

            Assert.IsTrue(expected);
        }
    }
}
