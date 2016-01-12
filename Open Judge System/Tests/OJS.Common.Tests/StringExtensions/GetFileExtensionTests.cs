namespace OJS.Common.Tests.StringExtensions
{
    using NUnit.Framework;

    using OJS.Common.Extensions;

    [TestFixture]
    public class GetFileExtensionTests
    {
        [Test]
        public void GetFileExtensionShouldReturnEmptyStringWhenEmptyStringIsPassed()
        {
            string expected = string.Empty;
            string value = string.Empty;
            string actual = value.GetFileExtension();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GetFileExtensionShouldReturnEmptyStringWhenNullIsPassed()
        {
            string expected = string.Empty;
            string actual = ((string)null).GetFileExtension();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GetFileExtensionShouldReturnJpgWhenValidImageIsPassed()
        {
            string expected = "jpg";
            string value = "pic.jpg";
            string actual = value.GetFileExtension();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GetFileExtensionShouldReturnPngWhenValidImageWithManyDotsIsPassed()
        {
            string expected = "png";
            string value = "pic.test.value.jpg.png";
            string actual = value.GetFileExtension();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GetFileExtensionShouldReturnEmptyStringWhenFileDoesNotHaveExtension()
        {
            string expected = string.Empty;
            string value = "testing";
            string actual = value.GetFileExtension();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GetFileExtensionShouldReturnEmptyStringWhenFileEndsInADot()
        {
            string expected = string.Empty;
            string value = "testing.";
            string actual = value.GetFileExtension();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GetFileExtensionShouldReturnEmptyStringWhenFileContainsManyDotsAndEndsInADot()
        {
            string expected = string.Empty;
            string value = "testing.jpg.value.gosho.";
            string actual = value.GetFileExtension();
            Assert.AreEqual(expected, actual);
        }
    }
}
