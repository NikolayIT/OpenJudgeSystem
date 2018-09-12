namespace OJS.Workers.Compilers
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;

    using OJS.Workers.Common;
    using OJS.Workers.Common.Helpers;

    public class MsBuildLibraryCompiler : Compiler
    {
        public MsBuildLibraryCompiler(int processExitTimeOutMultiplier)
            : base(processExitTimeOutMultiplier)
        {
        }

        protected string InputFile { get; private set; }

        public override string ChangeOutputFileAfterCompilation(string outputFolder)
        {
            var compiledFileName = Path.GetFileNameWithoutExtension(this.InputFile);
            compiledFileName = $"{compiledFileName}{Constants.ClassLibraryFileExtension}";
            var newOutputFile = Directory
                .EnumerateFiles(outputFolder)
                .FirstOrDefault(x => x.EndsWith(compiledFileName));
            if (newOutputFile == null)
            {
                return outputFolder;
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

            this.CompilationDirectory = $"{Path.GetDirectoryName(inputFile)}\\{CompilationDirectoryName}";

            if (Directory.Exists(this.CompilationDirectory))
            {
                DirectoryHelpers.SafeDeleteDirectory(this.CompilationDirectory, true);
            }

            Directory.CreateDirectory(this.CompilationDirectory);

            string newInputFilePath = this.RenameInputFile(inputFile);
            if (newInputFilePath != inputFile)
            {
                File.Move(inputFile, newInputFilePath);
                inputFile = newInputFilePath;
            }

            this.InputFile = inputFile;

            var outputFile = this.CompilationDirectory;
            var arguments = this.BuildCompilerArguments(inputFile, outputFile, additionalArguments);

            var directoryInfo = new FileInfo(compilerPath).Directory;
            if (directoryInfo == null)
            {
                return new CompileResult(false, $"Compiler directory is null. Compiler path value: {compilerPath}");
            }

            var processStartInfo = this.SetCompilerProcessStartInfo(compilerPath, directoryInfo, arguments);

            var compilerOutput = ExecuteCompiler(processStartInfo, this.MaxProcessExitTimeOutInMilliseconds);

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
