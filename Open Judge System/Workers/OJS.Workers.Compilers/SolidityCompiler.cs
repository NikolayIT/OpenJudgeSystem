namespace OJS.Workers.Compilers
{
    using System;
    using System.Diagnostics;
    using System.IO;

    using OJS.Workers.Common;

    public class SolidityCompiler : Compiler
    {
        public SolidityCompiler(int processExitTimeOutMultiplier)
            : base(processExitTimeOutMultiplier)
        {
        }

        public override string BuildCompilerArguments(
            string inputFile,
            string outputFile,
            string additionalArguments) => $"compile {additionalArguments}";

        public override CompileResult Compile(
            string compilerPath,
            string inputFile,
            string additionalArguments)
        {
            if (compilerPath == null)
            {
                throw new ArgumentNullException(nameof(compilerPath));
            }

            if (!File.Exists(compilerPath))
            {
                return new CompileResult(false, $"Compiler not found! Searched in: {compilerPath}");
            }

            var arguments = this.BuildCompilerArguments(inputFile, string.Empty, additionalArguments);

            var directoryInfo = new DirectoryInfo(inputFile);

            var processStartInfo = this.SetCompilerProcessStartInfo(compilerPath, directoryInfo, arguments);

            var compilerOutput = ExecuteCompiler(processStartInfo, this.MaxProcessExitTimeOutInMilliseconds);

            if (!compilerOutput.IsSuccessful)
            {
                return new CompileResult(false, $"Compiled file is missing. Compiler output: {compilerOutput.Output}");
            }

            return string.IsNullOrWhiteSpace(compilerOutput.Output)
                ? new CompileResult(string.Empty)
                : new CompileResult(true, compilerOutput.Output, string.Empty);
        }
    }
}