namespace OJS.Data.Tests.Contest
{
    using System;

    using NUnit.Framework;

    using OJS.Data.Models;
    using OJS.Tests.Common;

    [TestFixture]
    public class TestContestBaseData : TestClassBase
    {
        protected void PopulateEmptyDataBaseWithContest()
        {
            this.FullCleanDatabase();

            this.EmptyOjsData.Contests.Add(new Contest
            {
                Name = "Created",
                IsVisible = true,
                IsDeleted = false,
                PracticeStartTime = DateTime.Now.AddHours(-2),
                PracticeEndTime = DateTime.Now.AddHours(2),
            });

            this.EmptyOjsData.SaveChanges();
        }

        protected void FullCleanDatabase()
        {
            this.EmptyOjsData.Context.ClearDatabase();
        }
    }
}
