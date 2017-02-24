namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Workers.Checkers;
    using OJS.Workers.Common.Helpers;
    using OJS.Workers.Executors;

    public class JavaProjectTestsExecutionStrategy : JavaZipFileCompileExecuteAndCheckExecutionStrategy
    {
        protected const string JUnitRunnerClassName = "_$TestRunner";

        public JavaProjectTestsExecutionStrategy(string javaExecutablePath, Func<CompilerType, string> getCompilerPathFunc, string javaLibsPath)
            : base(javaExecutablePath, getCompilerPathFunc)
        {
            if (!Directory.Exists(javaLibsPath))
            {
                throw new ArgumentException(
                    $"Java libraries not found in: {javaLibsPath}",
                    nameof(javaLibsPath));
            }

            this.JavaLibsPath = javaLibsPath;
            this.JUnitTestRunnerSourceFilePath =
                $"{this.WorkingDirectory}\\{JUnitRunnerClassName}{GlobalConstants.JavaSourceFileExtension}";
            this.TestNames = new List<string>();
            this.UserClassNames = new List<string>();
        }

        protected string JavaLibsPath { get; }

        protected string JUnitTestRunnerSourceFilePath { get; }

        protected List<string> TestNames { get; }

        protected List<string> UserClassNames { get; }

        protected string JUnitTestRunnerCode
        {
            get
            {
                return $@"
import org.junit.runner.JUnitCore;
import org.junit.runner.Result;
import org.junit.runner.notification.Failure;

import java.util.HashMap;
import java.util.Map;

public class _$TestRunner {{
    public static void main(String[] args) {{
        for (String arg: args) {{
            try {{
                Class currentClass = Class.forName(arg);
                Classes.allClasses.put(currentClass.getSimpleName(),currentClass);
            }} catch (ClassNotFoundException e) {{}}
        }}

        Class[] testClasses = new Class[]{{{string.Join(", ", this.TestNames.Select(x => x + ".class"))}}};

        for (Class testClass: testClasses) {{
            Result result = JUnitCore.runClasses(testClass);
            for (Failure failure : result.getFailures()) {{
                System.out.printf(""%s %s%s"", failure.getDescription().getTestClass().getSimpleName(), failure.getException(), System.lineSeparator());
            }}
        }}
    }}
}}

class Classes{{
    public static Map<String, Class> allClasses = new HashMap<>();
}}";
            }
        }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult();

            // Create a temp file with the submission code
            string submissionFilePath;
            try
            {
                submissionFilePath = this.CreateSubmissionFile(executionContext);
            }
            catch (ArgumentException exception)
            {
                result.IsCompiledSuccessfully = false;
                result.CompilerComment = exception.Message;

                return result;
            }

            executionContext.AdditionalCompilerArguments = $@"-cp ""{this.JavaLibsPath}*""";
            var compilerResult = this.DoCompile(executionContext, submissionFilePath);

            // Assign compiled result info to the execution result
            result.IsCompiledSuccessfully = compilerResult.IsCompiledSuccessfully;
            result.CompilerComment = compilerResult.CompilerComment;
            if (!result.IsCompiledSuccessfully)
            {
                return result;
            }

            var arguments = new List<string>();

            // Prepare execution process arguments and time measurement info
            var classPathArgument = $@"-classpath ""{this.WorkingDirectory};{this.JavaLibsPath}*""";
            arguments.Add(classPathArgument);

            // Create an executor and a checker
            var executor = new RestrictedProcessExecutor();
            var checker = Checker.CreateChecker(executionContext.CheckerAssemblyName, executionContext.CheckerTypeName, executionContext.CheckerParameter);

            arguments.Add(JUnitRunnerClassName);
            arguments.AddRange(this.UserClassNames);

            // Process the submission and check each test
            var processExecutionResult = executor.ExecuteJavaProcess(
                this.JavaExecutablePath,
                string.Empty,
                executionContext.TimeLimit * 2, // Java virtual machine takes more time to start up
                executionContext.MemoryLimit,
                this.WorkingDirectory,
                arguments);

            var errorsByFiles = this.GetTestErrors(processExecutionResult.ReceivedOutput);
            var testIndex = 0;

            foreach (var test in executionContext.Tests)
            {
                // Construct and figure out what the Test results are
                string message = "Test Passed!";
                string testFile = this.TestNames[testIndex++];
                if (errorsByFiles.ContainsKey(testFile))
                {
                    message = errorsByFiles[testFile];
                }

                var testResult = this.ExecuteAndCheckTest(test, processExecutionResult, checker, message);
                result.TestResults.Add(testResult);
            }

            return result;
        }

        protected override string CreateSubmissionFile(ExecutionContext executionContext)
        {
            var trimmedAllowedFileExtensions = executionContext.AllowedFileExtensions?.Trim();
            var allowedFileExtensions = (!trimmedAllowedFileExtensions?.StartsWith(".") ?? false)
                ? $".{trimmedAllowedFileExtensions}"
                : trimmedAllowedFileExtensions;

            if (allowedFileExtensions != GlobalConstants.ZipFileExtension)
            {
                throw new ArgumentException("Submission file is not a zip file!");
            }

            return this.PrepareSubmissionFile(executionContext);
        }

        private string PrepareSubmissionFile(ExecutionContext context)
        {
            var submissionFilePath = $"{this.WorkingDirectory}\\{SubmissionFileName}";
            File.WriteAllBytes(submissionFilePath, context.FileContent);
            this.ExtractUserClassNames(submissionFilePath);
            this.AddTestsToUserSubmission(context, submissionFilePath);
            this.AddTestRunnerTemplate(submissionFilePath);

            return submissionFilePath;
        }

        private void AddTestsToUserSubmission(ExecutionContext context, string submissionZipFilePath)
        {
            int testNumber = 0;
            string[] filePaths = new string[context.Tests.Count()];

            foreach (var test in context.Tests)
            {
                string className = JavaCodePreprocessorHelper.GetPublicClassName(test.Input);
                var testFileName =
                        $"{this.WorkingDirectory}\\{className}{GlobalConstants.JavaSourceFileExtension}";
                File.WriteAllText(testFileName, test.Input);
                filePaths[testNumber] = testFileName;
                this.TestNames.Add(className);
                testNumber++;
            }

            FileHelpers.AddFilesToZipArchive(submissionZipFilePath, string.Empty, filePaths);
        }

        private void AddTestRunnerTemplate(string submissionFilePath)
        {
            // It is important to call the JUintTestRunnerCodeTemplate after the TestClasses have been filled
            // otherwise no tests will be queued in the JUnitTestRunner, which would result in no tests failing.
            File.WriteAllText(this.JUnitTestRunnerSourceFilePath, this.JUnitTestRunnerCode);
            FileHelpers.AddFileToZipArchive(submissionFilePath, string.Empty, this.JUnitTestRunnerSourceFilePath);
        }

        private void ExtractUserClassNames(string submissionFilePath)
        {
            this.UserClassNames.AddRange(
                FileHelpers.GetFilePathsFromZip(submissionFilePath)
                    .Where(x => !x.EndsWith("/"))
                    .Select(x => x.Contains(".") ? x.Substring(0, x.LastIndexOf(".", StringComparison.Ordinal)) : x)
                    .Select(x => x.Replace("/", ".")));
        }

        private Dictionary<string, string> GetTestErrors(string receivedOutput)
        {
            var errorsByFiles = new Dictionary<string, string>();
            var output = new StringReader(receivedOutput);
            string line = output.ReadLine();
            while (line != null)
            {
                int firstSpaceIndex = line.IndexOf(" ", StringComparison.Ordinal);
                string fileName = line.Substring(0, firstSpaceIndex);
                string errorMessage = line.Substring(firstSpaceIndex);
                errorsByFiles.Add(fileName, errorMessage);
                line = output.ReadLine();
            }

            return errorsByFiles;
        }
    }
}