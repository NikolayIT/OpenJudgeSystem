namespace OJS.Common.Extensions
{
    using System;
    using System.IO;
    using System.Threading;

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
                var path = Path.Combine(GlobalConstants.ExecutionStrategiesWorkingDirectoryPath, randomDirectoryName);
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

        public static void DeleteExecutionStrategiesWorkingDirectories()
        {
            var directoryPaths = Directory.GetDirectories(GlobalConstants.ExecutionStrategiesWorkingDirectoryPath);
            foreach (var dirPath in directoryPaths)
            {
                var dir = new DirectoryInfo(dirPath);
                if (dir.Exists && dir.CreationTime < DateTime.Now.AddHours(-1))
                {
                    var isDeleted = false;
                    var retryCount = 0;
                    while (!isDeleted && retryCount <= 3)
                    {
                        try
                        {
                            SafeDeleteDirectory(dirPath, true);
                            isDeleted = true;
                        }
                        catch
                        {
                            Thread.Sleep(1000);
                            retryCount++;
                        }
                    }
                }
            }
        }
    }
}