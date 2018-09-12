namespace OJS.Common.Tests.StringExtensions
{
    using System;

    using NUnit.Framework;

    using OJS.Workers.Common.Extensions;

    [TestFixture]
    public class GetStringWithEllipsisBetweenTests
    {
        [Test]
        public void ShouldThrowExceptionWhenValueIsNotNullAndStartIndexIsNegative()
        {
            const string Value = "vladislav";
            const int StartIndex = -2;
            const int EndIndex = 2;

            Assert.Throws(
                Is.InstanceOf<ArgumentException>(),
                () => { var result = Value.GetStringWithEllipsisBetween(StartIndex, EndIndex); });
        }

        [Test]
        public void ShouldThrowExceptionWhenValueIsNotNullAndStartIndexIsEqualToStringLength()
        {
            const string Value = "vladislav";
            var startIndex = Value.Length;
            const int EndIndex = 2;

            Assert.Throws(
                Is.InstanceOf<ArgumentException>(),
                () => { var result = Value.GetStringWithEllipsisBetween(startIndex, EndIndex); });
        }

        [Test]
        public void ShouldThrowExceptionWhenValueIsNotNullAndStartIndexIsGreaterThanStringLength()
        {
            const string Value = "vladislav";
            var startIndex = Value.Length + 1;
            const int EndIndex = 2;

            Assert.Throws(
                Is.InstanceOf<ArgumentException>(),
                () => { var result = Value.GetStringWithEllipsisBetween(startIndex, EndIndex); });
        }

        [Test]
        public void ShouldReturnNullWhenValueIsNullAndStartAndEndIndicesAreValid()
        {
            const string Value = null;
            const int StartIndex = 1;
            const int EndIndex = 2;

            var result = Value.GetStringWithEllipsisBetween(StartIndex, EndIndex);

            Assert.IsNull(result);
        }

        [Test]
        public void ShouldThrowExceptionWhenValueIsNotNullAndStartIndexIsGreaterThanEndIndex()
        {
            const string Value = "vladislav";
            const int StartIndex = 5;
            const int EndIndex = 2;

            Assert.Throws(
                Is.InstanceOf<ArgumentException>(),
                () => { var result = Value.GetStringWithEllipsisBetween(StartIndex, EndIndex); });
        }

        [Test]
        public void ShouldReturnEmptyStringWhenValueIsNotNullAndStartIndexIsEqualToValueLength()
        {
            const string Value = "vladislav";
            var startIndex = Value.Length;
            var endIndex = Value.Length;

            var result = Value.GetStringWithEllipsisBetween(startIndex, endIndex);

            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void ShouldReturnEmptyStringWhenValueIsNotNullAndStartIndexIsEqualToEndIndex()
        {
            const string Value = "vladislav";
            const int StartIndex = 2;
            const int EndIndex = 2;

            var result = Value.GetStringWithEllipsisBetween(StartIndex, EndIndex);

            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void ShouldReturnNullWhenValueIsNullAndStartIndexIsEqualToEndIndex()
        {
            const string Value = null;
            const int StartIndex = 2;
            const int EndIndex = 2;

            var result = Value.GetStringWithEllipsisBetween(StartIndex, EndIndex);

            Assert.IsNull(result);
        }

        [Test]
        public void ShouldReturnValueWhenValueIsNotNullAndStartIndexIsEqualToZeroAndEndIndexIsEqualToStringLength()
        {
            const string Value = "vladislav";
            const int StartIndex = 0;
            var endIndex = Value.Length;

            var result = Value.GetStringWithEllipsisBetween(StartIndex, endIndex);

            Assert.AreEqual(Value, result);
        }

        [Test]
        public void ShouldReturnCorrectSubstringWithoutEllipsisWhenValueIsNotNullAndStartIndexIsLessThanEllipsisLength()
        {
            const string Value = "vladislav";
            const int StartIndex = 2;
            var endIndex = Value.Length;

            var result = Value.GetStringWithEllipsisBetween(StartIndex, endIndex);

            Assert.AreEqual("adislav", result);
        }

        [Test]
        public void ShouldReturnCorrectSubstringWithoutEllipsisWhenValueIsNotNullAndStartIndexIsEqualToEllipsisLength()
        {
            const string Value = "vladislav";
            const int StartIndex = 3;
            var endIndex = Value.Length;

            var result = Value.GetStringWithEllipsisBetween(StartIndex, endIndex);

            Assert.AreEqual("dislav", result);
        }

        [Test]
        public void ShouldReturnCorrectSubstringWithEllipsisWhenValueIsNotNullAndStartIndexIsGreaterThanEllipsisLength()
        {
            const string Value = "vladislav";
            const int StartIndex = 5;
            var endIndex = Value.Length;

            var result = Value.GetStringWithEllipsisBetween(StartIndex, endIndex);

            Assert.AreEqual("...slav", result);
        }

        [Test]
        public void ShouldReturnCorrectSubstringWithoutEllipsisWhenValueIsNotNullAndEndIndexIsLessThanLengthMinusEllipsisLength()
        {
            const string Value = "vladislav";
            const int StartIndex = 0;
            var endIndex = Value.Length - 2;

            var result = Value.GetStringWithEllipsisBetween(StartIndex, endIndex);

            Assert.AreEqual("vladisl", result);
        }

        [Test]
        public void ShouldReturnCorrectSubstringWithoutEllipsisWhenValueIsNotNullAndEndIndexIsEqualToLengthMinusEllipsisLength()
        {
            const string Value = "vladislav";
            const int StartIndex = 0;
            var endIndex = Value.Length - 3;

            var result = Value.GetStringWithEllipsisBetween(StartIndex, endIndex);

            Assert.AreEqual("vladis", result);
        }

        [Test]
        public void ShouldReturnCorrectSubstringWithEllipsisWhenValueIsNotNullAndEndIndexIsGreaterThanLengthMinusEllipsisLength()
        {
            const string Value = "vladislav";
            const int StartIndex = 0;
            var endIndex = Value.Length - 4;

            var result = Value.GetStringWithEllipsisBetween(StartIndex, endIndex);

            Assert.AreEqual("vladi...", result);
        }

        [Test]
        public void ShouldReturnCorrectSubstringWithEllipsisOnBothSidesWhenStartIndexAndEndIndexAreAppropriate()
        {
            const string Value = "vladislav";
            const int StartIndex = 4;
            var endIndex = Value.Length - 4;

            var result = Value.GetStringWithEllipsisBetween(StartIndex, endIndex);

            Assert.AreEqual("...i...", result);
        }

        [Test]
        public void ShouldReturnEmptyStringWhenStartIndexWhenStartIndexAndEndIndexAreEqualAndAppropriate()
        {
            const string Value = "vladislav";
            const int StartIndex = 4;
            const int EndIndex = StartIndex;

            var result = Value.GetStringWithEllipsisBetween(StartIndex, EndIndex);

            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void ShouldReturnEmptyStringWhenValueIsEmptyString()
        {
            var value = string.Empty;
            const int StartIndex = 0;
            const int EndIndex = 0;

            var result = value.GetStringWithEllipsisBetween(StartIndex, EndIndex);

            Assert.AreEqual(value, result);
        }

        [Test]
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