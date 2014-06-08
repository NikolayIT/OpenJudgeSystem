namespace OJS.Workers.Compilers
{
    using System;
    using System.Text;

    using OJS.Common.Extensions;
    using OJS.Common.Models;

    public class JavaCompiler : Compiler
    {
        private const string JavaCompiledFileExtension = ".class";

        private static readonly string JavaSourceFileExtension = string.Format(".{0}", CompilerType.Java.GetFileExtension());

        public override string BuildCompilerArguments(string inputFile, string outputFile, string additionalArguments)
        {
            var arguments = new StringBuilder();

            // Additional compiler arguments
            arguments.Append(additionalArguments);
            arguments.Append(' ');

            // Input file argument
            arguments.Append(string.Format("\"{0}\"", inputFile));

            return arguments.ToString().Trim();
        }

        public override string RenameInputFile(string inputFile)
        {
            var inputFileExtension = inputFile.EndsWith(JavaSourceFileExtension) ? string.Empty : JavaSourceFileExtension;
            return string.Format("{0}{1}", inputFile, inputFileExtension);
        }

        public override string GetOutputFileName(string inputFileName)
        {
            var indexOfJavaExtension = inputFileName.LastIndexOf(JavaSourceFileExtension, StringComparison.InvariantCultureIgnoreCase);
            if (indexOfJavaExtension >= 0)
            {
                inputFileName = inputFileName.Substring(0, indexOfJavaExtension);
            }

            return string.Format("{0}{1}", inputFileName, JavaCompiledFileExtension);
        }
    }
}
