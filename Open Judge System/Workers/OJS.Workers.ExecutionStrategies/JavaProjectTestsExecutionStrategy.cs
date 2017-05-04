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

    public class JavaProjectTestsExecutionStrategy : JavaUnitTestsExecutionStrategy
    {
        public JavaProjectTestsExecutionStrategy(
            string javaExecutablePath,
            Func<CompilerType, string> getCompilerPathFunc,
            string javaLibsPath)
            : base(javaExecutablePath, getCompilerPathFunc, javaLibsPath)
        {
            this.UserClassNames = new List<string>();
            this.ClassPath = $@" -classpath ""{this.WorkingDirectory};{this.JavaLibsPath}*""";
        }

        protected List<string> UserClassNames { get; }

        protected override string JUnitTestRunnerCode
        {
            get
            {
                return $@"
import org.junit.runner.JUnitCore;
import org.junit.runner.Result;
import org.junit.runner.notification.Failure;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.InputStream;
import java.io.PrintStream;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
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

        InputStream originalIn = System.in;
        PrintStream originalOut = System.out;

        InputStream in = new ByteArrayInputStream(new byte[0]);
        System.setIn(in);

        ByteArrayOutputStream out = new ByteArrayOutputStream();
        System.setOut(new PrintStream(out));

        List<Result> results = new ArrayList<>();
        for (Class testClass: testClasses) {{
            results.add(JUnitCore.runClasses(testClass));
        }}

        System.setIn(originalIn);
        System.setOut(originalOut);

        for (Result result : results){{
            for (Failure failure : result.getFailures()) {{
                String failureClass = failure.getDescription().getTestClass().getSimpleName();
                String failureException = failure.getException().toString().replaceAll(""\r"", ""\\\\r"").replaceAll(""\n"",""\\\\n"");
                System.out.printf(""%s %s%s"", failureClass, failureException, System.lineSeparator());
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

            var compilerPath = this.GetCompilerPathFunc(executionContext.CompilerType);
            var combinedArguments = executionContext.AdditionalCompilerArguments + this.ClassPath;

            var compilerResult = this.Compile(
                executionContext.CompilerType,
                compilerPath,
                combinedArguments,
                submissionFilePath);

            result.IsCompiledSuccessfully = compilerResult.IsCompiledSuccessfully;
            result.CompilerComment = compilerResult.CompilerComment;
            if (!result.IsCompiledSuccessfully)
            {
                return result;
            }

            var executor = new RestrictedProcessExecutor();
            var checker = Checker.CreateChecker(
                executionContext.CheckerAssemblyName,
                executionContext.CheckerTypeName,
                executionContext.CheckerParameter);

            var arguments = new List<string>();
            arguments.Add(this.ClassPath);
            arguments.Add(AdditionalExecutionArguments);
            arguments.Add(JUnitRunnerClassName);
            arguments.AddRange(this.UserClassNames);

            var processExecutionResult = executor.ExecuteJavaProcess(
                this.JavaExecutablePath,
                string.Empty,
                executionContext.TimeLimit,
                executionContext.MemoryLimit,
                this.WorkingDirectory,
                arguments);

            if (processExecutionResult.ReceivedOutput.Contains(JvmInsufficientMemoryMessage))
            {
                throw new InsufficientMemoryException(JvmInsufficientMemoryMessage);
            }

            var errorsByFiles = this.GetTestErrors(processExecutionResult.ReceivedOutput);
            var testIndex = 0;

            foreach (var test in executionContext.Tests)
            {
                var message = "Test Passed!";
                var testFile = this.TestNames[testIndex++];
                if (errorsByFiles.ContainsKey(testFile))
                {
                    message = errorsByFiles[testFile];
                }

                var testResult = this.ExecuteAndCheckTest(test, processExecutionResult, checker, message);
                result.TestResults.Add(testResult);
            }

            return result;
        }

        protected override string PrepareSubmissionFile(ExecutionContext context)
        {
            var submissionFilePath = $"{this.WorkingDirectory}\\{SubmissionFileName}";
            File.WriteAllBytes(submissionFilePath, context.FileContent);
            FileHelpers.RemoveFilesFromZip(submissionFilePath, RemoveMacFolderPattern);
            this.ExtractUserClassNames(submissionFilePath);
            this.AddTestsToUserSubmission(context, submissionFilePath);
            this.AddTestRunnerTemplate(submissionFilePath);

            return submissionFilePath;
        }

        private void AddTestsToUserSubmission(ExecutionContext context, string submissionZipFilePath)
        {
            var testNumber = 0;
            var filePaths = new string[context.Tests.Count()];

            foreach (var test in context.Tests)
            {
                var className = JavaCodePreprocessorHelper.GetPublicClassName(test.Input);
                var testFileName =
                        $"{this.WorkingDirectory}\\{className}{GlobalConstants.JavaSourceFileExtension}";
                File.WriteAllText(testFileName, test.Input);
                filePaths[testNumber] = testFileName;
                this.TestNames.Add(className);
                testNumber++;
            }

            FileHelpers.AddFilesToZipArchive(submissionZipFilePath, string.Empty, filePaths);
            FileHelpers.DeleteFiles(filePaths);
        }

        private void AddTestRunnerTemplate(string submissionFilePath)
        {
            // It is important to call the JUintTestRunnerCodeTemplate after the TestClasses have been filled
            // otherwise no tests will be queued in the JUnitTestRunner, which would result in no tests failing.
            File.WriteAllText(this.JUnitTestRunnerSourceFilePath, this.JUnitTestRunnerCode);
            FileHelpers.AddFilesToZipArchive(submissionFilePath, string.Empty, this.JUnitTestRunnerSourceFilePath);
            FileHelpers.DeleteFiles(this.JUnitTestRunnerSourceFilePath);
        }

        private void ExtractUserClassNames(string submissionFilePath)
        {
            this.UserClassNames.AddRange(
                FileHelpers.GetFilePathsFromZip(submissionFilePath)
                    .Where(x => !x.EndsWith("/") && x.EndsWith(GlobalConstants.JavaSourceFileExtension))
                    .Select(x => x.Contains(".") ? x.Substring(0, x.LastIndexOf(".", StringComparison.Ordinal)) : x)
                    .Select(x => x.Replace("/", ".")));
        }

        private Dictionary<string, string> GetTestErrors(string receivedOutput)
        {
            var errorsByFiles = new Dictionary<string, string>();
            var output = new StringReader(receivedOutput);
            var line = output.ReadLine();
            while (line != null)
            {
                int firstSpaceIndex = line.IndexOf(" ", StringComparison.Ordinal);
                var fileName = line.Substring(0, firstSpaceIndex);
                var errorMessage = line.Substring(firstSpaceIndex);
                errorsByFiles.Add(fileName, errorMessage);
                line = output.ReadLine();
            }

            return errorsByFiles;
        }
    }
}