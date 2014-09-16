namespace OJS.Common.Extensions
{
    using System.IO;

    public static class StreamExtensions
    {
        public static byte[] ToByteArray(this Stream input)
        {
            var buffer = new byte[16 * 1024];
            using (var memoryStream = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    memoryStream.Write(buffer, 0, read);
                }

                return memoryStream.ToArray();
            }
        }

        public static Stream ToStream(this byte[] input)
        {
            var fileStream = new MemoryStream(input) { Position = 0 };
            return fileStream;
        }
    }
}
