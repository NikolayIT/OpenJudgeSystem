namespace OJS.Common.Extensions
{
    using System.IO;

    // TODO: Unit test
    public static class FileHelpers
    {
        public static string CreateTempDirectory()
        {
            while (true)
            {
                var randomDirectoryName = Path.GetRandomFileName();
                var path = Path.Combine(Path.GetTempPath(), randomDirectoryName);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    return path;
                }
            }
        }

        public static string SaveStringToTempFile(string stringToWrite)
        {
            var tempFilePath = Path.GetTempFileName();
            File.WriteAllText(tempFilePath, stringToWrite);
            return tempFilePath;
        }

        public static string SaveByteArrayToTempFile(byte[] dataToWrite)
        {
            var tempFilePath = Path.GetTempFileName();
            File.WriteAllBytes(tempFilePath, dataToWrite);
            return tempFilePath;
        }
    }
}
