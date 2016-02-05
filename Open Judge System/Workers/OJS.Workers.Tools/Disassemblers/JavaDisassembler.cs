namespace OJS.Workers.Tools.Disassemblers
{
    using System.Text;

    public class JavaDisassembler : Disassembler
    {
        public JavaDisassembler(string javaDisassemblerPath)
            : base(javaDisassemblerPath)
        {
        }

        protected override string BuildDisassemblerArguments(string inputFilePath, string additionalArguments)
        {
            var arguments = new StringBuilder();

            arguments.Append(additionalArguments);
            arguments.Append(' ');

            // Disassembled code (https://docs.oracle.com/javase/8/docs/technotes/tools/windows/javap.html)
            arguments.Append("-c");
            arguments.Append(' ');

            // Input file argument
            arguments.Append($"\"{inputFilePath}\"");

            return arguments.ToString();
        }
    }
}
