namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.IO;
    using System.Linq;

    using OJS.Common.Models;
    using OJS.Workers.Checkers;
    using OJS.Workers.Executors;

    public class DotNetCoreCompileExecuteAndCheckExecutionStrategy : ExecutionStrategy
    {
        private const string RuntimeConfigJsonTemplate = @"
            {
	            ""runtimeOptions"": {
                    ""framework"": {
                        ""name"": ""Microsoft.NETCore.App"",
                        ""version"": ""2.0.5""
                    }
                }
            }";

        public DotNetCoreCompileExecuteAndCheckExecutionStrategy(
            Func<CompilerType, string> getCompilerPathFunc,
            int baseTimeUsed,
            int baseMemoryUsed)
            : base(baseTimeUsed, baseMemoryUsed) =>
                this.GetCompilerPathFunc = getCompilerPathFunc;

        protected Func<CompilerType, string> GetCompilerPathFunc { get; }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult();

            // Compile the file
            var compilerResult = this.ExecuteCompiling(executionContext, this.GetCompilerPathFunc, result);
            if (!compilerResult.IsCompiledSuccessfully)
            {
                return result;
            }

            this.CreateRuntimeConfigJsonFile(this.WorkingDirectory, RuntimeConfigJsonTemplate);

            // Execute and check each test
            var executor = new RestrictedProcessExecutor(this.BaseTimeUsed, this.BaseMemoryUsed);

            var checker = Checker.CreateChecker(
                executionContext.CheckerAssemblyName,
                executionContext.CheckerTypeName,
                executionContext.CheckerParameter);

            var arguments = new[]
            {
                compilerResult.OutputFile
            };

            var compilerPath = this.GetCompilerPathFunc(executionContext.CompilerType);

            foreach (var test in executionContext.Tests)
            {
                var processExecutionResult = executor.Execute(
                    compilerPath,
                    test.Input,
                    executionContext.TimeLimit,
                    executionContext.MemoryLimit,
                    arguments,
                    this.WorkingDirectory);

                var testResult = this.ExecuteAndCheckTest(
                    test,
                    processExecutionResult,
                    checker,
                    processExecutionResult.ReceivedOutput);

                result.TestResults.Add(testResult);
            }

            return result;
        }

        private void CreateRuntimeConfigJsonFile(string directory, string text)
        {
            var compiledFileName = Directory
                .GetFiles(directory)
                .Select(Path.GetFileNameWithoutExtension)
                .First();

            var jsonFileName = $"{compiledFileName}.runtimeconfig.json";

            var jsonFilePath = Path.Combine(directory, jsonFileName);

            File.WriteAllText(jsonFilePath, text);
        }
    }
}