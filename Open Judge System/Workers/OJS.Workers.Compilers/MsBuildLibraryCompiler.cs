namespace OJS.Workers.Compilers
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;

    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Workers.Common;

    public class MsBuildLibraryCompiler : Compiler
    {
        protected string InputFile { get; private set; }

        public override string GetOutputFileName(string inputFileName)
        {
            var inputFolder = Path.GetDirectoryName(inputFileName);
            var outputPath = $"{inputFolder}\\_Compiled";
            Directory.CreateDirectory(outputPath);
            return outputPath;
        }

        public override string ChangeOutputFileAfterCompilation(string outputFolder)
        {
            var compiledFileName = Path.GetFileNameWithoutExtension(this.InputFile);
            compiledFileName = $"{compiledFileName}{GlobalConstants.ClassLibraryFileExtension}";
            var newOutputFile = Directory
                .EnumerateFiles(outputFolder)
                .FirstOrDefault(x => x.EndsWith(compiledFileName));
            if (newOutputFile == null)
            {
                var tempDir = DirectoryHelpers.CreateTempDirectory();
                Directory.Delete(tempDir);
                Directory.Move(outputFolder, tempDir);
                return tempDir;
            }

            return newOutputFile;
        }

        public override string BuildCompilerArguments(string inputFile, string outputFolder, string additionalArguments)
        {
            var arguments = new StringBuilder();
            arguments.Append($"\"{inputFile}\" ");
            arguments.Append($"/p:OutputPath=\"{outputFolder}\" ");
            arguments.Append("/p:PreBuildEvent=\"\" /p:PostBuildEvent=\"\" ");
            arguments.Append(additionalArguments);
            return arguments.ToString().Trim();
        }

        public override CompileResult Compile(string compilerPath, string inputFile, string additionalArguments)
        {
            if (compilerPath == null)
            {
                throw new ArgumentNullException(nameof(compilerPath));
            }

            if (inputFile == null)
            {
                throw new ArgumentNullException(nameof(inputFile));
            }

            if (!File.Exists(compilerPath))
            {
                return new CompileResult(false, $"Compiler not found! Searched in: {compilerPath}");
            }

            if (!File.Exists(inputFile))
            {
                return new CompileResult(false, $"Input file not found! Searched in: {inputFile}");
            }

            string newInputFilePath = this.RenameInputFile(inputFile);
            if (newInputFilePath != inputFile)
            {
                File.Move(inputFile, newInputFilePath);
                inputFile = newInputFilePath;
            }

            this.InputFile = inputFile;

            var outputFile = this.GetOutputFileName(inputFile);
            var arguments = this.BuildCompilerArguments(inputFile, outputFile, additionalArguments);

            var directoryInfo = new FileInfo(compilerPath).Directory;
            if (directoryInfo == null)
            {
                return new CompileResult(false, $"Compiler directory is null. Compiler path value: {compilerPath}");
            }

            var processStartInfo =
                new ProcessStartInfo(compilerPath)
                {
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    WorkingDirectory = directoryInfo.ToString(),
                    Arguments = arguments
                };

            this.UpdateCompilerProcessStartInfo(processStartInfo);

            var compilerOutput = ExecuteCompiler(processStartInfo);
 
            outputFile = this.ChangeOutputFileAfterCompilation(outputFile);

            if (!File.Exists(outputFile) && !compilerOutput.IsSuccessful)
            {
                return new CompileResult(false, $"Compiled file is missing. Compiler output: {compilerOutput.Output}");
            }

            if (!string.IsNullOrWhiteSpace(compilerOutput.Output))
            {
                return new CompileResult(true, compilerOutput.Output, outputFile);
            }

            return new CompileResult(outputFile);
        }
    }
}
