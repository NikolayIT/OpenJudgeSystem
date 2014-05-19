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
        private const string SandboxExecutorClassName = "_$SandboxExecutor";

        private const string SandboxExecutorFileName = "_$SandboxExecutor.class";
        private const string SandboxSecurityManagerFileName = "_$SandboxSecurityManager.class";

        private const string SandboxExecutorFilePath = @"C:\Users\Administrator\Desktop\JavaSandBoxExecutorFiles\_$SandboxExecutor.class";
        private const string SandboxSecurityManagerFilePath = @"C:\Users\Administrator\Desktop\JavaSandBoxExecutorFiles\_$SandboxSecurityManager.class";

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

            var localSandboxExecutorFilePath = string.Format("{0}\\{1}", this.workingDirectory, SandboxExecutorFileName);
            var localSandboxSecurityManagerFilePath = string.Format("{0}\\{1}", this.workingDirectory, SandboxSecurityManagerFileName);

            File.Copy(SandboxExecutorFilePath, localSandboxExecutorFilePath, true);
            File.Copy(SandboxSecurityManagerFilePath, localSandboxSecurityManagerFilePath, true);

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
                    new[]
                    {
                        string.Format("-classpath \"{0}\"", this.workingDirectory),
                        SandboxExecutorClassName
                    });

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
            var classNameMatch = Regex.Match(submissionCode, @"public class Program");
            if (!classNameMatch.Success)
            {
                throw new ArgumentException("No 'public class Program' found!");
            }

            var className = classNameMatch.Value.Substring(classNameMatch.Value.LastIndexOf(' ') + 1).Trim();

            var submissionFilePath = new FileInfo(string.Format("{0}\\{1}", this.workingDirectory, className)).FullName;
            File.WriteAllText(submissionFilePath, submissionCode);

            return submissionFilePath;
        }
    }
}
