namespace OJS.Workers.Compilers
{
    using System.Text;

    public class CSharpCompiler : Compiler
    {
        public CSharpCompiler(int processExitTimeOutMultiplier)
            : base(processExitTimeOutMultiplier)
        {
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
    }
}
