namespace OJS.Common.Tests.StringExtensions
{
    using NUnit.Framework;

    using OJS.Common.Extensions;

    [TestFixture]
    public class GetStringBetweenTests
    {
        [Test]
        public void GetStringBetweenShouldReturnProperValueWhenCalledWithSingleCharacters()
        {
            const string Value = "Test №10 execution successful!";
            const string Expected = "10";
            var actual = Value.GetStringBetween("№", " ");
            Assert.AreEqual(Expected, actual);
        }

        [Test]
        public void GetStringBetweenShouldReturnProperValueWhenCalledWithMultilineText()
        {
            const string Value = @"Answer incorrect!
Expected output:
1

Your output:
2

Time used (in milliseconds): 21.4844
Memory used (in bytes): 1024000";
            const string Expected = @"
Expected output:
1

Your output:
2

";
            var actual = Value.GetStringBetween("Answer incorrect!", "Time used");
            Assert.AreEqual(Expected, actual);
        }

        [Test]
        public void GetStringBetweenShouldReturnProperValueWhenCalledWithNewLineAsSecondArgument()
        {
            const string Value = @"Answer correct!!!
Time used (in milliseconds): 26.3672
Memory used (in bytes): 94208
";
            const string Expected = "94208";
            var actual = Value.GetStringBetween("Memory used (in bytes): ", "\r\n");
            Assert.AreEqual(Expected, actual);
        }
    }
}
