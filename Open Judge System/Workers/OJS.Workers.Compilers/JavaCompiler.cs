namespace OJS.Workers.Compilers
{
    using System;
    using System.Text;

    using OJS.Workers.Common.Extensions;
    using OJS.Workers.Common.Models;

    public class JavaCompiler : Compiler
    {
        private const string JavaCompiledFileExtension = ".class";

        private static readonly string JavaSourceFileExtension = $".{CompilerType.Java.GetFileExtension()}";

        public JavaCompiler(int processExitTimeOutMultiplier)
            : base(processExitTimeOutMultiplier)
        {
        }

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
            var indexOfJavaSourceFileExtension =
                inputFileName.LastIndexOf(JavaSourceFileExtension, StringComparison.InvariantCultureIgnoreCase);

            if (indexOfJavaSourceFileExtension >= 0)
            {
                inputFileName = inputFileName.Substring(0, indexOfJavaSourceFileExtension);
            }

            return $"{inputFileName}{JavaCompiledFileExtension}";
        }
    }
}