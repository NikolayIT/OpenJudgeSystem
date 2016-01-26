namespace OJS.Workers.Compilers
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Ionic.Zip;

    using OJS.Common;
    using OJS.Common.Extensions;

    public class MsBuildCompiler : Compiler
    {
        private const string NuGetExecutablePath = @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\nuget.exe"; // TODO: move to settings

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

        public override string RenameInputFile(string inputFile)
        {
            return inputFile + ".zip";
        }

        public override string ChangeOutputFileAfterCompilation(string outputFile)
        {
            var newOutputFile = Directory.GetFiles(this.outputPath).FirstOrDefault(x => x.EndsWith(".exe"));
            if (newOutputFile == null)
            {
                var tempDir = DirectoryHelpers.CreateTempDirectory();
                Directory.Delete(tempDir);
                Directory.Move(this.outputPath, tempDir);
                return tempDir;
            }

            var tempFile = Path.GetTempFileName() + Rand.Next();
            var tempExeFile = tempFile + ".exe";
            File.Move(newOutputFile, tempExeFile);
            File.Delete(tempFile);
            return tempExeFile;
        }

        public override string BuildCompilerArguments(string inputFile, string outputFile, string additionalArguments)
        {
            var arguments = new StringBuilder();

            UnzipFile(inputFile, this.inputPath);
            string solutionOrProjectFile = this.FindSolutionOrProjectFile();

            if (solutionOrProjectFile.EndsWith(".sln"))
            {
                RestoreNugetPackages(solutionOrProjectFile);
            }

            // Input file argument
            arguments.Append(string.Format("\"{0}\" ", solutionOrProjectFile));

            // Output path argument
            arguments.Append(string.Format("/p:OutputPath=\"{0}\" ", this.outputPath));

            // Disable pre and post build events
            arguments.Append("/p:PreBuildEvent=\"\" /p:PostBuildEvent=\"\" ");

            // Additional compiler arguments
            arguments.Append(additionalArguments);

            return arguments.ToString().Trim();
        }

        private static void UnzipFile(string fileToUnzip, string outputDirectory)
        {
            using (var zipFile = ZipFile.Read(fileToUnzip))
            {
                foreach (var entry in zipFile)
                {
                    entry.Extract(outputDirectory, ExtractExistingFileAction.OverwriteSilently);
                }
            }
        }

        private static void RestoreNugetPackages(string solution)
        {
            var solutionFileInfo = new FileInfo(solution);

            var processStartInfo = new ProcessStartInfo()
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = solutionFileInfo.DirectoryName,
                FileName = NuGetExecutablePath,
                Arguments = string.Format("restore \"{0}\"", solutionFileInfo.Name)
            };

            using (var process = new Process())
            {
                process.StartInfo = processStartInfo;
                process.Start();
                process.WaitForExit(GlobalConstants.DefaultProcessExitTimeOutMilliseconds);
            }
        }

        private string FindSolutionOrProjectFile()
        {
            var solutionOrProjectFile = Directory
                .EnumerateFiles(this.inputPath, "*.sln", SearchOption.AllDirectories)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(solutionOrProjectFile))
            {
                solutionOrProjectFile = Directory
                    .EnumerateFiles(this.inputPath, "*.*", SearchOption.AllDirectories)
                    .FirstOrDefault(x =>
                        x.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) ||
                        x.EndsWith(".vbproj", StringComparison.OrdinalIgnoreCase));
            }

            return solutionOrProjectFile;
        }
    }
}
