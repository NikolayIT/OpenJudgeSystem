namespace OJS.Data.Tests.Contest.EntityValidation
{
    using System;
    using System.Data.Entity.Validation;
    using System.Linq;

    using NUnit.Framework;

    using OJS.Data.Models;

    [TestFixture]
    public class TestContestEntityValidation : TestContestBaseData
    {
        [SetUp]
        public void CleanDatabase()
        {
            this.FullCleanDatabase();
        }

        [Test]
        [ExpectedException(typeof(DbEntityValidationException))]
        public void ContestShouldNotBeAddedIfStartTimeIsLaterThanEndTime()
        {
            this.EmptyOjsData.Contests.Add(new Contest
            {
                Name = "Invalid",
                IsVisible = true,
                IsDeleted = false,
                StartTime = DateTime.Now.AddHours(2),
                EndTime = DateTime.Now.AddHours(-2),
            });

            this.EmptyOjsData.SaveChanges();

            var result = this.EmptyOjsData.Contests.All().FirstOrDefault(x => x.Name == "Invalid");
        }
    }
}
