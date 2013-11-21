namespace OJS.Data.Tests.Contest
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Data;
    using OJS.Data.Contracts;
    using OJS.Data.Models;
    using OJS.Tests.Common;
    using OJS.Tests.Common.DataFakes;

    [TestClass]
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
