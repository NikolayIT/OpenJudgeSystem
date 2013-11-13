namespace OJS.Workers.LocalWorker
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using OJS.Common.Models;
    using OJS.Workers.Common;
    using OJS.Workers.Compilers;

    public class SubmissionsExecutorResult
    {
        public bool IsCompiledSuccessfully { get; set; }

        public string CompilerComment { get; set; }
        // public Submission Submission { get; set; }
        // public List<TestRun> TestRuns { get; set; }
    }

    public class SubmissionsExecutorContext
    {
        public CompilerType CompilerType { get; set; }

        public string AdditionalCompilerArguments { get; set; }

        public string Code { get; set; }
    }

    public class SubmissionsExecutor
    {
        public SubmissionsExecutorResult Execute(SubmissionsExecutorContext submission)
        {
            var result = new SubmissionsExecutorResult();

            // 1. Save source to a file
            var sourceCodeFilePath = this.SaveStringToTempFile(submission.Code);

            // 2. Compile the file
            ICompiler compiler = Compiler.GetCompiler(submission.CompilerType);
            var compilerPath = this.GetCompilerPath(submission.CompilerType);
            var compilerResult = compiler.Compile(compilerPath, sourceCodeFilePath, submission.AdditionalCompilerArguments);
            if (!compilerResult.IsCompiledSuccessfully)
            {
                result.IsCompiledSuccessfully = false;
                result.CompilerComment = compilerResult.CompilerComment;

                return result;
            }

            var outputFile = compilerResult.OutputFile;

            // 3. Execute each test

            // 4. Check results for each test
            // 5. Clean file
            // 6. Return results

            throw new NotImplementedException();
        }

        private string SaveStringToTempFile(string stringToWrite)
        {
            var code = stringToWrite;
            var tempFilePath = Path.GetTempFileName();
            File.WriteAllText(tempFilePath, code);
            return tempFilePath;
        }
        
        private string GetCompilerPath(CompilerType type)
        {
            switch (type)
            {
                case CompilerType.None:
                    return null;
                case CompilerType.CSharp:
                    return Settings.CSharpCompilerPath;
                case CompilerType.MsBuild:
                    throw new NotImplementedException("Compiler not supported.");
                case CompilerType.CPlusPlus:
                    return Settings.CPlusPlusCompilerPath;
                case CompilerType.JavaScript:
                    return null;
                case CompilerType.Java:
                    throw new NotImplementedException("Compiler not supported.");
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }
    }
}
