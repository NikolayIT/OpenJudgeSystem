namespace OJS.Common.Tests.StringExtensions
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OJS.Common.Extensions;

    [TestClass]
    public class GetStringBetweenTests
    {
        [TestMethod]
        public void GetStringBetweenShouldReturnProperValueWhenCalledWithSingleCharacters()
        {
            const string Value = "Test №10 execution successful!";
            const string Expected = "10";
            var actual = Value.GetStringBetween("№", " ");
            Assert.AreEqual(Expected, actual);
        }
    }
}
