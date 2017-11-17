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

        public static string SaveStringToTempFile(string directory, string stringToWrite)
        {
            var tempFilePath = Path.GetTempFileName();
            File.Delete(tempFilePath);
            var fullTempFilePath = $"{directory}\\{Path.GetFileName(tempFilePath)}";
            File.WriteAllText(fullTempFilePath, stringToWrite);
            return fullTempFilePath;
        }

        public static string SaveByteArrayToTempFile(byte[] dataToWrite)
        {
            var tempFilePath = Path.GetFileName(Path.GetTempFileName());
            File.WriteAllBytes(tempFilePath, dataToWrite);
            return tempFilePath;
        }

        public static string SaveByteArrayToTempFile(string directory, byte[] dataToWrite)
        {
            var tempFilePath = Path.GetTempFileName();
            File.Delete(tempFilePath);
            var fullTempFilePath = $"{directory}\\{Path.GetFileName(tempFilePath)}";
            File.WriteAllBytes(fullTempFilePath, dataToWrite);
            return fullTempFilePath;
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

            var discoveredFile = files.First();

            return ProcessModulePath(discoveredFile);
        }

        public static string FindFileMatchingPattern<TOut>(
            string workingDirectory,
            string pattern,
            Func<string, TOut> orderBy)
        {
            var files = DiscoverAllFilesMatchingPattern(workingDirectory, pattern);

            var discoveredFile = files.OrderByDescending(orderBy).First();

            return ProcessModulePath(discoveredFile);
        }

        public static string FindFileMatchingPattern<TOut>(
            string workingDirectory,
            string pattern,
            Func<string, bool> where,
            Func<string, TOut> orderBy)
        {
            var files = DiscoverAllFilesMatchingPattern(workingDirectory, pattern);

            var discoveredFile = files.Where(where).OrderByDescending(orderBy).First();

            return ProcessModulePath(discoveredFile);
        }

        public static IEnumerable<string> FindAllFilesMatchingPattern(
            string workingDirectory,
            string pattern)
        {
            var files = DiscoverAllFilesMatchingPattern(workingDirectory, pattern);

            return files.Select(ProcessModulePath).ToList();
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
            using (var file = new ZipFile(archivePath))
            {
                return file.EntryFileNames;
            }
        }

        public static void RemoveFilesFromZip(string pathToArchive, string pattern)
        {
            using (var file = new ZipFile(pathToArchive))
            {
                file.RemoveSelectedEntries(pattern);
                file.Save();
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

        public static string ExtractFileFromZip(string pathToArchive, string fileName, string destinationDirectory)
        {
            using (var zip = new ZipFile(pathToArchive))
            {
                var entryToExtract = zip.Entries.FirstOrDefault(f => f.FileName.EndsWith(fileName));
                if (entryToExtract == null)
                {
                    throw new FileNotFoundException($"{fileName} not found in submission!");
                }

                entryToExtract.Extract(destinationDirectory);

                var extractedFilePath = $"{destinationDirectory}\\{entryToExtract.FileName.Replace("/", "\\")}";

                return extractedFilePath;
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
                    $@"'{pattern}' file not found in output directory!",
                    nameof(pattern));
            }

            return files;
        }
    }
}