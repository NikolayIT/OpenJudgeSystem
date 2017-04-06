namespace OJS.Workers.Compilers
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;

    using OJS.Common;
    using OJS.Common.Extensions;

    public class CPlusPlusZipCompiler : Compiler
    {
        private const string CPlusPlusClassFileExtension = ".cpp";
        private const string CPlusPlusHeaderFileExtension = ".h";
        private const string CClassFileExtension = ".c";

        private readonly string workingDirectory;

        public CPlusPlusZipCompiler()
        {
            this.workingDirectory = DirectoryHelpers.CreateTempDirectory();
        }

        ~CPlusPlusZipCompiler()
        {
            DirectoryHelpers.SafeDeleteDirectory(this.workingDirectory, true);
        }

        public override string RenameInputFile(string inputFile)
        {
            if (inputFile.EndsWith(GlobalConstants.ZipFileExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                return inputFile;
            }

            return $"{inputFile}{GlobalConstants.ZipFileExtension}";
        }

        public override string BuildCompilerArguments(string inputFile, string outputFile, string additionalArguments)
        {
            var arguments = new StringBuilder();

            arguments.Append($"-o \"{outputFile}\"");
            arguments.Append(' ');

            arguments.Append(additionalArguments);
            arguments.Append(' ');
            FileHelpers.UnzipFile(inputFile, this.workingDirectory);

            var filesToCompile = Directory.EnumerateFiles(
                    this.workingDirectory,
                    "*.*",
                    SearchOption.AllDirectories)
                .Where(f =>
                       f.EndsWith(CClassFileExtension) ||
                       f.EndsWith(CPlusPlusClassFileExtension) ||
                       f.EndsWith(CPlusPlusHeaderFileExtension));

            foreach (var file in filesToCompile)
            {
                arguments.Append($"\"{file}\"");
                arguments.Append(' ');
            }

            return arguments.ToString();
        }

        public override string GetOutputFileName(string inputFileName)
        {
            inputFileName = inputFileName.Replace(".zip", ".exe");
            return inputFileName;
        }
    }
}
