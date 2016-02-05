namespace OJS.Workers.Tools.Disassemblers
{
    using System.Text;

    public class DotNetDisassembler : Disassembler
    {
        public DotNetDisassembler(string dotNetDisassemblerPath)
            : base(dotNetDisassemblerPath)
        {
        }

        protected override string BuildDisassemblerArguments(string inputFilePath, string additionalArguments)
        {
            var arguments = new StringBuilder();

            // Input file argument
            arguments.Append($"\"{inputFilePath}\"");
            arguments.Append(' ');

            // Additional disassembler arguments
            arguments.Append(additionalArguments);
            arguments.Append(' ');

            // Displays the results to the OUT stream
            arguments.Append("/text");

            return arguments.ToString();
        }
    }
}
