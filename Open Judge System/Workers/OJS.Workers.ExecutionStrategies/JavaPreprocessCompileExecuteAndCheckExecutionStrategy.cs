namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;

    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Workers.Checkers;
    using OJS.Workers.Executors;

    public class JavaPreprocessCompileExecuteAndCheckExecutionStrategy : ExecutionStrategy
    {
        private readonly string javaExecutablePath;
        private readonly string workingDirectory;
        private readonly Func<CompilerType, string> getCompilerPathFunc;

        public JavaPreprocessCompileExecuteAndCheckExecutionStrategy(string javaExecutablePath, Func<CompilerType, string> getCompilerPathFunc)
        {
            if (!File.Exists(javaExecutablePath))
            {
                throw new ArgumentException(string.Format("Java not found in: {0}", javaExecutablePath), "javaExecutablePath");
            }

            this.javaExecutablePath = javaExecutablePath;
            this.workingDirectory = FileHelpers.CreateTempDirectory();
            this.getCompilerPathFunc = getCompilerPathFunc;
        }

        ~JavaPreprocessCompileExecuteAndCheckExecutionStrategy()
        {
            if (Directory.Exists(this.workingDirectory))
            {
                Directory.Delete(this.workingDirectory, true);
            }
        }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult();

            // Create a temp file with the submission code
            string submissionFilePath;
            try
            {
                submissionFilePath = this.CreateSubmissionFile(executionContext.Code);
            }
            catch (ArgumentException exception)
            {
                result.IsCompiledSuccessfully = false;
                result.CompilerComment = exception.Message;
                return result;
            }

            // Compile the submission code
            var compilerPath = this.getCompilerPathFunc(executionContext.CompilerType);
            var compilerResult = this.Compile(executionContext.CompilerType, compilerPath, executionContext.AdditionalCompilerArguments, submissionFilePath);

            // Assign compiled result info to the execution result
            result.IsCompiledSuccessfully = compilerResult.IsCompiledSuccessfully;
            result.CompilerComment = compilerResult.CompilerComment;
            if (!result.IsCompiledSuccessfully)
            {
                return result;
            }

            // Create an executor and a checker
            var executor = new StandardProcessExecutor();
            var checker = Checker.CreateChecker(executionContext.CheckerAssemblyName, executionContext.CheckerTypeName, executionContext.CheckerParameter);

            // Process the submission and check each test
            foreach (var test in executionContext.Tests)
            {
                var processExecutionResult = executor.Execute(
                    this.javaExecutablePath,
                    test.Input,
                    executionContext.TimeLimit,
                    executionContext.MemoryLimit,
                    new[] { submissionFilePath });

                var testResult = this.ExecuteAndCheckTest(test, processExecutionResult, checker, processExecutionResult.ReceivedOutput);
                result.TestResults.Add(testResult);
            }

            var outputFile = compilerResult.OutputFile;
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }

            return result;
        }

        private string CreateSubmissionFile(string submissionCode)
        {
            // TODO: Improve not to use public class
            var classNameRegEx = new Regex(@"public class \w+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            var classNameMatch = classNameRegEx.Match(submissionCode);
            if (!classNameMatch.Success)
            {
                throw new ArgumentException("No public class found!", "submissionCode");
            }

            var className = classNameMatch.Value.Substring(classNameMatch.Value.LastIndexOf(' ') + 1).Trim();

            var submissionFilePath = new FileInfo(string.Format("{0}\\{1}", this.workingDirectory, className)).FullName;
            File.WriteAllText(submissionFilePath, submissionCode);

            return submissionFilePath;
        }
    }
}
