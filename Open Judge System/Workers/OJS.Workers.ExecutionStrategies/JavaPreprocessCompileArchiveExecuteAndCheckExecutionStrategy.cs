namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Workers.Checkers;
    using OJS.Workers.Common;
    using OJS.Workers.Executors;

    public class JavaPreprocessCompileArchiveExecuteAndCheckExecutionStrategy : ExecutionStrategy
    {
        private const string JavaCompiledFileExtension = ".class";
        private const string JavaSourceFilesPattern = "*.java";
        private const string ArchivedFileName = "_submission.jar";
        private const string PackageNameRegEx = @"\bpackage\s+[a-zA-Z_][a-zA-Z_.0-9]{0,150}\s*;";
        private const string ClassNameRegEx = @"public\s+class\s+([a-zA-Z_][a-zA-Z_0-9]{0,50})\s*{";
        private const string TimeMeasurementFileName = "_time.txt";

        private readonly string javaExecutablePath;
        private readonly string javaArchiverPath;
        private readonly string sandboxExecutorSourceFilePath;
        private readonly string workingDirectory;
        private readonly Func<CompilerType, string> getCompilerPathFunc;

        public JavaPreprocessCompileArchiveExecuteAndCheckExecutionStrategy(
            string javaExecutablePath,
            string javaArchiverPath,
            string sandboxExecutorSourceFilePath,
            Func<CompilerType, string> getCompilerPathFunc)
        {
            if (!File.Exists(javaExecutablePath))
            {
                throw new ArgumentException(
                    string.Format("Java not found in: {0}!", javaExecutablePath),
                    "javaExecutablePath");
            }

            if (!File.Exists(javaArchiverPath))
            {
                throw new ArgumentException(
                    string.Format("Java archiver not found in: {0}!", javaArchiverPath),
                    "javaArchiverPath");
            }

            if (!File.Exists(sandboxExecutorSourceFilePath))
            {
                throw new ArgumentException(
                    string.Format("Sandbox executor source file not found in: {0}!", sandboxExecutorSourceFilePath),
                    "sandboxExecutorSourceFilePath");
            }

            this.javaExecutablePath = javaExecutablePath;
            this.javaArchiverPath = javaArchiverPath;
            this.sandboxExecutorSourceFilePath = sandboxExecutorSourceFilePath;
            this.workingDirectory = FileHelpers.CreateTempDirectory();
            this.getCompilerPathFunc = getCompilerPathFunc;
        }

        ~JavaPreprocessCompileArchiveExecuteAndCheckExecutionStrategy()
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

            // Copy the sandbox executor source file to the working directory
            var sandboxExecutorSourceFilePathLastIndexOfSlash = this.sandboxExecutorSourceFilePath.LastIndexOf('\\');
            var sandboxExecutorFileName = this.sandboxExecutorSourceFilePath.Substring(sandboxExecutorSourceFilePathLastIndexOfSlash + 1);

            var workingSandboxExecutorSourceFilePath = string.Format("{0}\\{1}", this.workingDirectory, sandboxExecutorFileName);
            File.Copy(this.sandboxExecutorSourceFilePath, workingSandboxExecutorSourceFilePath);

            // Compile all source files - sandbox executor and submission file(s)
            var compilerPath = this.getCompilerPathFunc(executionContext.CompilerType);
            var sourceFilesToCompile = new DirectoryInfo(this.workingDirectory).GetFiles(JavaSourceFilesPattern).Select(x => x.FullName);
            var compilerResult = this.CompileSourceFiles(
                executionContext.CompilerType,
                compilerPath,
                executionContext.AdditionalCompilerArguments,
                sourceFilesToCompile);

            // Assign compiled result info to the execution result
            result.IsCompiledSuccessfully = compilerResult.IsCompiledSuccessfully;
            result.CompilerComment = compilerResult.CompilerComment;
            if (!result.IsCompiledSuccessfully)
            {
                return result;
            }

            // Archive all compiled into a jar file
            try
            {
                this.ArchiveCompiledFilesIntoJarFile();
            }
            catch (Exception exception)
            {
                result.IsCompiledSuccessfully = false;
                result.CompilerComment = exception.Message;

                return result;
            }

            // Prepare execution process arguments and time measurement info
            var classPathWithJarFile = string.Format("-classpath \"{0}\\{1}\"", this.workingDirectory, ArchivedFileName);

            var sandboxExecutorFileNameLastIndexOfDot = sandboxExecutorFileName.LastIndexOf('.');
            var sandboxExecutorClassName = sandboxExecutorFileName.Substring(0, sandboxExecutorFileNameLastIndexOfDot);

            var submissionFilePathLastIndexOfSlash = submissionFilePath.LastIndexOf('\\');
            var submissionFilePathLastIndexOfDot = submissionFilePath.LastIndexOf('.');
            var classToExecute = submissionFilePath.Substring(
                submissionFilePathLastIndexOfSlash + 1,
                submissionFilePathLastIndexOfDot - submissionFilePathLastIndexOfSlash - 1);

            var timeMeasurementFilePath = string.Format("{0}\\{1}", this.workingDirectory, TimeMeasurementFileName);

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
                    new[] { classPathWithJarFile, sandboxExecutorClassName, classToExecute, string.Format("\"{0}\"", timeMeasurementFilePath) });

                UpdateExecutionTime(timeMeasurementFilePath, processExecutionResult, executionContext.TimeLimit);

                var testResult = this.ExecuteAndCheckTest(test, processExecutionResult, checker, processExecutionResult.ReceivedOutput);
                result.TestResults.Add(testResult);
            }

            return result;
        }

        private static void UpdateExecutionTime(string timeMeasurementFilePath, ProcessExecutionResult processExecutionResult, int timeLimit)
        {
            if (File.Exists(timeMeasurementFilePath))
            {
                long timeInNanoseconds;
                var timeMeasurementFileContent = File.ReadAllText(timeMeasurementFilePath);
                if (long.TryParse(timeMeasurementFileContent, out timeInNanoseconds))
                {
                    processExecutionResult.TimeWorked = TimeSpan.FromMilliseconds((double)timeInNanoseconds / 1000000);

                    if (processExecutionResult.Type == ProcessExecutionResultType.TimeLimit &&
                        processExecutionResult.TimeWorked.TotalMilliseconds <= timeLimit)
                    {
                        // The time from the time measurement file is under the time limit
                        processExecutionResult.Type = ProcessExecutionResultType.Success;
                    }
                }

                File.Delete(timeMeasurementFilePath);
            }
        }

        private string CreateSubmissionFile(string submissionCode)
        {
            // Remove existing packages
            submissionCode = Regex.Replace(submissionCode, PackageNameRegEx, string.Empty);

            // TODO: Allow not always to user public class
            var classNameMatch = Regex.Match(submissionCode, ClassNameRegEx);
            if (!classNameMatch.Success)
            {
                throw new ArgumentException("No valid public class found!");
            }

            var className = classNameMatch.Groups[1].Value;
            var submissionFilePath = string.Format("{0}\\{1}.{2}", this.workingDirectory, className, CompilerType.Java.GetFileExtension());

            File.WriteAllText(submissionFilePath, submissionCode);

            return submissionFilePath;
        }

        private CompileResult CompileSourceFiles(CompilerType compilerType, string compilerPath, string compilerArguments, IEnumerable<string> sourceFilesToCompile)
        {
            CompileResult compilerResult = null;
            foreach (var sourceFile in sourceFilesToCompile)
            {
                compilerResult = this.Compile(compilerType, compilerPath, compilerArguments, sourceFile);

                if (!compilerResult.IsCompiledSuccessfully)
                {
                    break; // The compilation of other files is not necessary
                }
            }

            return compilerResult;
        }

        private void ArchiveCompiledFilesIntoJarFile()
        {
            var arguments = string.Format("cf {0} *{1}", ArchivedFileName, JavaCompiledFileExtension);

            var processStartInfo = new ProcessStartInfo(this.javaArchiverPath)
            {
                Arguments = arguments,
                RedirectStandardError = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = this.workingDirectory
            };

            using (var process = Process.Start(processStartInfo))
            {
                if (process == null)
                {
                    throw new Exception("Could not start Java archiver!");
                }

                var errorMessage = process.StandardError.ReadToEnd();
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    throw new Exception(errorMessage);
                }

                process.WaitForExit();
            }
        }
    }
}
