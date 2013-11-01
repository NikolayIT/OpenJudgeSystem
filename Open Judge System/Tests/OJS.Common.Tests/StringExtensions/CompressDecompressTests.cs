namespace OJS.Common.Tests.StringExtensions
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OJS.Common.Extensions;

    [TestClass]
    public class CompressDecompressTests
    {
        [TestMethod]
        public void DecompressShouldProduceTheOriginallyCompressedString()
        {
            const string InputString = "Николай";
            var compressed = InputString.Compress();
            var decompressed = compressed.Decompress();

            Assert.AreEqual(InputString, decompressed);
        }
    }
}
