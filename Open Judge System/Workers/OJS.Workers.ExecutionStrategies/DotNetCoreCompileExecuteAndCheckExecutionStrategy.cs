namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;

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

        private readonly string cSharpDotNetCoreCompilerPath;
        private readonly string dotNetCoreSharedAssembliesPath;

        public DotNetCoreCompileExecuteAndCheckExecutionStrategy(
            Func<CompilerType, string> getCompilerPathFunc,
            string cSharpDotNetCoreCompilerPath,
            string dotNetCoreSharedAssembliesPath,
            int baseTimeUsed,
            int baseMemoryUsed)
            : base(baseTimeUsed, baseMemoryUsed)
        {
            this.cSharpDotNetCoreCompilerPath = cSharpDotNetCoreCompilerPath;
            this.dotNetCoreSharedAssembliesPath = dotNetCoreSharedAssembliesPath;
            this.GetCompilerPathFunc = getCompilerPathFunc;
        }

        protected Func<CompilerType, string> GetCompilerPathFunc { get; }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult();

            executionContext.AdditionalCompilerArguments =
                this.BuildAdditionalCompilerArguments(executionContext.AdditionalCompilerArguments);

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

        private string BuildAdditionalCompilerArguments(string additionalArguments)
        {
            var arguments = new StringBuilder();

            // Add the CSharp compiler path followed by '|' in order to extract it in the Compiler
            arguments.Append(this.cSharpDotNetCoreCompilerPath);
            arguments.Append('|');

            // Add all System references
            var references = Directory.GetFiles(this.dotNetCoreSharedAssembliesPath).Where(f => f.Contains("System"));

            foreach (var reference in references)
            {
                arguments.Append($"-r:\"{reference}\" ");
            }

            arguments.Append(additionalArguments);

            return arguments.ToString();
        }
    }
}