namespace OJS.Tests.Common.DataFakes
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;

    using OJS.Data;
    using OJS.Data.Models;

    internal sealed class DatabaseConfiguration : DbMigrationsConfiguration<FakeOjsDbContext>
    {
        public DatabaseConfiguration()
        {
            this.AutomaticMigrationsEnabled = true;
            this.AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(FakeOjsDbContext context)
        {
            context.ClearDatabase();
            this.SeedContests(context);
        }

        private void SeedContests(IOjsDbContext context)
        {
            // active contests
            var visibleActiveContest = new Contest
            {
                Name = "Active-Visible",
                IsVisible = true,
                IsDeleted = false,
                StartTime = DateTime.Now.AddHours(-2),
                EndTime = DateTime.Now.AddHours(2),
            };

            var nonVisibleActiveContest = new Contest
            {
                Name = "Active-Non-Visible",
                IsVisible = false,
                IsDeleted = false,
                StartTime = DateTime.Now.AddHours(-2),
                EndTime = DateTime.Now.AddHours(2)
            };

            var visibleDeletedActiveContest = new Contest
            {
                Name = "Active-Visible-Deleted",
                IsVisible = true,
                IsDeleted = true,
                StartTime = DateTime.Now.AddHours(-2),
                EndTime = DateTime.Now.AddHours(2)
            };

            var nonVisibleDeletedActiveContest = new Contest
            {
                Name = "Active-Non-Visible-Deleted",
                IsVisible = false,
                IsDeleted = true,
                StartTime = DateTime.Now.AddHours(-2),
                EndTime = DateTime.Now.AddHours(2)
            };

            // past contests
            var visiblePastContest = new Contest
            {
                Name = "Past-Visible",
                IsVisible = true,
                IsDeleted = false,
                StartTime = DateTime.Now.AddHours(-2),
                EndTime = DateTime.Now.AddHours(-1)
            };

            var nonVisiblePastContest = new Contest
            {
                Name = "Past-Non-Visible",
                IsVisible = false,
                IsDeleted = false,
                StartTime = DateTime.Now.AddHours(-2),
                EndTime = DateTime.Now.AddHours(-1)
            };

            var visiblePastDeletedContest = new Contest
            {
                Name = "Past-Visible-Deleted",
                IsVisible = true,
                IsDeleted = true,
                StartTime = DateTime.Now.AddHours(-2),
                EndTime = DateTime.Now.AddHours(-1)
            };

            var nonVisiblePastDeletedContest = new Contest
            {
                Name = "Past-Non-Visible-Deleted",
                IsVisible = false,
                IsDeleted = true,
                StartTime = DateTime.Now.AddHours(-2),
                EndTime = DateTime.Now.AddHours(-1)
            };

            // future contests
            var visibleFutureContest = new Contest
            {
                Name = "Future-Visible",
                IsVisible = true,
                IsDeleted = false,
                StartTime = DateTime.Now.AddHours(1),
                EndTime = DateTime.Now.AddHours(2)
            };

            var nonVisibleFutureContest = new Contest
            {
                Name = "Future-Non-Visible",
                IsVisible = false,
                IsDeleted = false,
                StartTime = DateTime.Now.AddHours(1),
                EndTime = DateTime.Now.AddHours(2)
            };

            var visibleFutureDeletedContest = new Contest
            {
                Name = "Future-Visible-Deleted",
                IsVisible = true,
                IsDeleted = true,
                StartTime = DateTime.Now.AddHours(1),
                EndTime = DateTime.Now.AddHours(2)
            };

            var nonVisibleFutureDeletedContest = new Contest
            {
                Name = "Future-Non-Visible-Deleted",
                IsVisible = false,
                IsDeleted = true,
                StartTime = DateTime.Now.AddHours(1),
                EndTime = DateTime.Now.AddHours(2)
            };

            var contests = new List<Contest>()
                {
                    // active contests in list
                    (Contest)visibleActiveContest.ObjectClone(),
                    (Contest)visibleActiveContest.ObjectClone(),
                    (Contest)nonVisibleActiveContest.ObjectClone(),
                    (Contest)nonVisibleActiveContest.ObjectClone(),
                    (Contest)nonVisibleActiveContest.ObjectClone(),
                    (Contest)visibleDeletedActiveContest.ObjectClone(),
                    (Contest)visibleDeletedActiveContest.ObjectClone(),
                    (Contest)visibleDeletedActiveContest.ObjectClone(),
                    (Contest)visibleDeletedActiveContest.ObjectClone(),
                    (Contest)nonVisibleDeletedActiveContest.ObjectClone(),

                    // past contest in list
                    (Contest)visiblePastContest.ObjectClone(),
                    (Contest)visiblePastContest.ObjectClone(),
                    (Contest)visiblePastContest.ObjectClone(),
                    (Contest)nonVisiblePastContest.ObjectClone(),
                    (Contest)nonVisiblePastContest.ObjectClone(),
                    (Contest)nonVisiblePastContest.ObjectClone(),
                    (Contest)nonVisiblePastContest.ObjectClone(),
                    (Contest)visiblePastDeletedContest.ObjectClone(),
                    (Contest)visiblePastDeletedContest.ObjectClone(),
                    (Contest)visiblePastDeletedContest.ObjectClone(),
                    (Contest)visiblePastDeletedContest.ObjectClone(),
                    (Contest)nonVisiblePastDeletedContest.ObjectClone(),

                    // future contests in list
                    (Contest)visibleFutureContest.ObjectClone(),
                    (Contest)visibleFutureContest.ObjectClone(),
                    (Contest)visibleFutureContest.ObjectClone(),
                    (Contest)visibleFutureContest.ObjectClone(),
                    (Contest)nonVisibleFutureContest.ObjectClone(),
                    (Contest)nonVisibleFutureContest.ObjectClone(),
                    (Contest)nonVisibleFutureContest.ObjectClone(),
                    (Contest)visibleFutureDeletedContest.ObjectClone(),
                    (Contest)visibleFutureDeletedContest.ObjectClone(),
                    (Contest)visibleFutureDeletedContest.ObjectClone(),
                    (Contest)visibleFutureDeletedContest.ObjectClone(),
                    (Contest)visibleFutureDeletedContest.ObjectClone(),
                    (Contest)nonVisibleFutureDeletedContest.ObjectClone(),
                };

            foreach (var contest in contests)
            {
                context.Contests.Add(contest);
            }

            context.SaveChanges();
        }
    }
}
