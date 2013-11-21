namespace OJS.Common.Tests.StringExtensions
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OJS.Common.Extensions;

    [TestClass]
    public class StringToUrlTests
    {
        [TestMethod]
        public void ToUrlMethodShouldReturnProperCSharpText()
        {
            var original = "SomeUrlWithC#InIt";

            var result = StringExtensions.ToUrl(original);
            var expected = "SomeUrlWithCSharpInIt";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ToUrlMethodShouldReturnProperCPlusPlusText()
        {
            var original = "SomeUrlWithC++InIt";

            var result = StringExtensions.ToUrl(original);
            var expected = "SomeUrlWithCPlusPlusInIt";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ToUrlMethodShouldConvertUglySymbolsToDashInMiddle()
        {
            var original = "Some%Url&With!Ugly)Symbol";

            var result = StringExtensions.ToUrl(original);
            var expected = "Some-Url-With-Ugly-Symbol";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ToUrlMethodShouldConvertUglySymbolsToDashInMiddleWithRepeatitions()
        {
            var original = "Some%$Url&!With!^^^Ugly**)Symbol";

            var result = StringExtensions.ToUrl(original);
            var expected = "Some-Url-With-Ugly-Symbol";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ToUrlMethodShouldRemoveUglySymbolsInTheStartOfString()
        {
            var original = "###Some%$Url&!With!^^^Ugly**)Symbol";

            var result = StringExtensions.ToUrl(original);
            var expected = "Some-Url-With-Ugly-Symbol";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ToUrlMethodShouldConvertUglySymbolsToDashInTheEndOfString()
        {
            var original = "Some%$Url&!With!^^^Ugly**)Symbol*&*";

            var result = StringExtensions.ToUrl(original);
            var expected = "Some-Url-With-Ugly-Symbol";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ToUrlMethodShouldConvertSpacesToDash()
        {
            var original = "  Some  Url  With  Ugly  Symbol  ";

            var result = StringExtensions.ToUrl(original);
            var expected = "Some-Url-With-Ugly-Symbol";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ToUrlMethodShouldConvertCSharpAndCPlusPlus()
        {
            var original = "  Some  C++UrlC++  With  UglyC#C#  Symbol  ";

            var result = StringExtensions.ToUrl(original);
            var expected = "Some-CPlusPlusUrlCPlusPlus-With-UglyCSharpCSharp-Symbol";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ToUrlMethodShouldRemoveUglySymbolsAtBeginningAndEnd()
        {
            var original = "% Some  C++UrlC++  With  UglyC#C#  Symbol #";

            var result = StringExtensions.ToUrl(original);
            var expected = "Some-CPlusPlusUrlCPlusPlus-With-UglyCSharpCSharp-Symbol";

            Assert.AreEqual(expected, result);
        }
    }
}
