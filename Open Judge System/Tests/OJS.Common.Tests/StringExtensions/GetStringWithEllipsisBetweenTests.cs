namespace OJS.Common.Tests.StringExtensions
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Common.Extensions;

    [TestClass]
    public class GetStringWithEllipsisBetweenTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), AllowDerivedTypes = true)]
        public void ShouldThrowExceptionWhenValueIsNotNullAndStartIndexIsNegative()
        {
            const string Value = "vladislav";
            const int StartIndex = -2;
            const int EndIndex = 2;

            var result = Value.GetStringWithEllipsisBetween(StartIndex, EndIndex);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), AllowDerivedTypes = true)]
        public void ShouldThrowExceptionWhenValueIsNotNullAndStartIndexIsEqualToStringLength()
        {
            const string Value = "vladislav";
            var startIndex = Value.Length;
            const int EndIndex = 2;

            var result = Value.GetStringWithEllipsisBetween(startIndex, EndIndex);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), AllowDerivedTypes = true)]
        public void ShouldThrowExceptionWhenValueIsNotNullAndStartIndexIsGreaterThanStringLength()
        {
            const string Value = "vladislav";
            var startIndex = Value.Length + 1;
            const int EndIndex = 2;

            var result = Value.GetStringWithEllipsisBetween(startIndex, EndIndex);
        }

        [TestMethod]
        public void ShouldReturnNullWhenValueIsNullAndStartAndEndIndicesAreValid()
        {
            const string Value = null;
            const int StartIndex = 1;
            const int EndIndex = 2;

            var result = Value.GetStringWithEllipsisBetween(StartIndex, EndIndex);

            Assert.IsNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), AllowDerivedTypes = true)]
        public void ShouldThrowExceptionWhenValueIsNotNullAndStartIndexIsGreaterThanEndIndex()
        {
            const string Value = "vladislav";
            const int StartIndex = 5;
            const int EndIndex = 2;

            var result = Value.GetStringWithEllipsisBetween(StartIndex, EndIndex);
        }

        [TestMethod]
        public void ShouldReturnEmptyStringWhenValueIsNotNullAndStartIndexIsEqualToValueLength()
        {
            const string Value = "vladislav";
            var startIndex = Value.Length;
            var endIndex = Value.Length;

            var result = Value.GetStringWithEllipsisBetween(startIndex, endIndex);

            Assert.AreEqual(string.Empty, result);
        }
        
        [TestMethod]
        public void ShouldReturnEmptyStringWhenValueIsNotNullAndStartIndexIsEqualToEndIndex()
        {
            const string Value = "vladislav";
            const int StartIndex = 2;
            const int EndIndex = 2;

            var result = Value.GetStringWithEllipsisBetween(StartIndex, EndIndex);

            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void ShouldReturnNullWhenValueIsNullAndStartIndexIsEqualToEndIndex()
        {
            const string Value = null;
            const int StartIndex = 2;
            const int EndIndex = 2;

            var result = Value.GetStringWithEllipsisBetween(StartIndex, EndIndex);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void ShouldReturnValueWhenValueIsNotNullAndStartIndexIsEqualToZeroAndEndIndexIsEqualToStringLength()
        {
            const string Value = "vladislav";
            const int StartIndex = 0;
            var endIndex = Value.Length;

            var result = Value.GetStringWithEllipsisBetween(StartIndex, endIndex);

            Assert.AreEqual(Value, result);
        }

        [TestMethod]
        public void ShouldReturnCorrectSubstringWithoutEllipsisWhenValueIsNotNullAndStartIndexIsLessThanEllipsisLength()
        {
            const string Value = "vladislav";
            const int StartIndex = 2;
            var endIndex = Value.Length;

            var result = Value.GetStringWithEllipsisBetween(StartIndex, endIndex);

            Assert.AreEqual("adislav", result);
        }

        [TestMethod]
        public void ShouldReturnCorrectSubstringWithoutEllipsisWhenValueIsNotNullAndStartIndexIsEqualToEllipsisLength()
        {
            const string Value = "vladislav";
            const int StartIndex = 3;
            var endIndex = Value.Length;

            var result = Value.GetStringWithEllipsisBetween(StartIndex, endIndex);

            Assert.AreEqual("dislav", result);
        }

        [TestMethod]
        public void ShouldReturnCorrectSubstringWithEllipsisWhenValueIsNotNullAndStartIndexIsGreaterThanEllipsisLength()
        {
            const string Value = "vladislav";
            const int StartIndex = 5;
            var endIndex = Value.Length;

            var result = Value.GetStringWithEllipsisBetween(StartIndex, endIndex);

            Assert.AreEqual("...slav", result);
        }

        [TestMethod]
        public void ShouldReturnCorrectSubstringWithoutEllipsisWhenValueIsNotNullAndEndIndexIsLessThanLengthMinusEllipsisLength()
        {
            const string Value = "vladislav";
            const int StartIndex = 0;
            var endIndex = Value.Length - 2;

            var result = Value.GetStringWithEllipsisBetween(StartIndex, endIndex);

            Assert.AreEqual("vladisl", result);
        }

        [TestMethod]
        public void ShouldReturnCorrectSubstringWithoutEllipsisWhenValueIsNotNullAndEndIndexIsEqualToLengthMinusEllipsisLength()
        {
            const string Value = "vladislav";
            const int StartIndex = 0;
            var endIndex = Value.Length - 3;

            var result = Value.GetStringWithEllipsisBetween(StartIndex, endIndex);

            Assert.AreEqual("vladis", result);
        }

        [TestMethod]
        public void ShouldReturnCorrectSubstringWithEllipsisWhenValueIsNotNullAndEndIndexIsGreaterThanLengthMinusEllipsisLength()
        {
            const string Value = "vladislav";
            const int StartIndex = 0;
            var endIndex = Value.Length - 4;

            var result = Value.GetStringWithEllipsisBetween(StartIndex, endIndex);

            Assert.AreEqual("vladi...", result);
        }

        [TestMethod]
        public void ShouldReturnCorrectSubstringWithEllipsisOnBothSidesWhenStartIndexAndEndIndexAreAppropriate()
        {
            const string Value = "vladislav";
            const int StartIndex = 4;
            var endIndex = Value.Length - 4;

            var result = Value.GetStringWithEllipsisBetween(StartIndex, endIndex);

            Assert.AreEqual("...i...", result);
        }

        [TestMethod]
        public void ShouldReturnEmptyStringWhenStartIndexWhenStartIndexAndEndIndexAreEqualAndAppropriate()
        {
            const string Value = "vladislav";
            const int StartIndex = 4;
            const int EndIndex = StartIndex;

            var result = Value.GetStringWithEllipsisBetween(StartIndex, EndIndex);

            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void ShouldReturnEmptyStringWhenValueIsEmptyString()
        {
            var value = string.Empty;
            const int StartIndex = 0;
            const int EndIndex = 0;

            var result = value.GetStringWithEllipsisBetween(StartIndex, EndIndex);

            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void ShouldReturnSingleWhitespaceStringWhenValueIsSingleWhitespace()
        {
            const string Value = " ";
            const int StartIndex = 0;
            const int EndIndex = 1;

            var result = Value.GetStringWithEllipsisBetween(StartIndex, EndIndex);

            Assert.AreEqual(Value, result);
        }
    }
}
