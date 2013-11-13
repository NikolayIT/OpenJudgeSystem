namespace OJS.Workers.Compilers
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    using OJS.Common.Models;
    using OJS.Workers.Common;

    /// <summary>
    /// Defines the base of the work with compilers algorithm and allow the subclasses to implement some of the algorithm parts.
    /// </summary>
    /// <remarks>Template method design pattern is used.</remarks>
    public abstract class Compiler : ICompiler
    {
        public static ICompiler CreateCompiler(CompilerType compilerType)
        {
            switch (compilerType)
            {
                case CompilerType.None:
                    return null;
                case CompilerType.CSharp:
                    return new CSharpCompiler();
                case CompilerType.CPlusPlusGcc:
                    return new CPlusPlusCompiler();
                default:
                    throw new ArgumentException("Unsupported compiler.");
            }
        }

        public CompileResult Compile(string compilerPath, string inputFile, string additionalArguments)
        {
            if (compilerPath == null)
            {
                throw new ArgumentNullException("compilerPath");
            }

            if (inputFile == null)
            {
                throw new ArgumentNullException("inputFile");
            }

            if (!File.Exists(compilerPath))
            {
                return new CompileResult(false, string.Format("Compiler not found! Searched in: {0}", compilerPath));
            }

            if (!File.Exists(inputFile))
            {
                return new CompileResult(false, string.Format("Input file not found! Searched in: {0}", inputFile));
            }

            // Move source file if needed
            string newInputFilePath = this.RenameInputFile(inputFile);
            if (newInputFilePath != inputFile)
            {
                File.Move(inputFile, newInputFilePath);
                inputFile = newInputFilePath;
            }

            // Build compiler arguments
            var outputFile = inputFile + ".exe";
            var arguments = this.BuildCompilerArguments(inputFile, outputFile, additionalArguments);

            // Find compiler directory
            var directoryInfo = new FileInfo(compilerPath).Directory;
            if (directoryInfo == null)
            {
                return new CompileResult(false, string.Format("Compiler directory is null. Compiler path value: {0}", compilerPath));
            }

            // Prepare process start information
            var processStartInfo = new ProcessStartInfo(compilerPath)
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
            string compilerOutput;
            using (var process = Process.Start(processStartInfo))
            {
                compilerOutput = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }

            // Check results and return CompilerResult instance
            if (!File.Exists(outputFile))
            {
                // Compiled file is missing
                return new CompileResult(false, compilerOutput);
            }

            if (!string.IsNullOrWhiteSpace(compilerOutput))
            {
                // Compile file is ready but the compiler has something on standard error (possibly compile warnings)
                return new CompileResult(true, compilerOutput);
            }
            
            // Compilation is ready without warnings
            return new CompileResult(outputFile);
        }

        public abstract string RenameInputFile(string inputFile);

        public abstract string BuildCompilerArguments(string inputFile, string outputFile, string additionalArguments);

        public abstract void UpdateCompilerProcessStartInfo(ProcessStartInfo processStartInfo);
    }
}
