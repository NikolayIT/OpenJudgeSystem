namespace OJS.Workers.Compilers
{
    using System.Diagnostics;
    using System.Text;

    public class CPlusPlusCompiler : Compiler
    {
        public override string RenameInputFile(string inputFile)
        {
            // Add "cpp" extension so the compiler will treat the file as C++ code file
            return inputFile + ".cpp";
        }

        public override string BuildCompilerArguments(string inputFile, string outputFile, string additionalArguments)
        {
            var arguments = new StringBuilder();

            // Input file argument
            arguments.Append(string.Format("\"{0}\"", inputFile));
            arguments.Append(' ');

            // Output file argument
            arguments.Append(string.Format("-o \"{0}\"", outputFile));
            arguments.Append(' ');

            // Additional compiler arguments
            arguments.Append(additionalArguments);

            return arguments.ToString().Trim();
        }
    }
}
