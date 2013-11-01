namespace OJS.Data.Tests.Data.ContestsRepository
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Data.Models;
    using OJS.Data.Repositories;
    using OJS.Tests.Common;

    [TestClass]
    public class TestContestRepositoryBaseData : TestClassBase
    {
        public TestContestRepositoryBaseData()
        {
            this.Repository = new ContestsRepository(this.EmptyOjsData.Context);
        }

        protected ContestsRepository Repository { get; set; }

        protected void PopulateEmptyDataBaseWithContests()
        {
            this.InitializeEmptyOjsData();

            for (int i = 1; i <= 10; i++)
            {
                this.EmptyOjsData.Contests.Add(new Contest
                {
                    Name = i.ToString(),
                    IsVisible = true,
                    IsDeleted = false,
                    StartTime = DateTime.Now.AddHours(-2),
                    EndTime = DateTime.Now.AddHours(2),
                });
            }

            for (int i = 11; i <= 30; i++)
            {
                this.EmptyOjsData.Contests.Add(new Contest
                {
                    Name = i.ToString(),
                    IsVisible = true,
                    IsDeleted = false,
                    StartTime = DateTime.Now.AddHours(-2),
                    EndTime = DateTime.Now.AddHours(-1),
                });
            }

            for (int i = 31; i <= 40; i++)
            {
                this.EmptyOjsData.Contests.Add(new Contest
                {
                    Name = i.ToString(),
                    IsVisible = true,
                    IsDeleted = false,
                    StartTime = DateTime.Now.AddHours(2),
                    EndTime = DateTime.Now.AddHours(10),
                });
            }

            for (int i = 41; i <= 55; i++)
            {
                this.EmptyOjsData.Contests.Add(new Contest
                {
                    Name = i.ToString(),
                    IsVisible = false,
                    IsDeleted = false,
                    StartTime = DateTime.Now.AddHours(2),
                    EndTime = DateTime.Now.AddHours(10),
                });
            }

            this.EmptyOjsData.SaveChanges();
        }
    }
}
