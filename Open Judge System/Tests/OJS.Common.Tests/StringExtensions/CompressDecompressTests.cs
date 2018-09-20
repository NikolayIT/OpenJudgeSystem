namespace OJS.Common.Tests.StringExtensions
{
    using NUnit.Framework;

    using OJS.Workers.Common.Extensions;

    [TestFixture]
    public class CompressDecompressTests
    {
        [Test]
        public void DecompressShouldProduceTheOriginallyCompressedString()
        {
            const string InputString = "Николай";
            var compressed = InputString.Compress();
            var decompressed = compressed.Decompress();

            Assert.That(InputString, Is.EqualTo(decompressed));
        }
    }
}
