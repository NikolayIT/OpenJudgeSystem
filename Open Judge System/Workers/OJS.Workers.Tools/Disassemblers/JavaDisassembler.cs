namespace OJS.Workers.Tools.Disassemblers
{
    using System.Text;

    public class JavaDisassembler : Disassembler
    {
        // https://docs.oracle.com/javase/8/docs/technotes/tools/windows/javap.html
        private const string PrintDisassembledCodeArgument = "-c";

        public JavaDisassembler(string javaDisassemblerPath)
            : base(javaDisassemblerPath)
        {
        }

        protected override string BuildDisassemblerArguments(string inputFilePath, string additionalArguments)
        {
            var arguments = new StringBuilder();

            arguments.Append(additionalArguments);
            arguments.Append(' ');

            arguments.Append(PrintDisassembledCodeArgument);
            arguments.Append(' ');

            // Input file argument
            arguments.Append($"\"{inputFilePath}\"");

            return arguments.ToString();
        }
    }
}
