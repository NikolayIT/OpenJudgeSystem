namespace OJS.Workers.Common.Extensions
{
    using System.IO;
    using System.IO.Compression;
    using System.Text;

    public static class CompressStringExtensions
    {
        public static byte[] Compress(this string stringToCompress)
        {
            if (stringToCompress == null)
            {
                return null;
            }

            var bytes = Encoding.UTF8.GetBytes(stringToCompress);

            using (var memoryStreamInput = new MemoryStream(bytes))
            {
                using (var memoryStreamOutput = new MemoryStream())
                {
                    using (var deflateStream = new DeflateStream(memoryStreamOutput, CompressionMode.Compress))
                    {
                        memoryStreamInput.CopyTo(deflateStream);
                    }

                    return memoryStreamOutput.ToArray();
                }
            }
        }

        public static string Decompress(this byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }

            using (var memoryStreamInput = new MemoryStream(bytes))
            {
                using (var memoryStreamOutput = new MemoryStream())
                {
                    using (var deflateStream = new DeflateStream(memoryStreamInput, CompressionMode.Decompress))
                    {
                        deflateStream.CopyTo(memoryStreamOutput);
                    }

                    return Encoding.UTF8.GetString(memoryStreamOutput.ToArray());
                }
            }
        }
    }
}