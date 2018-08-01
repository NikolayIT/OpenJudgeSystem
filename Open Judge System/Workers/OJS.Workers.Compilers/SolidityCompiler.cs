namespace OJS.Workers.Compilers
{
    using System.Text;

    using OJS.Common.Extensions;

    public class SolidityCompiler : Compiler
    {
        private const string BinaryFilePattern = "*.bin";

        public SolidityCompiler(int processExitTimeOutMultiplier)
            : base(processExitTimeOutMultiplier)
        {
        }

        public override bool ShouldDeleteSourceFile => false;

        public override string ChangeOutputFileAfterCompilation(string outputFile) =>
            FileHelpers.FindFileMatchingPattern(this.CompilationDirectory, BinaryFilePattern);

        public override string BuildCompilerArguments(
            string inputFile,
            string outputFile,
            string additionalArguments)
        {
            var arguments = new StringBuilder();

            // Output file argument
            arguments.Append($"-o \"{this.CompilationDirectory}\" ");

            arguments.Append("--bin --abi ");

            // Input file argument
            arguments.Append($"\"{inputFile}\" ");

            // Additional compiler arguments
            arguments.Append(additionalArguments);

            return arguments.ToString().Trim();
        }
    }
}