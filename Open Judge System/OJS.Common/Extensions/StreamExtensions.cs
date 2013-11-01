namespace OJS.Common.Extensions
{
    using System.IO;

    public static class FileExtensions
    {
        public static byte[] ToByteArray(this Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public static Stream ToStream(this byte[] input)
        {
            var fileStream = new MemoryStream(input);
            fileStream.Position = 0;

            return fileStream;
        }
    }
}
