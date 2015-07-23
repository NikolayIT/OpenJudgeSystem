namespace OJS.Data.Tests.Data.ContestsRepository.AllVisibleInCategory
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    using OJS.Data.Models;
    using OJS.Data.Tests.Data.ContestsRepository.Base_Data;
    using OJS.Tests.Common.DataFakes;

    [TestFixture]
    public class TestContestRepositoryAllVisibleInCategory : TestContestRepositoryBaseData
    {
        public TestContestRepositoryAllVisibleInCategory()
        {
            var firstCategory = new ContestCategory();
            var secondCategory = new ContestCategory();
            var thirdCategory = new ContestCategory();

            var firstInnerCategory = new ContestCategory();
            var secondInnerCategory = new ContestCategory();
            var thirdInnerCategory = new ContestCategory();
            var fourthInnerCategory = new ContestCategory();
            var fifthInnerCategory = new ContestCategory();
            var sixthInnerCategory = new ContestCategory();

            firstCategory.Children.Add(firstInnerCategory);
            firstCategory.Children.Add(secondInnerCategory);
            secondCategory.Children.Add(thirdInnerCategory);
            secondCategory.Children.Add(fourthInnerCategory);
            thirdCategory.Children.Add(fifthInnerCategory);
            thirdCategory.Children.Add(sixthInnerCategory);

            this.EmptyOjsData.ContestCategories.Add(firstCategory);
            this.EmptyOjsData.ContestCategories.Add(secondCategory);
            this.EmptyOjsData.ContestCategories.Add(thirdCategory);

            for (var i = 0; i < 5; i++)
            {
                var visibleActiveContest = new Contest
                {
                    Name = "Visible", 
                    IsVisible = true, 
                    IsDeleted = false, 
                    StartTime = DateTime.Now.AddDays(-2), 
                    EndTime = DateTime.Now.AddDays(2)
                };

                var visiblePastContest = new Contest
                {
                    Name = "Visible", 
                    IsVisible = true, 
                    IsDeleted = false, 
                    StartTime = DateTime.Now.AddDays(-2), 
                    EndTime = DateTime.Now.AddDays(-1)
                };

                var visibleFutureContest = new Contest
                {
                    Name = "Visible", 
                    IsVisible = true, 
                    IsDeleted = false, 
                    StartTime = DateTime.Now.AddDays(2), 
                    EndTime = DateTime.Now.AddDays(4)
                };

                var nonVisibleContest = new Contest
                {
                    Name = "NonVisible", 
                    IsVisible = false, 
                    IsDeleted = false, 
                    StartTime = DateTime.Now.AddDays(-2), 
                    EndTime = DateTime.Now.AddDays(4)
                };

                var deletedVisibleContest = new Contest
                {
                    Name = "DeletedVisible", 
                    IsVisible = true, 
                    IsDeleted = true, 
                    StartTime = DateTime.Now.AddDays(-2), 
                    EndTime = DateTime.Now.AddDays(4)
                };

                firstCategory.Contests.Add((Contest)visibleActiveContest.ObjectClone());
                firstCategory.Contests.Add((Contest)visiblePastContest.ObjectClone());
                firstCategory.Contests.Add((Contest)visibleFutureContest.ObjectClone());
                firstCategory.Contests.Add((Contest)nonVisibleContest.ObjectClone());
                firstCategory.Contests.Add((Contest)deletedVisibleContest.ObjectClone());
                secondCategory.Contests.Add((Contest)visibleActiveContest.ObjectClone());
                secondCategory.Contests.Add((Contest)visiblePastContest.ObjectClone());
                secondCategory.Contests.Add((Contest)visibleFutureContest.ObjectClone());
                secondCategory.Contests.Add((Contest)nonVisibleContest.ObjectClone());
                secondCategory.Contests.Add((Contest)deletedVisibleContest.ObjectClone());
                thirdCategory.Contests.Add((Contest)visibleActiveContest.ObjectClone());
                thirdCategory.Contests.Add((Contest)visiblePastContest.ObjectClone());
                thirdCategory.Contests.Add((Contest)visibleFutureContest.ObjectClone());
                thirdCategory.Contests.Add((Contest)nonVisibleContest.ObjectClone());
                thirdCategory.Contests.Add((Contest)deletedVisibleContest.ObjectClone());
                firstInnerCategory.Contests.Add((Contest)visibleActiveContest.ObjectClone());
                firstInnerCategory.Contests.Add((Contest)visiblePastContest.ObjectClone());
                firstInnerCategory.Contests.Add((Contest)visibleFutureContest.ObjectClone());
                firstInnerCategory.Contests.Add((Contest)nonVisibleContest.ObjectClone());
                firstInnerCategory.Contests.Add((Contest)deletedVisibleContest.ObjectClone());
                secondInnerCategory.Contests.Add((Contest)visibleActiveContest.ObjectClone());
                secondInnerCategory.Contests.Add((Contest)visiblePastContest.ObjectClone());
                secondInnerCategory.Contests.Add((Contest)visibleFutureContest.ObjectClone());
                secondInnerCategory.Contests.Add((Contest)nonVisibleContest.ObjectClone());
                secondInnerCategory.Contests.Add((Contest)deletedVisibleContest.ObjectClone());
                thirdInnerCategory.Contests.Add((Contest)visibleActiveContest.ObjectClone());
                thirdInnerCategory.Contests.Add((Contest)visiblePastContest.ObjectClone());
                thirdInnerCategory.Contests.Add((Contest)visibleFutureContest.ObjectClone());
                thirdInnerCategory.Contests.Add((Contest)nonVisibleContest.ObjectClone());
                thirdInnerCategory.Contests.Add((Contest)deletedVisibleContest.ObjectClone());
                fourthInnerCategory.Contests.Add((Contest)visibleActiveContest.ObjectClone());
                fourthInnerCategory.Contests.Add((Contest)visiblePastContest.ObjectClone());
                fourthInnerCategory.Contests.Add((Contest)visibleFutureContest.ObjectClone());
                fourthInnerCategory.Contests.Add((Contest)nonVisibleContest.ObjectClone());
                fourthInnerCategory.Contests.Add((Contest)deletedVisibleContest.ObjectClone());
                fifthInnerCategory.Contests.Add((Contest)visibleActiveContest.ObjectClone());
                fifthInnerCategory.Contests.Add((Contest)visiblePastContest.ObjectClone());
                fifthInnerCategory.Contests.Add((Contest)visibleFutureContest.ObjectClone());
                fifthInnerCategory.Contests.Add((Contest)nonVisibleContest.ObjectClone());
                fifthInnerCategory.Contests.Add((Contest)deletedVisibleContest.ObjectClone());
                sixthInnerCategory.Contests.Add((Contest)visibleActiveContest.ObjectClone());
                sixthInnerCategory.Contests.Add((Contest)visiblePastContest.ObjectClone());
                sixthInnerCategory.Contests.Add((Contest)visibleFutureContest.ObjectClone());
                sixthInnerCategory.Contests.Add((Contest)nonVisibleContest.ObjectClone());
                sixthInnerCategory.Contests.Add((Contest)deletedVisibleContest.ObjectClone());
            }

            this.EmptyOjsData.SaveChanges();
        }

        [Test]
        public void ContestRepositoryShouldReturnProperVisibleInCategoryContestsCount()
        {
            var categories = this.EmptyOjsData.ContestCategories.All().ToList();

            foreach (var category in categories)
            {
                var contestsInCurrentCategory = this.EmptyOjsData.Contests.AllVisibleInCategory(category.Id).ToList();

                Assert.AreEqual(15, contestsInCurrentCategory.Count);
            }
        }

        [Test]
        public void ContestRepositoryShouldReturnProperVisibleInCategoryContestsNames()
        {
            var categories = this.EmptyOjsData.ContestCategories.All().ToList();

            foreach (var category in categories)
            {
                var contestsInCurrentCategory = this.EmptyOjsData.Contests.AllVisibleInCategory(category.Id).ToList();

                foreach (var contest in contestsInCurrentCategory)
                {
                    Assert.AreEqual("Visible", contest.Name);
                }
            }
        }

        [Test]
        public void ContestRepositoryShouldReturnProperVisibleInCategoryVisibleProperty()
        {
            var categories = this.EmptyOjsData.ContestCategories.All().ToList();

            foreach (var category in categories)
            {
                var contestsInCurrentCategory = this.EmptyOjsData.Contests.AllVisibleInCategory(category.Id).ToList();

                foreach (var contest in contestsInCurrentCategory)
                {
                    Assert.AreEqual(true, contest.IsVisible);
                }
            }
        }

        [Test]
        public void ContestRepositoryShouldReturnProperVisibleInCategoryIsDeleted()
        {
            var categories = this.EmptyOjsData.ContestCategories.All().ToList();

            foreach (var category in categories)
            {
                var contestsInCurrentCategory = this.EmptyOjsData.Contests.AllVisibleInCategory(category.Id).ToList();

                foreach (var contest in contestsInCurrentCategory)
                {
                    Assert.AreEqual(false, contest.IsDeleted);
                }
            }
        }
    }
}
