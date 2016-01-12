namespace OJS.Common.Tests.StringExtensions
{
    using NUnit.Framework;

    using OJS.Common.Extensions;

    [TestFixture]
    public class ToIntegerTests
    {
        [Test]
        public void ZeroStringShouldReturnZero()
        {
            const int Expected = 0;
            const string Input = "0";
            int actual = Input.ToInteger();
            Assert.AreEqual(Expected, actual);
        }

        [Test]
        public void InvalidInputShouldReturnZero()
        {
            const int Expected = 0;
            const string Input = "invalid";
            int actual = Input.ToInteger();
            Assert.AreEqual(Expected, actual);
        }

        [Test]
        public void ValidInputShouldReturnSameValue()
        {
            const int Expected = 1234567890;
            const string Input = "1234567890";
            int actual = Input.ToInteger();
            Assert.AreEqual(Expected, actual);
        }
    }
}
