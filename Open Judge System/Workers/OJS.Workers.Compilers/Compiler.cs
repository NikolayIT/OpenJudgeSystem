namespace OJS.Workers.Compilers
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading;

    using OJS.Common;
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
                case CompilerType.MsBuild:
                    return new MsBuildCompiler();
                case CompilerType.Java:
                    return new JavaCompiler();
                case CompilerType.JavaZip:
                    return new JavaZipCompiler();
                case CompilerType.JavaInPlaceCompiler:
                    return new JavaInPlaceFolderCompiler();
                case CompilerType.MsBuildLibrary:
                    return new MsBuildLibraryCompiler();
                case CompilerType.CPlusPlusZip:
                    return new CPlusPlusZipCompiler();
                default:
                    throw new ArgumentException("Unsupported compiler.");
            }
        }

        public virtual CompileResult Compile(string compilerPath, string inputFile, string additionalArguments)
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

            // Delete input file
            if (File.Exists(newInputFilePath))
            {
                File.Delete(newInputFilePath);
            }

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

        public virtual string RenameInputFile(string inputFile)
        {
            return inputFile;
        }

        public virtual string GetOutputFileName(string inputFileName)
        {
            return inputFileName + ".exe";
        }

        public virtual string ChangeOutputFileAfterCompilation(string outputFile)
        {
            return outputFile;
        }

        public abstract string BuildCompilerArguments(string inputFile, string outputFile, string additionalArguments);

        public virtual void UpdateCompilerProcessStartInfo(ProcessStartInfo processStartInfo)
        {
        }

        protected static CompilerOutput ExecuteCompiler(ProcessStartInfo compilerProcessStartInfo)
        {
            var outputBuilder = new StringBuilder();
            var errorOutputBuilder = new StringBuilder();
            int exitCode;

            using (var outputWaitHandle = new AutoResetEvent(false))
            {
                using (var errorWaitHandle = new AutoResetEvent(false))
                {
                    using (var process = new Process())
                    {
                        process.StartInfo = compilerProcessStartInfo;

                        process.OutputDataReceived += (sender, e) =>
                            {
                                if (e.Data == null)
                                {
                                    outputWaitHandle.Set();
                                }
                                else
                                {
                                    outputBuilder.AppendLine(e.Data);
                                }
                            };

                        process.ErrorDataReceived += (sender, e) =>
                            {
                                if (e.Data == null)
                                {
                                    errorWaitHandle.Set();
                                }
                                else
                                {
                                    errorOutputBuilder.AppendLine(e.Data);
                                }
                            };

                        var started = process.Start();
                        if (!started)
                        {
                            return new CompilerOutput(1, "Could not start compiler.");
                        }

                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();

                        var exited = process.WaitForExit(GlobalConstants.DefaultProcessExitTimeOutMilliseconds);
                        if (!exited)
                        {
                            process.CancelOutputRead();
                            process.CancelErrorRead();

                            // Double check if the process has exited before killing it
                            if (!process.HasExited)
                            {
                                process.Kill();
                            }

                            return new CompilerOutput(1, "Compiler process timed out.");
                        }

                        outputWaitHandle.WaitOne(300);
                        errorWaitHandle.WaitOne(300);
                        exitCode = process.ExitCode;
                    }
                }
            }

            var output = outputBuilder.ToString().Trim();
            var errorOutput = errorOutputBuilder.ToString().Trim();

            var compilerOutput = $"{output}{Environment.NewLine}{errorOutput}".Trim();
            return new CompilerOutput(exitCode, compilerOutput);
        }
    }
}
