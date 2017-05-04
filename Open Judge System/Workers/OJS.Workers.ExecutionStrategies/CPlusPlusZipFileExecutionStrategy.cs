namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Workers.Checkers;
    using OJS.Workers.Executors;

    public class CPlusPlusZipFileExecutionStrategy : ExecutionStrategy
    {
        private const string SubmissionName = "UserSubmission.zip";
        private const string FileNameAndExtensionPattern = @"//((\w+)\.(cpp|h))//";

        private readonly Func<CompilerType, string> getCompilerPathFunc;

        public CPlusPlusZipFileExecutionStrategy(Func<CompilerType, string> getCompilerPath)
        {
            this.getCompilerPathFunc = getCompilerPath;
            this.WorkingDirectory = DirectoryHelpers.CreateTempDirectory();
        }

        ~CPlusPlusZipFileExecutionStrategy()
        {
            DirectoryHelpers.SafeDeleteDirectory(this.WorkingDirectory, true);
        }

        public string WorkingDirectory { get; set; }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult();

            var submissionDestination = $@"{this.WorkingDirectory}\{SubmissionName}";

            File.WriteAllBytes(submissionDestination, executionContext.FileContent);
            FileHelpers.RemoveFilesFromZip(submissionDestination, RemoveMacFolderPattern);

            if (!string.IsNullOrEmpty(executionContext.TaskSkeletonAsString))
            {
                var pathsOfHeadersAndCppFiles = this.ExtractTaskSkeleton(executionContext.TaskSkeletonAsString);
                FileHelpers.AddFilesToZipArchive(submissionDestination, string.Empty, pathsOfHeadersAndCppFiles.ToArray());
            }

            var compilerPath = this.getCompilerPathFunc(executionContext.CompilerType);

            var compilationResult = this.Compile(
                executionContext.CompilerType,
                compilerPath,
                executionContext.AdditionalCompilerArguments,
                submissionDestination);

            if (!compilationResult.IsCompiledSuccessfully)
            {
                return result;
            }

            result.IsCompiledSuccessfully = true;

            var executor = new RestrictedProcessExecutor();
            var checker = Checker.CreateChecker(
                executionContext.CheckerAssemblyName,
                executionContext.CheckerTypeName,
                executionContext.CheckerParameter);

            foreach (var test in executionContext.Tests)
            {
                var processExecutionResult = executor.Execute(
                    compilationResult.OutputFile,
                    test.Input,
                    executionContext.TimeLimit,
                    executionContext.MemoryLimit);

                var testResults = this.ExecuteAndCheckTest(
                    test,
                    processExecutionResult,
                    checker,
                    processExecutionResult.ReceivedOutput);

                result.TestResults.Add(testResults);
            }

            return result;
        }

        private IEnumerable<string> ExtractTaskSkeleton(string executionContextTaskSkeletonAsString)
        {
            var headersAndCppFiles = executionContextTaskSkeletonAsString.Split(
                new string[] { GlobalConstants.ClassDelimiter },
                StringSplitOptions.RemoveEmptyEntries);

            var pathsToHeadersAndCppFiles = new List<string>();
            var fileNameAndExtensionMatcher = new Regex(FileNameAndExtensionPattern);

            foreach (var headersOrCppFile in headersAndCppFiles)
            {
                var match = fileNameAndExtensionMatcher.Match(headersOrCppFile);
                if (match.Success)
                {
                    File.WriteAllText($@"{this.WorkingDirectory}\{match.Groups[1]}", headersOrCppFile);
                    pathsToHeadersAndCppFiles.Add($@"{this.WorkingDirectory}\{match.Groups[1]}");
                }
            }

            return pathsToHeadersAndCppFiles;
        }
    }
}
