namespace OJS.Workers.Compilers
{
    using System.IO;
    using System.Linq;
    using System.Text;

    using ICSharpCode.SharpZipLib.Zip;

    using OJS.Common.Extensions;

    public class MsBuildCompiler : Compiler
    {
        private readonly string inputPath;
        private readonly string outputPath;

        public MsBuildCompiler()
        {
            this.inputPath = DirectoryHelpers.CreateTempDirectory();
            this.outputPath = DirectoryHelpers.CreateTempDirectory();
        }

        ~MsBuildCompiler()
        {
            DirectoryHelpers.SafeDeleteDirectory(this.inputPath, true);
            DirectoryHelpers.SafeDeleteDirectory(this.outputPath, true);
        }

        public override string RenameInputFile(string inputFile)
        {
            return inputFile + ".zip";
        }

        public override string ChangeOutputFileAfterCompilation(string outputFile)
        {
            var newOutputFile = Directory.GetFiles(this.outputPath).FirstOrDefault(x => x.EndsWith(".exe"));
            if (newOutputFile == null)
            {
                return null;
            }

            var tempFile = Path.GetTempFileName();
            var tempExeFile = tempFile + ".exe";
            File.Move(newOutputFile, tempExeFile);
            File.Delete(tempFile);
            return tempExeFile;
        }

        public override string BuildCompilerArguments(string inputFile, string outputFile, string additionalArguments)
        {
            var arguments = new StringBuilder();

            UnzipFile(inputFile, this.inputPath);
            string solutionOrProjectFile = this.FindSolutionOrProjectFile();

            // Input file argument
            arguments.Append(string.Format("\"{0}\" ", solutionOrProjectFile));

            // Output path argument
            arguments.Append(string.Format("/p:OutputPath=\"{0}\" ", this.outputPath));

            // Additional compiler arguments
            arguments.Append(additionalArguments);

            return arguments.ToString().Trim();
        }

        private static void UnzipFile(string fileToUnzip, string outputDirectory)
        {
            var fastZip = new FastZip();
            fastZip.ExtractZip(fileToUnzip, outputDirectory, null);
        }

        private string FindSolutionOrProjectFile()
        {
            var solutionOrProjectFile = Directory.GetFiles(this.inputPath).FirstOrDefault(x => x.EndsWith(".sln"));
            if (string.IsNullOrWhiteSpace(solutionOrProjectFile))
            {
                solutionOrProjectFile = Directory.GetFiles(this.inputPath).FirstOrDefault(x => x.EndsWith(".csproj") || x.EndsWith(".vbproj"));
            }

            if (string.IsNullOrWhiteSpace(solutionOrProjectFile))
            {
                var directory = Directory.GetDirectories(this.inputPath).FirstOrDefault();
                if (directory != null)
                {
                    solutionOrProjectFile = Directory.GetFiles(directory).FirstOrDefault(x => x.EndsWith(".sln") || x.EndsWith(".csproj") || x.EndsWith(".vbproj"));
                }
            }

            return solutionOrProjectFile;
        }
    }
}
