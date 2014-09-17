namespace OJS.Common.Tests.StringExtensions
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Common.Extensions;

    [TestClass]
    public class PascalCaseToTextTests
    {
        [TestMethod]
        public void FewWordsStringShouldReturnProperResult()
        {
            const string Input = "PascalCaseExample";
            const string Expected = "Pascal case example";
            var result = Input.PascalCaseToText();
            Assert.AreEqual(Expected, result);
        }

        [TestMethod]
        public void OneWordStringShouldReturnProperResult()
        {
            const string Input = "Pascal";
            const string Expected = "Pascal";
            var result = Input.PascalCaseToText();
            Assert.AreEqual(Expected, result);
        }

        [TestMethod]
        public void MethodShouldNotChangeTheOtherPartsOfTheString()
        {
            const string Input = "  PascalCase a A OtherWord Word2 ";
            const string Expected = "  Pascal case a A Other word Word2 ";
            var result = Input.PascalCaseToText();
            Assert.AreEqual(Expected, result);
        }

        [TestMethod]
        public void AbbreviationsShouldBeKept()
        {
            const string Input = "Ivo knows SOLID";
            const string Expected = "Ivo knows SOLID";
            var result = Input.PascalCaseToText();
            Assert.AreEqual(Expected, result);
        }

        [TestMethod]
        public void AbbreviationsShouldBeKeptIfNoOtherWords()
        {
            const string Input = "SOLID";
            const string Expected = "SOLID";
            var result = Input.PascalCaseToText();
            Assert.AreEqual(Expected, result);
        }

        [TestMethod]
        public void NullStringShouldReturnNull()
        {
            const string Input = null;
            const string Expected = null;
            var result = Input.PascalCaseToText();
            Assert.AreEqual(Expected, result);
        }
    }
}
