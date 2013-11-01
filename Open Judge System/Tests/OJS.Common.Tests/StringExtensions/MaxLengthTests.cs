namespace OJS.Common.Tests.StringExtensions
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OJS.Common.Extensions;

    [TestClass]
    public class MaxLengthTests
    {
        [TestMethod]
        public void MaxLengthShouldReturnEmptyStringWhenEmptyStringIsPassed()
        {
            string expected = string.Empty;
            string value = string.Empty;
            string actual = value.MaxLength(0);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MaxLengthShouldReturnProperStringWhenLongerStringIsPassed()
        {
            string expected = "12345";
            string value = "1234567890";
            string actual = value.MaxLength(5);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MaxLengthShouldReturnProperStringWhenShorterStringIsPassed()
        {
            string expected = "123";
            string value = "123";
            string actual = value.MaxLength(5);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MaxLengthShouldReturnProperStringWhenStringWithTheSameLengthIsPassed()
        {
            string expected = "123";
            string value = "123";
            string actual = value.MaxLength(3);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MaxLengthShouldReturnProperStringWhenZeroLengthIsPassed()
        {
            string expected = string.Empty;
            string value = "123";
            string actual = value.MaxLength(0);
            Assert.AreEqual(expected, actual);
        }
    }
}
