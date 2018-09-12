namespace OJS.Workers.Compilers
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;

    using OJS.Workers.Common;
    using OJS.Workers.Common.Helpers;

    public class MsBuildCompiler : Compiler
    {
        private const string CsharpProjectFileExtension = ".csproj";
        private const string VisualBasicProjectFileExtension = ".vbproj";
        private const string AllFilesSearchPattern = "*.*";
        private const string SolutionFilesSearchPattern = "*.sln";

        public MsBuildCompiler(int processExitTimeOutMultiplier)
            : base(processExitTimeOutMultiplier)
        {
        }

        public override string RenameInputFile(string inputFile) => $"{inputFile}{Constants.ZipFileExtension}";

        public override string ChangeOutputFileAfterCompilation(string outputFile)
        {
            var newOutputFile = Directory
                .EnumerateFiles(this.CompilationDirectory)
                .FirstOrDefault(x => x.EndsWith(Constants.ExecutableFileExtension));

            var tempFile = Path.GetTempFileName();
            var tempExeFile = $"{Path.GetDirectoryName(outputFile)}\\{Path.GetFileName(tempFile)}{Constants.ExecutableFileExtension}";
            File.Move(newOutputFile, tempExeFile);
            File.Delete(tempFile);
            return tempExeFile;
        }

        public override string BuildCompilerArguments(string inputFile, string outputFile, string additionalArguments)
        {
            var arguments = new StringBuilder();

            FileHelpers.UnzipFile(inputFile, this.CompilationDirectory);
            var solutionOrProjectFile = this.FindSolutionOrProjectFile();

            if (string.IsNullOrWhiteSpace(solutionOrProjectFile))
            {
                throw new ArgumentException(
                    "Input file does not contain a project or solution file.",
                    nameof(inputFile));
            }

            // Input file argument
            arguments.Append($"\"{solutionOrProjectFile}\" ");

            // Output path argument
            arguments.Append($"/p:OutputPath=\"{this.CompilationDirectory}\" ");

            // Disable pre and post build events
            arguments.Append("/p:PreBuildEvent=\"\" /p:PostBuildEvent=\"\" ");

            // Additional compiler arguments
            arguments.Append(additionalArguments);

            return arguments.ToString().Trim();
        }

        private string FindSolutionOrProjectFile()
        {
            var solutionOrProjectFile = Directory
                .EnumerateFiles(this.CompilationDirectory, SolutionFilesSearchPattern, SearchOption.AllDirectories)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(solutionOrProjectFile))
            {
                solutionOrProjectFile = Directory
                    .EnumerateFiles(this.CompilationDirectory, AllFilesSearchPattern, SearchOption.AllDirectories)
                    .FirstOrDefault(x =>
                        x.EndsWith(CsharpProjectFileExtension, StringComparison.OrdinalIgnoreCase) ||
                        x.EndsWith(VisualBasicProjectFileExtension, StringComparison.OrdinalIgnoreCase));
            }

            return solutionOrProjectFile;
        }
    }
}
