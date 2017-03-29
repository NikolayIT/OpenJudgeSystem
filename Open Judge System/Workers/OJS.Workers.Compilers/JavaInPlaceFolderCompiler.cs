namespace OJS.Workers.Compilers
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    using OJS.Workers.Common;

    public class JavaInPlaceFolderCompiler : Compiler
    {
        private const string JavaSourceFilesSearchPattern = "*.java";

        public override string GetOutputFileName(string inputDirectoryName)
        {
            var outputPath = $"{inputDirectoryName}\\_Compiled";
            Directory.CreateDirectory(outputPath);
            return outputPath;
        }

        public override string BuildCompilerArguments(string inputFolder, string outputDirectory, string additionalArguments)
        {
            var arguments = new StringBuilder();

            // Output path argument
            arguments.Append($"-d \"{outputDirectory}\" ");

            // Additional compiler arguments
            arguments.Append(additionalArguments);
            arguments.Append(' ');

            // Input files arguments
            var filesToCompile =
                Directory.GetFiles(inputFolder, JavaSourceFilesSearchPattern, SearchOption.AllDirectories);
            for (var i = 0; i < filesToCompile.Length; i++)
            {
                arguments.Append($"\"{filesToCompile[i]}\"");
                arguments.Append(' ');
            }

            File.WriteAllText("E:\\args.txt",string.Join(", ", arguments));
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

            // Build compiler arguments
            var outputDirectory = this.GetOutputFileName(inputDirectory);
            var arguments = this.BuildCompilerArguments(inputDirectory, outputDirectory, additionalArguments);

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

            outputDirectory = this.ChangeOutputFileAfterCompilation(outputDirectory);

            // Check results and return CompilerResult instance
            if (!Directory.Exists(outputDirectory) && !compilerOutput.IsSuccessful)
            {
                // Compiled file is missing
                return new CompileResult(false, $"Compiled file is missing. Compiler output: {compilerOutput.Output}");
            }

            if (!string.IsNullOrWhiteSpace(compilerOutput.Output))
            {
                // Compile file is ready but the compiler has something on standard error (possibly compile warnings)
                return new CompileResult(true, compilerOutput.Output, outputDirectory);
            }

            // Compilation is ready without warnings
            return new CompileResult(outputDirectory);
        }
    }
}