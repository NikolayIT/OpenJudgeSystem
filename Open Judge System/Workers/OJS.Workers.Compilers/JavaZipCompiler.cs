namespace OJS.Workers.Compilers
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Ionic.Zip;
    
    using OJS.Common;
    using OJS.Common.Extensions;

    public class JavaZipCompiler : Compiler
    {
        private const string JavaCompiledFilesSearchPattern = "*.class";
        private const string JavaSourceFilesSearchPattern = "*.java";
        private const string MainClassFilePathSuffix = "\\Main.class";

        private readonly string workingDirectory;

        public JavaZipCompiler()
        {
            this.workingDirectory = DirectoryHelpers.CreateTempDirectory();
        }

        ~JavaZipCompiler()
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

        public override string GetOutputFileName(string inputFileName) => new FileInfo(inputFileName).DirectoryName;

        public override string BuildCompilerArguments(string inputFile, string outputDirectory, string additionalArguments)
        {
            var arguments = new StringBuilder();

            // Output path argument
            arguments.Append($"-d \"{outputDirectory}\" ");

            // Additional compiler arguments
            arguments.Append(additionalArguments);
            arguments.Append(' ');

            UnzipFile(inputFile, this.workingDirectory);

            // Input files arguments
            var filesToCompile =
                Directory.GetFiles(this.workingDirectory, JavaSourceFilesSearchPattern, SearchOption.AllDirectories);
            for (var i = 0; i < filesToCompile.Length; i++)
            {
                arguments.Append($"\"{filesToCompile[i]}\"");
                arguments.Append(' ');
            }

            return arguments.ToString();
        }

        public override string ChangeOutputFileAfterCompilation(string outputDirectory)
        {
            var compiledFiles =
                Directory.EnumerateFiles(outputDirectory, JavaCompiledFilesSearchPattern, SearchOption.AllDirectories);

            // TODO: Find the main class after analyzing which source file contains the main method
            var mainClassFile = compiledFiles
                .FirstOrDefault(file => file.EndsWith(MainClassFilePathSuffix, StringComparison.InvariantCultureIgnoreCase));

            return mainClassFile;
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
    }
}