namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.IO;

    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Workers.Checkers;
    using OJS.Workers.Common;
    using OJS.Workers.Compilers;

    public abstract class ExecutionStrategy : IExecutionStrategy
    {
        protected const string RemoveMacFolderPattern = "__MACOSX/*";

        public abstract ExecutionResult Execute(ExecutionContext executionContext);

        protected ExecutionResult CompileExecuteAndCheck(ExecutionContext executionContext, Func<CompilerType, string> getCompilerPathFunc, IExecutor executor)
        {
            var result = new ExecutionResult();

            // Compile the file
            var compilerResult = this.ExecuteCompiling(executionContext, getCompilerPathFunc, result);
            if (!compilerResult.IsCompiledSuccessfully)
            {
                return result;
            }

            var outputFile = compilerResult.OutputFile;

            // Execute and check each test
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

        protected TestResult ExecuteAndCheckTest(TestContext test, ProcessExecutionResult processExecutionResult, IChecker checker, string receivedOutput)
        {
            var testResult = new TestResult
            {
                Id = test.Id,
                TimeUsed = (int)processExecutionResult.TimeWorked.TotalMilliseconds,
                MemoryUsed = (int)processExecutionResult.MemoryUsed,
            };

            if (processExecutionResult.Type == ProcessExecutionResultType.RunTimeError)
            {
                testResult.ResultType = TestRunResultType.RunTimeError;
                testResult.ExecutionComment = processExecutionResult.ErrorOutput.MaxLength(2048); // Trimming long error texts
            }
            else if (processExecutionResult.Type == ProcessExecutionResultType.TimeLimit)
            {
                testResult.ResultType = TestRunResultType.TimeLimit;
            }
            else if (processExecutionResult.Type == ProcessExecutionResultType.MemoryLimit)
            {
                testResult.ResultType = TestRunResultType.MemoryLimit;
            }
            else if (processExecutionResult.Type == ProcessExecutionResultType.Success)
            {
                var checkerResult = checker.Check(test.Input, receivedOutput, test.Output, test.IsTrialTest);
                testResult.ResultType = checkerResult.IsCorrect ? TestRunResultType.CorrectAnswer : TestRunResultType.WrongAnswer;

                // TODO: Do something with checkerResult.ResultType
                testResult.CheckerDetails = checkerResult.CheckerDetails;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(processExecutionResult), "Invalid ProcessExecutionResultType value.");
            }

            return testResult;
        }

        protected CompileResult ExecuteCompiling(ExecutionContext executionContext, Func<CompilerType, string> getCompilerPathFunc, ExecutionResult result)
        {
            var submissionFilePath = string.IsNullOrEmpty(executionContext.AllowedFileExtensions)
                                         ? FileHelpers.SaveStringToTempFile(executionContext.Code)
                                         : FileHelpers.SaveByteArrayToTempFile(executionContext.FileContent);

            var compilerPath = getCompilerPathFunc(executionContext.CompilerType);
            var compilerResult = this.Compile(executionContext.CompilerType, compilerPath, executionContext.AdditionalCompilerArguments, submissionFilePath);

            result.IsCompiledSuccessfully = compilerResult.IsCompiledSuccessfully;
            result.CompilerComment = compilerResult.CompilerComment;
            return compilerResult;
        }

        protected virtual CompileResult Compile(CompilerType compilerType, string compilerPath, string compilerArguments, string submissionFilePath)
        {
            if (compilerType == CompilerType.None)
            {
                return new CompileResult(true, null) { OutputFile = submissionFilePath };
            }

            if (!File.Exists(compilerPath))
            {
                throw new ArgumentException($"Compiler not found in: {compilerPath}", nameof(compilerPath));
            }

            ICompiler compiler = Compiler.CreateCompiler(compilerType);
            var compilerResult = compiler.Compile(compilerPath, submissionFilePath, compilerArguments);

            if (File.Exists(submissionFilePath))
            {
                File.Delete(submissionFilePath);
            }

            return compilerResult;
        }
    }
}
