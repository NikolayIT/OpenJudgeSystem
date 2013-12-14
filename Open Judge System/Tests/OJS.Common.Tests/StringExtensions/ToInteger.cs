namespace OJS.Common.Tests.StringExtensions
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Common.Extensions;

    [TestClass]
    public class ToInteger
    {
        [TestMethod]
        public void ZeroStringShouldReturnZero()
        {
            const int Expected = 0;
            const string Input = "0";
            int actual = Input.ToInteger();
            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        public void InvalidInputShouldReturnZero()
        {
            const int Expected = 0;
            const string Input = "invalid";
            int actual = Input.ToInteger();
            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        public void ValidInputShouldReturnSameValue()
        {
            const int Expected = 1234567890;
            const string Input = "1234567890";
            int actual = Input.ToInteger();
            Assert.AreEqual(Expected, actual);
        }
    }
}
