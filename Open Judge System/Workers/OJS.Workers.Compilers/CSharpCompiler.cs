namespace OJS.Workers.Compilers
{
    using System.Text;

    public class CSharpCompiler : Compiler
    {
        public override string BuildCompilerArguments(string inputFile, string outputFile, string additionalArguments)
        {
            var arguments = new StringBuilder();

            // Output file argument
            arguments.Append($"/out:\"{outputFile}\"");
            arguments.Append(' ');

            // Input file argument
            arguments.Append($"\"{inputFile}\"");
            arguments.Append(' ');

            // Additional compiler arguments
            arguments.Append(additionalArguments);

            return arguments.ToString().Trim();
        }
    }
}
