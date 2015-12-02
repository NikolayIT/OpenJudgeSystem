namespace OJS.Workers.Compilers
{
    using System;
    using System.Text;

    using OJS.Common;

    public class JavaCompiler : Compiler
    {
        public override string BuildCompilerArguments(string inputFile, string outputFile, string additionalArguments)
        {
            var arguments = new StringBuilder();

            // Additional compiler arguments
            arguments.Append(additionalArguments);
            arguments.Append(' ');

            // Input file argument
            arguments.Append($"\"{inputFile}\"");

            return arguments.ToString().Trim();
        }

        public override string RenameInputFile(string inputFile)
        {
            var inputFileExtension = inputFile.EndsWith(GlobalConstants.JavaSourceFileExtension, StringComparison.InvariantCultureIgnoreCase)
                ? string.Empty
                : GlobalConstants.JavaSourceFileExtension;
            return $"{inputFile}{inputFileExtension}";
        }

        public override string GetOutputFileName(string inputFileName)
        {
            var indexOfJavaExtension = inputFileName
                .LastIndexOf(GlobalConstants.JavaSourceFileExtension, StringComparison.InvariantCultureIgnoreCase);
            if (indexOfJavaExtension >= 0)
            {
                inputFileName = inputFileName.Substring(0, indexOfJavaExtension);
            }

            return $"{inputFileName}{GlobalConstants.JavaCompiledFileExtension}";
        }
    }
}
