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

        private static readonly Random Rand = new Random();

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
            File.AppendAllText(@"D:\Log.txt", inputFile);
            // Input file argument
            arguments.Append($"\"{inputFile}\"");
            arguments.Append(' ');

            // Output file argument
            arguments.Append($"-o \"{outputFile}\"");
            arguments.Append(' ');

            // Additional compiler arguments
            arguments.Append(additionalArguments);
            FileHelpers.UnzipFile(inputFile, this.workingDirectory);

            // Input files arguments
            var filesToCompile = Directory.EnumerateFiles(
                    this.workingDirectory,
                    "*.*",
                    SearchOption.AllDirectories)
                .Where(
                    f =>
                        f.EndsWith(CClassFileExtension) ||
                        f.EndsWith(CPlusPlusClassFileExtension) ||
                        f.EndsWith(CPlusPlusHeaderFileExtension));

            foreach (var file in filesToCompile)
            {
                arguments.Append($"\"{file}\"");
                arguments.Append(' ');
            }

            File.WriteAllText(@"D:\Log.txt",arguments.ToString());
            return arguments.ToString();
        }

        public override string ChangeOutputFileAfterCompilation(string outputFile)
        {
            var newOutputFile = Directory
                .EnumerateFiles(this.workingDirectory)
                .FirstOrDefault(x => x.EndsWith(GlobalConstants.ExecutableFileExtension));
            if (newOutputFile == null)
            {
                // ??? create -> delete -> move into deleted dir?
                var tempDir = DirectoryHelpers.CreateTempDirectory();
                Directory.Delete(tempDir);
                Directory.Move(this.workingDirectory, tempDir);
                return tempDir;
            }

            var tempFile = Path.GetTempFileName() + Rand.Next();
            var tempExeFile = $"{tempFile}{GlobalConstants.ExecutableFileExtension}";
            File.Move(newOutputFile, tempExeFile);
            File.Delete(tempFile);
            return tempExeFile;
        }
    }
}
