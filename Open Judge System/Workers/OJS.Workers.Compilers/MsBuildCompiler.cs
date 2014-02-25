namespace OJS.Workers.Compilers
{
    using System.IO;
    using System.Linq;
    using System.Text;

    using OJS.Common.Extensions;

    public class MsBuildCompiler : Compiler
    {
        private readonly string outputPath;

        public MsBuildCompiler()
        {
            this.outputPath = FileHelpers.CreateTempDirectory();
        }

        ~MsBuildCompiler()
        {
            if (Directory.Exists(this.outputPath))
            {
                Directory.Delete(this.outputPath, true);
            }
        }

        public override string RenameInputFile(string inputFile)
        {
            return inputFile + ".zip";
        }

        public override string ChangeOutputFileAfterCompilation(string outputFile)
        {
            var newOutputFile = Directory.GetFiles(this.outputPath).FirstOrDefault(x => x.EndsWith(".exe"));
            return newOutputFile;
        }

        public override string BuildCompilerArguments(string inputFile, string outputFile, string additionalArguments)
        {
            var arguments = new StringBuilder();

            // TODO: inputFile -> unzip to filesPath
            var filesPath = inputFile;

            // Input file argument
            arguments.Append(string.Format("\"{0}\"", filesPath));
            arguments.Append(' ');

            // Settings
            arguments.Append("/t:rebuild ");
            arguments.Append("/p:Configuration=Release ");
            arguments.Append("/nologo ");

            // Output path argument
            arguments.Append(string.Format("/p:OutputPath=\"{0}\"", this.outputPath));
            arguments.Append(' ');

            // Additional compiler arguments
            arguments.Append(additionalArguments);

            return arguments.ToString().Trim();
        }
    }
}
