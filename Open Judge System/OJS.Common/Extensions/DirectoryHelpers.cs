namespace OJS.Common.Extensions
{
    using System;
    using System.IO;
    using System.Threading;

    using MissingFeatures;

    public static class DirectoryHelpers
    {
        private const int ThreadSleepMilliseconds = 1000;

        public static void CreateDirecory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

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
            var isDirectoryCreated = false;
            var path = string.Empty;
            while (!isDirectoryCreated)
            {
                var randomDirectoryName = Path.GetRandomFileName();
                path = Path.Combine(GlobalConstants.ExecutionStrategiesWorkingDirectoryPath, randomDirectoryName);
                if (Directory.Exists(path))
                {
                    continue;
                }

                Directory.CreateDirectory(path);
                isDirectoryCreated = true;
            }

            return path;
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

        public static void DeleteExecutionStrategyWorkingDirectories()
        {
            var directoryPaths = Directory.GetDirectories(GlobalConstants.ExecutionStrategiesWorkingDirectoryPath);
            foreach (var directoryPath in directoryPaths)
            {
                var directory = new DirectoryInfo(directoryPath);
                if (directory.Exists && directory.CreationTime < DateTime.Now.AddHours(-1))
                {
                    var isDeleted = false;
                    var retryCount = 0;
                    while (!isDeleted && retryCount <= 3)
                    {
                        try
                        {
                            SafeDeleteDirectory(directoryPath, true);
                            isDeleted = true;
                        }
                        catch
                        {
                            Thread.Sleep(ThreadSleepMilliseconds);
                            retryCount++;
                        }
                    }
                }
            }
        }
    }
}