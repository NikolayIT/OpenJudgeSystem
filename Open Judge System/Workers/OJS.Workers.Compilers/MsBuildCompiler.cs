namespace OJS.Workers.Compilers
{
    using System.IO;
    using System.Linq;
    using System.Text;

    using Ionic.Zip;

    using OJS.Common.Extensions;

    public class MsBuildCompiler : Compiler
    {
        private readonly string inputPath;
        private readonly string outputPath;

        public MsBuildCompiler()
        {
            this.inputPath = FileHelpers.CreateTempDirectory();
            this.outputPath = FileHelpers.CreateTempDirectory();
        }

        ~MsBuildCompiler()
        {
            if (Directory.Exists(this.inputPath))
            {
                Directory.Delete(this.inputPath, true);
            }

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
            if (newOutputFile == null)
            {
                return null;
            }

            var randomFileName = Path.GetTempFileName() + ".exe";
            File.Move(newOutputFile, randomFileName);
            return randomFileName;
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
            using (var zipFile = ZipFile.Read(fileToUnzip))
            {
                foreach (var entry in zipFile)
                {
                    entry.Extract(outputDirectory, ExtractExistingFileAction.OverwriteSilently);
                }
            }
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
