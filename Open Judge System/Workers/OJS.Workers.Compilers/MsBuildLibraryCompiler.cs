namespace OJS.Workers.Compilers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;

    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Workers.Common;

    public class MsBuildLibraryCompiler : Compiler
    {
        protected string InputFile { get; private set; }

        protected string OutputPath { get; private set; }

        public override string RenameInputFile(string inputFile)
        {
            this.InputFile = inputFile;
            var inputFolder = Path.GetDirectoryName(inputFile);
            this.OutputPath = $"{inputFolder}\\_Compiled";
            Directory.CreateDirectory(this.OutputPath);
            return inputFile;
        }

        public override string ChangeOutputFileAfterCompilation(string outputFile)
        {
            var compiledFileName = Path.GetFileNameWithoutExtension(this.InputFile);
            compiledFileName = $"{compiledFileName}{GlobalConstants.ClassLibraryFileExtension}";
            var newOutputFile = Directory
                .EnumerateFiles(this.OutputPath)
                .FirstOrDefault(x => x.EndsWith(compiledFileName));
            if (newOutputFile == null)
            {
                var tempDir = DirectoryHelpers.CreateTempDirectory();
                Directory.Delete(tempDir);
                Directory.Move(this.OutputPath, tempDir);
                return tempDir;
            }

            return newOutputFile;
        }

        public override string BuildCompilerArguments(string inputFile, string outputFile, string additionalArguments)
        {
            var arguments = new StringBuilder();

            // Input file argument
            arguments.Append($"\"{inputFile}\" ");

            // Output path argument
            arguments.Append($"/p:OutputPath=\"{this.OutputPath}\" ");

            // Disable pre and post build events
            arguments.Append("/p:PreBuildEvent=\"\" /p:PostBuildEvent=\"\" ");

            // Additional compiler arguments
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

            // Move source file if needed
            string newInputFilePath = this.RenameInputFile(inputFile);
            if (newInputFilePath != inputFile)
            {
                File.Move(inputFile, newInputFilePath);
                inputFile = newInputFilePath;
            }

            // Build compiler arguments
            var outputFile = this.GetOutputFileName(inputFile);
            var arguments = this.BuildCompilerArguments(inputFile, outputFile, additionalArguments);

            // Find compiler directory
            var directoryInfo = new FileInfo(compilerPath).Directory;
            if (directoryInfo == null)
            {
                return new CompileResult(false, $"Compiler directory is null. Compiler path value: {compilerPath}");
            }

            // Prepare process start information
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

            // Execute compiler
            var compilerOutput = ExecuteCompiler(processStartInfo);

            outputFile = this.ChangeOutputFileAfterCompilation(outputFile);

            // Check results and return CompilerResult instance
            if (!File.Exists(outputFile) && !compilerOutput.IsSuccessful)
            {
                // Compiled file is missing
                return new CompileResult(false, $"Compiled file is missing. Compiler output: {compilerOutput.Output}");
            }

            if (!string.IsNullOrWhiteSpace(compilerOutput.Output))
            {
                // Compile file is ready but the compiler has something on standard error (possibly compile warnings)
                return new CompileResult(true, compilerOutput.Output, outputFile);
            }

            // Compilation is ready without warnings
            return new CompileResult(outputFile);
        }
    }
}
