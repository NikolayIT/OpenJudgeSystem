namespace OJS.Workers.Compilers
{
    using System.Text;

    public class MsBuildCompiler : Compiler
    {
        public override string RenameInputFile(string inputFile)
        {
            // No need to rename input file
            return inputFile + ".zip";
        }

        public override string BuildCompilerArguments(string inputFile, string outputFile, string additionalArguments)
        {
            var arguments = new StringBuilder();

            // TODO: input file -> unzip to filesPath
            var filesPath = inputFile;

            // Input file argument
            arguments.Append(string.Format("\"{0}\"", filesPath));
            arguments.Append(' ');

            // Settings
            arguments.Append("/t:rebuild ");
            arguments.Append("/p:Configuration=Release ");
            arguments.Append("/nologo ");

            // Output path argument
            arguments.Append(string.Format("/p:OutputPath=\"{0}\"", outputFile));
            arguments.Append(' ');

            // Additional compiler arguments
            arguments.Append(additionalArguments);

            return arguments.ToString().Trim();
        }

        public override void UpdateCompilerProcessStartInfo(System.Diagnostics.ProcessStartInfo processStartInfo)
        {
            // No need to update compiler process start info
        }
    }
}
