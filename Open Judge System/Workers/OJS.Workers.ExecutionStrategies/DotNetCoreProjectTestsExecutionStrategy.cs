namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using OJS.Common.Extensions;
    using OJS.Common.Models;

    public class DotNetCoreProjectTestsExecutionStrategy : CSharpProjectTestsExecutionStrategy
    {
        private const string NunitLiteProgramTemplate =
            @"using System;
            using System.Reflection;
            using NUnit.Common;
            using NUnitLite;
            
            public class Program
            {
                public static void Main(string[] args)
                {
                    var writter = new ExtendedTextWrapper(Console.Out);
                    new AutoRun(typeof(Program).GetTypeInfo().Assembly).Execute(args, writter, Console.In);
                }
            }";

        public DotNetCoreProjectTestsExecutionStrategy(Func<CompilerType, string> getCompilerPathFunc) 
            : base(getCompilerPathFunc)
        {
        }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult();

            var userSubmission = executionContext.FileContent;

            this.ExtractFilesInWorkingDirectory(userSubmission);

            var csProjFilePath = this.GetCsProjFilePath();

            this.ExtractTestNames(executionContext.Tests);

            this.WriteTestFiles(executionContext, this.WorkingDirectory);

            var compilerPath = this.GetCompilerPathFunc(executionContext.CompilerType);

            this.CreateNunitLiteConsoleApp(
                compilerPath,
                NunitLiteProgramTemplate,
                this.WorkingDirectory + "\\NunitLiteTests",
                this.TestPaths);

            var compilerResult = this.Compile(
                executionContext.CompilerType,
                compilerPath,
                executionContext.AdditionalCompilerArguments,
                csProjFilePath);

            result.IsCompiledSuccessfully = compilerResult.IsCompiledSuccessfully;
            result.CompilerComment = compilerResult.CompilerComment;

            if (!compilerResult.IsCompiledSuccessfully)
            {
                return result;
            }

            // Delete tests before execution so the user can't access them
            FileHelpers.DeleteFiles(this.TestPaths.ToArray());

            return result;
        }

        private void CreateNunitLiteConsoleApp(string compilerPath, string nUnitLiteProgramTemplate, string workingDirectory, List<string> testPaths)
        {
            // TODO:
        }
    }
}