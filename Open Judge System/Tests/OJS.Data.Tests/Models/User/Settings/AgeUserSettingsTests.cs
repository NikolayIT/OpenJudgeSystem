namespace OJS.Data.Tests.Models.User.Settings
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OJS.Data.Models;

    [TestClass]
    public class AgeUserSettingsTests
    {
        [TestMethod]
        public void AgeShouldReturnNullIfDateOfBirthHasNotValue()
        {
            UserSettings settings = new UserSettings();

            var result = settings.Age;

            Assert.IsNull(result);
        }

        [TestMethod]
        public void AgeShouldReturnZeroYearsIfUserIsMonthsOld()
        {
            UserSettings settings = new UserSettings()
            {
                DateOfBirth = DateTime.Now.AddMonths(-2)
            };

            var result = settings.Age;
            byte expected = 0;

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void AgeShouldReturnZeroYearsIfUserIsDaysOld()
        {
            UserSettings settings = new UserSettings()
            {
                DateOfBirth = DateTime.Now.AddDays(-2)
            };

            var result = settings.Age;
            byte expected = 0;

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void AgeShouldReturnProperAgeIfCurrentYearBirthMonthHasPassed()
        {
            UserSettings settings = new UserSettings()
            {
                DateOfBirth = DateTime.Now.AddYears(-10).AddMonths(-1)
            };

            var result = settings.Age;
            byte expected = 10;

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void AgeShouldReturnProperAgeIfCurrentYearBirthMonthHasNotPassed()
        {
            UserSettings settings = new UserSettings()
            {
                DateOfBirth = DateTime.Now.AddYears(-10).AddMonths(1)
            };

            var result = settings.Age;
            byte expected = 9;

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void AgeShouldReturnProperAgeIfCurrentYearBirthMonthIsTheSameAndDayHasPassed()
        {
            UserSettings settings = new UserSettings()
            {
                DateOfBirth = DateTime.Now.AddYears(-10).AddDays(-1)
            };

            var result = settings.Age;
            byte expected = 10;

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void AgeShouldReturnProperAgeIfCurrentYearBirthMonthIsTheSameAndDayHasNotPassed()
        {
            UserSettings settings = new UserSettings()
            {
                DateOfBirth = DateTime.Now.AddYears(-10).AddDays(1)
            };

            var result = settings.Age;
            byte expected = 9;

            Assert.AreEqual(expected, result);
        }
    }
}
