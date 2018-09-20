namespace OJS.Workers.Compilers
{
    using System;
    using System.IO;
    using System.Text;

    using OJS.Workers.Common;
    using OJS.Workers.Common.Helpers;

    public class JavaInPlaceFolderCompiler : Compiler
    {
        private const string JavaSourceFilesSearchPattern = "*.java";

        public JavaInPlaceFolderCompiler(int processExitTimeOutMultiplier)
            : base(processExitTimeOutMultiplier)
        {
        }

        public override string BuildCompilerArguments(string inputFolder, string outputDirectory, string additionalArguments)
        {
            var arguments = new StringBuilder();
            arguments.Append($"-d \"{outputDirectory}\" ");
            arguments.Append(additionalArguments);
            arguments.Append(' ');

            var filesToCompile =
                Directory.GetFiles(inputFolder, JavaSourceFilesSearchPattern, SearchOption.AllDirectories);
            for (var i = 0; i < filesToCompile.Length; i++)
            {
                arguments.Append($"\"{filesToCompile[i]}\"");
                arguments.Append(' ');
            }

            return arguments.ToString();
        }

        public override CompileResult Compile(string compilerPath, string inputDirectory, string additionalArguments)
        {
            if (compilerPath == null)
            {
                throw new ArgumentNullException(nameof(compilerPath));
            }

            if (inputDirectory == null)
            {
                throw new ArgumentNullException(nameof(inputDirectory));
            }

            if (!File.Exists(compilerPath))
            {
                return new CompileResult(false, $"Compiler not found! Searched in: {compilerPath}");
            }

            if (!Directory.Exists(inputDirectory))
            {
                return new CompileResult(false, $"Input directory not found! Searched in: {inputDirectory}");
            }

            this.CompilationDirectory = $"{inputDirectory}\\{CompilationDirectoryName}";

            if (Directory.Exists(this.CompilationDirectory))
            {
                DirectoryHelpers.SafeDeleteDirectory(this.CompilationDirectory, true);
            }

            Directory.CreateDirectory(this.CompilationDirectory);

            // Build compiler arguments
            var outputDirectory = this.CompilationDirectory;
            var arguments = this.BuildCompilerArguments(inputDirectory, outputDirectory, additionalArguments);

            // Find compiler directory
            var directoryInfo = new FileInfo(compilerPath).Directory;
            if (directoryInfo == null)
            {
                return new CompileResult(false, $"Compiler directory is null. Compiler path value: {compilerPath}");
            }

            // Prepare process start information
            var processStartInfo = this.SetCompilerProcessStartInfo(compilerPath, directoryInfo, arguments);
            var compilerOutput = ExecuteCompiler(processStartInfo, this.MaxProcessExitTimeOutInMilliseconds);

            outputDirectory = this.ChangeOutputFileAfterCompilation(outputDirectory);
            if (!compilerOutput.IsSuccessful)
            {
                return new CompileResult(false, $"Compiled file is missing. Compiler output: {compilerOutput.Output}");
            }

            if (!string.IsNullOrWhiteSpace(compilerOutput.Output))
            {
                return new CompileResult(true, compilerOutput.Output, outputDirectory);
            }

            return new CompileResult(outputDirectory);
        }
    }
}