namespace OJS.Workers.Compilers
{
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    using OJS.Common;
    using OJS.Common.Extensions;

    public class SolidityCompiler : Compiler
    {
        private readonly string binaryFilePattern = $"*{GlobalConstants.ByteCodeFileExtension}";

        public SolidityCompiler(int processExitTimeOutMultiplier)
            : base(processExitTimeOutMultiplier)
        {
        }

        public override string ChangeOutputFileAfterCompilation(string outputFile) =>
            FileHelpers.FindFileMatchingPattern(this.CompilationDirectory, this.binaryFilePattern);

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
            var fileName = Path.GetFileName(inputFile);

            // Providing only the file name, instead of the file's full path,
            // because libraries placeholder in the byteCode breaks otherwise
            arguments.Append($"\"{fileName}\" ");

            // Additional compiler arguments
            arguments.Append(additionalArguments);

            return arguments.ToString().Trim();
        }

        public override ProcessStartInfo SetCompilerProcessStartInfo(
            string compilerPath,
            DirectoryInfo directoryInfo,
            string arguments)
        {
            // Calling the compiler from the working directory,
            // in order to provide just the file name as an input file argument,
            // instead of the full path to the file
            var workingDirectory = new DirectoryInfo(Directory.GetParent(this.CompilationDirectory).FullName);

            return base.SetCompilerProcessStartInfo(compilerPath, workingDirectory, arguments);
        }
    }
}