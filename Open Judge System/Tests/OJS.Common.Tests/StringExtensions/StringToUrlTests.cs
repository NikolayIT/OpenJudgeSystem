namespace OJS.Common.Tests.StringExtensions
{
    using NUnit.Framework;

    using OJS.Common.Extensions;

    [TestFixture]
    public class StringToUrlTests
    {
        [Test]
        public void ToUrlMethodShouldReturnProperCSharpText()
        {
            var original = "SomeUrlWithC#InIt";

            var result = original.ToUrl();
            var expected = "SomeUrlWithCSharpInIt";

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ToUrlMethodShouldReturnProperCPlusPlusText()
        {
            var original = "SomeUrlWithC++InIt";

            var result = original.ToUrl();
            var expected = "SomeUrlWithCPlusPlusInIt";

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ToUrlMethodShouldConvertUglySymbolsToDashInMiddle()
        {
            var original = "Some%Url&With!Ugly)Symbol";

            var result = original.ToUrl();
            var expected = "Some-Url-With-Ugly-Symbol";

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ToUrlMethodShouldConvertUglySymbolsToDashInMiddleWithRepeatitions()
        {
            var original = "Some%$Url&!With!^^^Ugly**)Symbol";

            var result = original.ToUrl();
            var expected = "Some-Url-With-Ugly-Symbol";

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ToUrlMethodShouldRemoveUglySymbolsInTheStartOfString()
        {
            var original = "###Some%$Url&!With!^^^Ugly**)Symbol";

            var result = original.ToUrl();
            var expected = "Some-Url-With-Ugly-Symbol";

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ToUrlMethodShouldConvertUglySymbolsToDashInTheEndOfString()
        {
            var original = "Some%$Url&!With!^^^Ugly**)Symbol*&*";

            var result = original.ToUrl();
            var expected = "Some-Url-With-Ugly-Symbol";

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ToUrlMethodShouldConvertSpacesToDash()
        {
            var original = "  Some  Url  With  Ugly  Symbol  ";

            var result = original.ToUrl();
            var expected = "Some-Url-With-Ugly-Symbol";

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ToUrlMethodShouldConvertCSharpAndCPlusPlus()
        {
            var original = "  Some  C++UrlC++  With  UglyC#C#  Symbol  ";

            var result = original.ToUrl();
            var expected = "Some-CPlusPlusUrlCPlusPlus-With-UglyCSharpCSharp-Symbol";

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ToUrlMethodShouldRemoveUglySymbolsAtBeginningAndEnd()
        {
            var original = "% Some  C++UrlC++  With  UglyC#C#  Symbol #";

            var result = original.ToUrl();
            var expected = "Some-CPlusPlusUrlCPlusPlus-With-UglyCSharpCSharp-Symbol";

            Assert.AreEqual(expected, result);
        }
    }
}
