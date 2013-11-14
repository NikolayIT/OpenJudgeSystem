namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using OJS.Common.Models;
    using OJS.Workers.Checkers;
    using OJS.Workers.Common;
    using OJS.Workers.Compilers;
    using OJS.Workers.Executors;

    public class CompileExecuteAndCheckExecutionStrategy : ExecutionStrategy
    {
        private readonly Func<CompilerType, string> getCompilerPathFunc;

        public CompileExecuteAndCheckExecutionStrategy(Func<CompilerType, string> getCompilerPathFunc)
        {
            this.getCompilerPathFunc = getCompilerPathFunc;
        }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult();

            // 1. Save source to a file
            var sourceCodeFilePath = this.SaveStringToTempFile(executionContext.Code);

            // 2. Compile the file
            ICompiler compiler = Compiler.CreateCompiler(executionContext.CompilerType);
            var compilerPath = this.getCompilerPathFunc(executionContext.CompilerType);
            var compilerResult = compiler.Compile(compilerPath, sourceCodeFilePath, executionContext.AdditionalCompilerArguments);
            File.Delete(sourceCodeFilePath);

            result.IsCompiledSuccessfully = compilerResult.IsCompiledSuccessfully;
            result.CompilerComment = compilerResult.CompilerComment;

            if (!compilerResult.IsCompiledSuccessfully)
            {
                return result;
            }

            var outputFile = compilerResult.OutputFile;

            // 3. Execute and check each test
            IExecutor executor = new RestrictedProcessExecutor();
            IChecker checker = Checker.CreateChecker(executionContext.CheckerAssemblyName, executionContext.CheckerTypeName, executionContext.CheckerParameter);
            result.TestResults = new List<TestResult>();
            foreach (var test in executionContext.Tests)
            {
                var processExecutionResult = executor.Execute(outputFile, test.Input, executionContext.TimeLimit, executionContext.MemoryLimit);
                var testResult = this.ExecuteAndCheckTest(test, processExecutionResult, checker, processExecutionResult.ReceivedOutput);
                result.TestResults.Add(testResult);
            }

            // 4. Clean our mess
            File.Delete(outputFile);

            return result;
        }
    }
}
