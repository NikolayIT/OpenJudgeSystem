namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.IO;

    using OJS.Common.Models;
    using OJS.Workers.Checkers;
    using OJS.Workers.Common;
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

            // Compile the file
            var compilerPath = this.getCompilerPathFunc(executionContext.CompilerType);
            var compilerResult = this.Compile(executionContext.CompilerType, compilerPath, executionContext.AdditionalCompilerArguments, executionContext.Code);
            result.IsCompiledSuccessfully = compilerResult.IsCompiledSuccessfully;
            result.CompilerComment = compilerResult.CompilerComment;
            if (!compilerResult.IsCompiledSuccessfully)
            {
                return result;
            }

            var outputFile = compilerResult.OutputFile;

            // Execute and check each test
            IExecutor executor = new RestrictedProcessExecutor();
            IChecker checker = Checker.CreateChecker(executionContext.CheckerAssemblyName, executionContext.CheckerTypeName, executionContext.CheckerParameter);
            foreach (var test in executionContext.Tests)
            {
                var processExecutionResult = executor.Execute(outputFile, test.Input, executionContext.TimeLimit, executionContext.MemoryLimit);
                var testResult = this.ExecuteAndCheckTest(test, processExecutionResult, checker, processExecutionResult.ReceivedOutput);
                result.TestResults.Add(testResult);
            }

            // Clean our mess
            File.Delete(outputFile);

            return result;
        }
    }
}
