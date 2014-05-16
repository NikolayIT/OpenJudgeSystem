namespace OJS.Workers.Compilers
{
    using System;
    using System.Text;

    public class JavaCompiler : Compiler
    {
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
            return string.Format("{0}{1}", inputFile, inputFile.EndsWith(".java") ? string.Empty : ".java");
        }

        public override string GetOutputFileName(string inputFileName)
        {
            var indexOfJavaExtension = inputFileName.LastIndexOf(".java", StringComparison.InvariantCultureIgnoreCase);
            if (indexOfJavaExtension >= 0)
            {
                inputFileName = inputFileName.Substring(0, indexOfJavaExtension);
            }

            return string.Format("{0}.class", inputFileName);
        }
    }
}
