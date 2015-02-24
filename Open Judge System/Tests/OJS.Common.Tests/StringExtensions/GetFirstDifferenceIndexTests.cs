namespace OJS.Common.Tests.StringExtensions
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Common.Extensions;

    [TestClass]
    public class GetFirstDifferenceIndexTests
    {
        [TestMethod]
        public void ShouldReturnMinusOneWhenBothStringsAreNull()
        {
            const string First = null;
            const string Second = null;

            var firstDifferenceIndex = First.GetFirstDifferenceIndexWith(Second);

            Assert.AreEqual(-1, firstDifferenceIndex);
        }

        [TestMethod]
        public void ShouldReturnZeroWhenParameterStringIsNull()
        {
            const string First = "test";
            const string Second = null;

            var firstDifferenceIndex = First.GetFirstDifferenceIndexWith(Second);

            Assert.AreEqual(0, firstDifferenceIndex);
        }

        [TestMethod]
        public void ShouldReturnZeroWhenInstanceStringIsNull()
        {
            const string First = null;
            const string Second = "test";

            var firstDifferenceIndex = First.GetFirstDifferenceIndexWith(Second);

            Assert.AreEqual(0, firstDifferenceIndex);
        }

        [TestMethod]
        public void ShouldReturnZeroWhenStringsHaveDifferentFirstLetter()
        {
            const string First = "string";
            const string Second = "test";

            var firstDifferenceIndex = First.GetFirstDifferenceIndexWith(Second);

            Assert.AreEqual(0, firstDifferenceIndex);
        }

        [TestMethod]
        public void ShouldReturnCorrectIndexWhenStringsAreDifferent()
        {
            const string First = "testing string";
            const string Second = "ten strings";

            var firstDifferenceIndex = First.GetFirstDifferenceIndexWith(Second);

            Assert.AreEqual(2, firstDifferenceIndex);
        }

        [TestMethod]
        public void ShouldReturnMinusOneWhenStringsAreDifferentAndIgnoresCase()
        {
            const string First = "testing string";
            const string Second = "teStInG strING";

            var firstDifferenceIndex = First.GetFirstDifferenceIndexWith(Second, true);

            Assert.AreEqual(-1, firstDifferenceIndex);
        }

        [TestMethod]
        public void ShouldReturnMinusOneWhenStringsAreEqual()
        {
            const string First = "testing string";
            const string Second = "testing string";

            var firstDifferenceIndex = First.GetFirstDifferenceIndexWith(Second);

            Assert.AreEqual(-1, firstDifferenceIndex);
        }

        [TestMethod]
        public void ShouldReturnCorrectIndexWhenFirstInstanceStringIsLongerThanParameterString()
        {
            const string First = "testing string and more";
            const string Second = "testing string";

            var firstDifferenceIndex = First.GetFirstDifferenceIndexWith(Second);

            Assert.AreEqual(Second.Length, firstDifferenceIndex);
        }

        [TestMethod]
        public void ShouldReturnCorrectIndexWhenParameterStringIsLongerThanInstanceString()
        {
            const string First = "testing string";
            const string Second = "testing string and more";

            var firstDifferenceIndex = First.GetFirstDifferenceIndexWith(Second);

            Assert.AreEqual(First.Length, firstDifferenceIndex);
        }

        [TestMethod]
        public void ShouldReturnMinusOneWhenBothStringsAreNullAndIgnoresCase()
        {
            const string First = null;
            const string Second = null;

            var firstDifferenceIndex = First.GetFirstDifferenceIndexWith(Second, true);

            Assert.AreEqual(-1, firstDifferenceIndex);
        }

        [TestMethod]
        public void ShouldReturnCorrectIndexWhenStringsAreDifferentAndIgnoresCase()
        {
            const string First = "Testing String";
            const string Second = "teStihg string";

            var firstDifferenceIndex = First.GetFirstDifferenceIndexWith(Second, true);

            Assert.AreEqual(5, firstDifferenceIndex);
        }
    }
}
