namespace OJS.Common.Tests.StringExtensions
{
    using NUnit.Framework;

    using OJS.Workers.Common.Extensions;

    [TestFixture]
    public class GetFirstDifferenceIndexWithTests
    {
        [Test]
        public void ShouldReturnMinusOneWhenBothStringsAreNull()
        {
            const string First = null;
            const string Second = null;

            var firstDifferenceIndex = First.GetFirstDifferenceIndexWith(Second);

            Assert.AreEqual(-1, firstDifferenceIndex);
        }

        [Test]
        public void ShouldReturnZeroWhenParameterStringIsNull()
        {
            const string First = "test";
            const string Second = null;

            var firstDifferenceIndex = First.GetFirstDifferenceIndexWith(Second);

            Assert.AreEqual(0, firstDifferenceIndex);
        }

        [Test]
        public void ShouldReturnZeroWhenInstanceStringIsNull()
        {
            const string First = null;
            const string Second = "test";

            var firstDifferenceIndex = First.GetFirstDifferenceIndexWith(Second);

            Assert.AreEqual(0, firstDifferenceIndex);
        }

        [Test]
        public void ShouldReturnZeroWhenStringsHaveDifferentFirstLetter()
        {
            const string First = "string";
            const string Second = "test";

            var firstDifferenceIndex = First.GetFirstDifferenceIndexWith(Second);

            Assert.AreEqual(0, firstDifferenceIndex);
        }

        [Test]
        public void ShouldReturnCorrectIndexWhenStringsAreDifferent()
        {
            const string First = "testing string";
            const string Second = "ten strings";

            var firstDifferenceIndex = First.GetFirstDifferenceIndexWith(Second);

            Assert.AreEqual(2, firstDifferenceIndex);
        }

        [Test]
        public void ShouldReturnMinusOneWhenStringsAreDifferentAndIgnoresCase()
        {
            const string First = "testing string";
            const string Second = "teStInG strING";

            var firstDifferenceIndex = First.GetFirstDifferenceIndexWith(Second, true);

            Assert.AreEqual(-1, firstDifferenceIndex);
        }

        [Test]
        public void ShouldReturnMinusOneWhenStringsAreEqual()
        {
            const string First = "testing string";
            const string Second = "testing string";

            var firstDifferenceIndex = First.GetFirstDifferenceIndexWith(Second);

            Assert.AreEqual(-1, firstDifferenceIndex);
        }

        [Test]
        public void ShouldReturnCorrectIndexWhenFirstInstanceStringIsLongerThanParameterString()
        {
            const string First = "testing string and more";
            const string Second = "testing string";

            var firstDifferenceIndex = First.GetFirstDifferenceIndexWith(Second);

            Assert.AreEqual(Second.Length, firstDifferenceIndex);
        }

        [Test]
        public void ShouldReturnCorrectIndexWhenParameterStringIsLongerThanInstanceString()
        {
            const string First = "testing string";
            const string Second = "testing string and more";

            var firstDifferenceIndex = First.GetFirstDifferenceIndexWith(Second);

            Assert.AreEqual(First.Length, firstDifferenceIndex);
        }

        [Test]
        public void ShouldReturnMinusOneWhenBothStringsAreNullAndIgnoresCase()
        {
            const string First = null;
            const string Second = null;

            var firstDifferenceIndex = First.GetFirstDifferenceIndexWith(Second, true);

            Assert.AreEqual(-1, firstDifferenceIndex);
        }

        [Test]
        public void ShouldReturnCorrectIndexWhenStringsAreDifferentAndIgnoresCase()
        {
            const string First = "Testing String";
            const string Second = "teStihg string";

            var firstDifferenceIndex = First.GetFirstDifferenceIndexWith(Second, true);

            Assert.AreEqual(5, firstDifferenceIndex);
        }
    }
}