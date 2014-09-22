namespace OJS.Workers.Compilers
{
    using System.Text;

    public class DotNetDisassembler : Compiler
    {
        public override string BuildCompilerArguments(string inputFile, string outputFile, string additionalArguments)
        {
            var arguments = new StringBuilder();

            // Input file argument
            arguments.Append(string.Format("\"{0}\"", inputFile));
            arguments.Append(' ');

            // Output file argument
            arguments.Append(string.Format("/output:\"{0}\"", outputFile));
            arguments.Append(' ');

            // TODO: Choose appropriate arguments from http://msdn.microsoft.com/en-us/library/f7dy01k1(v=vs.110).aspx in order to compare only the important parts of the code

            // Additional compiler arguments
            arguments.Append(additionalArguments);

            return arguments.ToString().Trim();
        }
    }
}
