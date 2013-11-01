namespace OJS.Data.Tests.Contest.EntityValidation
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Data.Contracts;

    [TestClass]
    public class TestContestEntityValidation : TestContestBaseData
    {
        [TestInitialize]
        public void CleanDatabase()
        {
            base.FullCleanDatabase();
        }

        [TestMethod]
        [ExpectedException(typeof(System.Data.Entity.Validation.DbEntityValidationException))]
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

            var result = this.EmptyOjsData.Contests.All()
                .Where(x => x.Name == "Invalid")
                .FirstOrDefault();
        }
    }
}
