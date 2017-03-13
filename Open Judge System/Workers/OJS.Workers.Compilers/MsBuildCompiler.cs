namespace OJS.Workers.Compilers
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;

    using OJS.Common;
    using OJS.Common.Extensions;

    public class MsBuildCompiler : Compiler
    {
        private const string SolutionFileExtension = ".sln";
        private const string CsharpProjectFileExtension = ".csproj";
        private const string VisualBasicProjectFileExtension = ".vbproj";
        private const string AllFilesSearchPattern = "*.*";
        private const string SolutionFilesSearchPattern = "*.sln";
        private const string NuGetExecutablePath = @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\nuget.exe"; // TODO: move to settings
        private const int NuGetRestoreProcessExitTimeOutMilliseconds = 2 * GlobalConstants.DefaultProcessExitTimeOutMilliseconds;

        private static readonly Random Rand = new Random();

        private readonly string inputPath;
        private readonly string outputPath;

        public MsBuildCompiler()
        {
            this.inputPath = DirectoryHelpers.CreateTempDirectory();
            this.outputPath = DirectoryHelpers.CreateTempDirectory();
        }

        // TODO: delete the temp files manually somehow
        ~MsBuildCompiler()
        {
            DirectoryHelpers.SafeDeleteDirectory(this.inputPath, true);
            DirectoryHelpers.SafeDeleteDirectory(this.outputPath, true);
        }

        public override string RenameInputFile(string inputFile) => $"{inputFile}{GlobalConstants.ZipFileExtension}";

        public override string ChangeOutputFileAfterCompilation(string outputFile)
        {
            var newOutputFile = Directory
                .EnumerateFiles(this.outputPath)
                .FirstOrDefault(x => x.EndsWith(GlobalConstants.ExecutableFileExtension));
            if (newOutputFile == null)
            {
                var tempDir = DirectoryHelpers.CreateTempDirectory();
                Directory.Delete(tempDir);
                Directory.Move(this.outputPath, tempDir);
                return tempDir;
            }

            var tempFile = Path.GetTempFileName() + Rand.Next();
            var tempExeFile = $"{tempFile}{GlobalConstants.ExecutableFileExtension}";
            File.Move(newOutputFile, tempExeFile);
            File.Delete(tempFile);
            return tempExeFile;
        }

        public override string BuildCompilerArguments(string inputFile, string outputFile, string additionalArguments)
        {
            var arguments = new StringBuilder();

            FileHelpers.UnzipFile(inputFile, this.inputPath);
            var solutionOrProjectFile = this.FindSolutionOrProjectFile();

            if (string.IsNullOrWhiteSpace(solutionOrProjectFile))
            {
                throw new ArgumentException(
                    "Input file does not contain a project or solution file.",
                    nameof(inputFile));
            }

            ////if (solutionOrProjectFile.EndsWith(SolutionFileExtension))
            ////{
            ////    RestoreNugetPackages(solutionOrProjectFile);
            ////}

            // Input file argument
            arguments.Append($"\"{solutionOrProjectFile}\" ");

            // Output path argument
            arguments.Append($"/p:OutputPath=\"{this.outputPath}\" ");

            // Disable pre and post build events
            arguments.Append("/p:PreBuildEvent=\"\" /p:PostBuildEvent=\"\" ");

            // Additional compiler arguments
            arguments.Append(additionalArguments);

            return arguments.ToString().Trim();
        }

        private static void RestoreNugetPackages(string solution)
        {
            var solutionFileInfo = new FileInfo(solution);

            var processStartInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = solutionFileInfo.DirectoryName,
                FileName = NuGetExecutablePath,
                Arguments = $"restore \"{solutionFileInfo.Name}\""
            };

            using (var process = Process.Start(processStartInfo))
            {
                if (process != null)
                {
                    var exited = process.WaitForExit(NuGetRestoreProcessExitTimeOutMilliseconds);
                    if (!exited)
                    {
                        process.Kill();
                    }
                }
            }
        }

        private string FindSolutionOrProjectFile()
        {
            var solutionOrProjectFile = Directory
                .EnumerateFiles(this.inputPath, SolutionFilesSearchPattern, SearchOption.AllDirectories)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(solutionOrProjectFile))
            {
                solutionOrProjectFile = Directory
                    .EnumerateFiles(this.inputPath, AllFilesSearchPattern, SearchOption.AllDirectories)
                    .FirstOrDefault(x =>
                        x.EndsWith(CsharpProjectFileExtension, StringComparison.OrdinalIgnoreCase) ||
                        x.EndsWith(VisualBasicProjectFileExtension, StringComparison.OrdinalIgnoreCase));
            }

            return solutionOrProjectFile;
        }
    }
}
