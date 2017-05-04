namespace OJS.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Ionic.Zip;

    // TODO: Unit test
    public static class FileHelpers
    {
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

        public static void ConvertContentToZip(string submissionZipFilePath)
        {
            using (var zipFile = new ZipFile(submissionZipFilePath))
            {
                zipFile.Save();
            }
        }

        public static void UnzipFile(string fileToUnzip, string outputDirectory)
        {
            using (var zipFile = ZipFile.Read(fileToUnzip))
            {
                foreach (var entry in zipFile)
                {
                    entry.Extract(outputDirectory, ExtractExistingFileAction.OverwriteSilently);
                }
            }
        }

        public static string FindFileMatchingPattern(string workingDirectory, string pattern)
        {
            var files = DiscoverAllFilesMatchingPattern(workingDirectory, pattern);

            string discoveredFile = files.First();

            return ProcessModulePath(discoveredFile);
        }

        public static string FindFileMatchingPattern<TOut>(
            string workingDirectory,
            string pattern,
            Func<string, TOut> orderBy)
        {
            var files = DiscoverAllFilesMatchingPattern(workingDirectory, pattern);

            string discoveredFile = files.OrderBy(orderBy).First();

            return ProcessModulePath(discoveredFile);
        }

        public static void AddFilesToZipArchive(string archivePath, string pathInArchive, params string[] filePaths)
        {
            using (var zipFile = new ZipFile(archivePath))
            {
                zipFile.UpdateFiles(filePaths, pathInArchive);
                zipFile.Save();
            }
        }

        public static IEnumerable<string> GetFilePathsFromZip(string archivePath)
        {
            using (ZipFile file = new ZipFile(archivePath))
            {
                return file.EntryFileNames;
            }
        }

        public static void DeleteFiles(params string[] filePaths)
        {
            foreach (var filePath in filePaths)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        public static string ProcessModulePath(string path) => path.Replace('\\', '/');

        private static List<string> DiscoverAllFilesMatchingPattern(string workingDirectory, string pattern)
        {
            var files = new List<string>(
                Directory.GetFiles(
                    workingDirectory,
                    pattern,
                    SearchOption.AllDirectories));
            if (files.Count == 0)
            {
                throw new ArgumentException(
                    $"'{pattern}' file not found in output directory!",
                    nameof(pattern));
            }

            return files;
        }
    }
}
