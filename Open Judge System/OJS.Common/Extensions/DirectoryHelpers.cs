namespace OJS.Common.Extensions
{
    using System.IO;

    using MissingFeatures;

    public static class DirectoryHelpers
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

        public static string CreateTempDirectoryForExecutionStrategy()
        {
            while (true)
            {
                var randomDirectoryName = Path.GetRandomFileName();
                var path = Path.Combine(GlobalConstants.ExecutionStrategiesPath, randomDirectoryName);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    return path;
                }
            }
        }

        public static void SafeDeleteDirectory(string path, bool recursive = false)
        {
            if (Directory.Exists(path))
            {
                var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                Directory.EnumerateFileSystemEntries(path, "*", searchOption).ForEach(x => File.SetAttributes(x, FileAttributes.Normal));

                Directory.Delete(path, recursive);
            }
        }
    }
}
