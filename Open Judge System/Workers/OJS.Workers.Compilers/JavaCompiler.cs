namespace OJS.Workers.Compilers
{
    using System;
    using System.Text;

    using OJS.Common.Extensions;
    using OJS.Common.Models;

    public class JavaCompiler : Compiler
    {
        private const string JavaCompiledFileExtension = ".class";

        private static readonly string JavaSourceFileExtension = $".{CompilerType.Java.GetFileExtension()}";

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
            if (inputFile.EndsWith(JavaSourceFileExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                return inputFile;
            }

            return $"{inputFile}{JavaSourceFileExtension}";
        }

        public override string GetOutputFileName(string inputFileName)
        {
            var indexOfJavaExtension = inputFileName.LastIndexOf(JavaSourceFileExtension, StringComparison.InvariantCultureIgnoreCase);
            if (indexOfJavaExtension >= 0)
            {
                inputFileName = inputFileName.Substring(0, indexOfJavaExtension);
            }

            return $"{inputFileName}{JavaCompiledFileExtension}";
        }
    }
}
