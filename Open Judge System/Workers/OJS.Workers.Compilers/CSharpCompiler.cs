namespace OJS.Workers.Compilers
{
    using System.Diagnostics;
    using System.Text;

    public class CSharpCompiler : BaseCompiler
    {
        public override string RenameInputFile(string inputFile)
        {
            // No need to rename input file
            return inputFile;
        }

        public override string BuildCompilerArguments(string inputFile, string outputFile, string additionalArguments)
        {
            var arguments = new StringBuilder();

            // Output file argument
            arguments.Append(string.Format("/out:\"{0}\"", outputFile));
            arguments.Append(' ');

            // Input file argument
            arguments.Append(string.Format("\"{0}\"", inputFile));
            arguments.Append(' ');

            // Additional compiler arguments
            arguments.Append(additionalArguments);

            return arguments.ToString().Trim();
        }

        public override void UpdateCompilerProcessStartInfo(ProcessStartInfo processStartInfo)
        {
            // No need to update compiler process start info
        }
    }
}
