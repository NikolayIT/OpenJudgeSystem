namespace OJS.Workers.Compilers
{
    using System.IO;
    using System.Text;

    using OJS.Common;
    using OJS.Common.Extensions;

    public class DotNetCompiler : Compiler
    {
        public override int MaxProcessExitTimeOutMillisecond => GlobalConstants.DefaultProcessExitTimeOutMilliseconds * 9;
        
        public override bool ShouldDeleteSourceFile => false;

        public override string ChangeOutputFileAfterCompilation(string outputFile)
        {
            string compiledFileName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(outputFile));
            string rootDir = Path.GetDirectoryName(outputFile);
            string compiledFile =
                FileHelpers.FindFileMatchingPattern(
                    rootDir,
                    $"{compiledFileName}{GlobalConstants.ClassLibraryFileExtension}");

            return compiledFile;
        }

        public override string BuildCompilerArguments(string inputFile, string outputFile, string additionalArguments)
        {
            var compilingDir = $"{Path.GetDirectoryName(inputFile)}\\{CompilationDirectoryName}";
            Directory.CreateDirectory(compilingDir);

            var arguments = new StringBuilder();
            arguments.Append("build ");
            arguments.Append($"-o {compilingDir} ");
            arguments.Append($"\"{inputFile}\" ");
            arguments.Append(additionalArguments);
            return arguments.ToString().Trim();
        }
    }
}